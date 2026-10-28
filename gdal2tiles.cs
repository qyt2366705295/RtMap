using Renci.SshNet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RtMap
{
    public partial class gdal2tiles : Form
    {
        private FileSystemWatcher _watcher;
        private BlockingCollection<string> _tifQueue;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isMonitoring = false;
        private ConcurrentDictionary<string, bool> _activeProcessingLocks;

        private FileSystemWatcher _syncWatcher;
        private BlockingCollection<string> _syncQueue;
        private CancellationTokenSource _syncCts;
        private bool _isSyncing = false;
        private ConcurrentDictionary<string, bool> _activeSyncLocks = new ConcurrentDictionary<string, bool>();

        public gdal2tiles()
        {
            InitializeComponent();
            this.FormClosing += gdal2tiles_FormClosing;
        }

        private void btnBrowseInput_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "请选择GeoTIFF影像目录";
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtInputDir.Text = fbd.SelectedPath;

                    try
                    {
                        string sessionName = "Task_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                        string autoOutputDir = Path.Combine(AppConfig.Instance.Paths.TileCacheRoot, sessionName);

                        // 自动填入文本框
                        txtOutputDir.Text = autoOutputDir;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"自动生成输出目录失败：{ex.Message}");
                    }
                }
            }
        }

        private void btnBrowseOutput_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "请选择切片的总输出目录";
                if (fbd.ShowDialog() == DialogResult.OK)
                    txtOutputDir.Text = fbd.SelectedPath;
            }
        }

        private void btnGdal2tiles_Click(object sender, EventArgs e)
        {
            if (!_isMonitoring)
            {
                string inputPath = txtInputDir.Text;
                string outputPath = txtOutputDir.Text;
                string remotePath = txtRemoteBaseDir.Text;

                if (string.IsNullOrWhiteSpace(inputPath) || string.IsNullOrWhiteSpace(outputPath) || string.IsNullOrWhiteSpace(remotePath))
                {
                    MessageBox.Show("请确保监控目录、输出目录和服务器上传路径都已填写！");
                    return;
                }

                TaskInfo selectedTask = cboTasks.SelectedItem as TaskInfo;
                bool isManualMode = (selectedTask == null || selectedTask.exec_id.StartsWith("["));
                TaskInfo taskToProcess = isManualMode ? null : selectedTask;

                StartMonitoring(inputPath, outputPath, remotePath, taskToProcess);

                btnGdal2tiles.Text = "停止切片";
                btnGdal2tiles.BackColor = System.Drawing.Color.LightCoral;
                txtInputDir.Enabled = false;
                txtOutputDir.Enabled = false;
                txtRemoteBaseDir.Enabled = false;
                _isMonitoring = true;
            }
            else
            {
                StopMonitoring();

                btnGdal2tiles.Text = "开始切片";
                btnGdal2tiles.BackColor = System.Drawing.Color.LightSkyBlue;
                txtInputDir.Enabled = true;
                txtOutputDir.Enabled = true;
                txtRemoteBaseDir.Enabled = true;
                _isMonitoring = false;
                lblStatus.Text = "已停止监控。";

                string sessionCacheDir = txtOutputDir.Text;
                if (chkAutoClean.Checked && !string.IsNullOrWhiteSpace(sessionCacheDir) && Directory.Exists(sessionCacheDir))
                {
                    try
                    {
                        // 稍微给后台线程一点退出和释放文件的缓冲时间
                        Thread.Sleep(3000);
                        Directory.Delete(sessionCacheDir, true);
                        lblStatus.Text = $"已停止监控，且本地缓存已彻底清理：{Path.GetFileName(sessionCacheDir)}";
                    }
                    catch (Exception ex)
                    {
                        lblStatus.Text = $"已停止监控。警告：缓存清理失败(文件可能仍被占用)，请手动删除。";
                        Console.WriteLine($"清理失败：{ex.Message}");
                    }
                }
            }
        }

        private void StartMonitoring(string inputDir, string outputDir, string remoteBaseDir, TaskInfo task)
        {
            _activeProcessingLocks = new ConcurrentDictionary<string, bool>(); // 初始化锁
            _tifQueue = new BlockingCollection<string>();
            _cancellationTokenSource = new CancellationTokenSource();

            _watcher = new FileSystemWatcher(inputDir, "*.tif");
            _watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
            _watcher.Created += Watcher_FileDetected;
            _watcher.Changed += Watcher_FileDetected;
            _watcher.EnableRaisingEvents = true;

            string[] existingFiles = Directory.GetFiles(inputDir, "*.tif");
            int addedCount = 0;
            foreach (string file in existingFiles)
            {
                if (!_tifQueue.Contains(file))
                {
                    _tifQueue.Add(file);
                    addedCount++;
                }
            }

            lblStatus.Text = $"已加载{addedCount}张影像，正在持续监控目录...";

            Task.Run(() => ProcessTifQueue(outputDir, remoteBaseDir, task, _cancellationTokenSource.Token));
        }

        private void StopMonitoring()
        {
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Dispose();
                _watcher = null;
            }

            if (_tifQueue != null && !_tifQueue.IsAddingCompleted)
            {
                _tifQueue.CompleteAdding();
            }

        }

        private void Watcher_FileDetected(object sender, FileSystemEventArgs e)
        {
            // 如果队列里已经有这个文件了，就不要重复添加
            if (_activeProcessingLocks.TryAdd(e.FullPath, true))
            {
                _tifQueue.Add(e.FullPath);
                UpdateStatusUI($"检测到新影像，已加入队列：{e.Name}");
            }
        }

        private void ProcessTifQueue(string baseOutputDir, string remoteBaseDir, TaskInfo task, CancellationToken token)
        {
            try
            {
                var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 5, CancellationToken = token };
                var partitioner = Partitioner.Create(
                    _tifQueue.GetConsumingEnumerable(token),
                    EnumerablePartitionerOptions.NoBuffering
                );
                Parallel.ForEach(partitioner, parallelOptions, tifPath =>
                {
                    try
                    {
                        string fileName = Path.GetFileName(tifPath);
                        UpdateStatusUI($"正在等待文件解锁：{fileName}...");

                        // 轮询等待底层C++程序彻底写完并释放文件
                        if (WaitUntilFileIsReady(tifPath, 60))
                        {
                            UpdateStatusUI($"开始切片：{fileName}...");

                            // 为每个TIF建立一个独立的同名子文件夹，防止多个TIF瓦片互相覆盖
                            string tileOutputDir = Path.Combine(baseOutputDir, Path.GetFileNameWithoutExtension(tifPath));

                            if (!Directory.Exists(tileOutputDir))
                            {
                                Directory.CreateDirectory(tileOutputDir);
                            }

                            // 调用GDAL命令行切片
                            RunGdal2Tiles(tifPath, tileOutputDir);
                            UpdateStatusUI($"切片完成，正在上传: {fileName}...");
                            UploadTilesToSftp(tileOutputDir, Path.GetFileNameWithoutExtension(tifPath), remoteBaseDir);
                            
                            if (task != null)
                            {
                                UpdateStatusUI($"正在调用UOS回调接口：{fileName}...");
                                UosApiClient.NotifyUosBuildComplete(task.exec_id, task.result_path, Path.GetFileNameWithoutExtension(tifPath)).Wait();
                            }
                            else
                            {
                                Console.WriteLine("当前为手动模式，跳过UOS接口回调。");
                            }

                            UpdateStatusUI($"切片+上传完成：{fileName}，继续监控...");
                        }
                    }
                    finally
                    {
                        _activeProcessingLocks.TryRemove(tifPath, out _);
                    }
                });
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("后台队列处理线程已收到取消指令，安全退出。");
            }
            catch (Exception ex)
            {
                UpdateStatusUI($"队列处理发生错误：{ex.Message}");
            }

        }

        private bool WaitUntilFileIsReady(string filePath, int timeoutSeconds)
        {
            DateTime startTime = DateTime.Now;
            while ((DateTime.Now - startTime).TotalSeconds < timeoutSeconds)
            {
                try
                {
                    // 尝试以Read权限且不共享的方式打开文件
                    using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        return true;
                    }
                }
                catch (IOException)
                {
                    // 文件被占用，1秒后再试
                    Thread.Sleep(200);
                }
            }
            return false;
        }

        private void RunGdal2Tiles(string inputTif, string outputDir)
        {
            try
            {
                // 调用本地Anaconda的Python多线程快速切片
                ProcessStartInfo psi = new ProcessStartInfo();

                string condaBase = AppConfig.Instance.Paths.PythonExe;
                string libraryBin = Path.Combine(condaBase, @"Library\bin");
                string scriptsDir = Path.Combine(condaBase, "Scripts");
                string projLib = Path.Combine(condaBase, @"Library\share\proj");

                string currentPath = Environment.GetEnvironmentVariable("PATH") ?? "";
                psi.EnvironmentVariables["PATH"] = $"{libraryBin};{scriptsDir};{condaBase};{currentPath}";
                psi.EnvironmentVariables["PROJ_LIB"] = projLib;
                psi.EnvironmentVariables["GDAL_DATA"] = Path.Combine(condaBase, @"Library\share\gdal");

                psi.FileName = Path.Combine(condaBase, "python.exe");
                string gdalScriptPath = Path.Combine(condaBase, @"Scripts\gdal2tiles.py");

                psi.Arguments = $"\"{gdalScriptPath}\" -p mercator -w none \"{inputTif}\" \"{outputDir}\"";
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;

                using (Process proc = Process.Start(psi))
                {
                    proc.WaitForExit(); // 阻塞等待这个TIF切完
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"调用GDAL失败：{ex.Message}");
            }
        }

        private void UpdateStatusUI(string msg)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => lblStatus.Text = msg));
            }
            else
            {
                lblStatus.Text = msg;
            }
        }

        private void gdal2tiles_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
            }
            StopMonitoring();
        }

        private void UploadTilesToSftp(string localTileDir, string folderName, string remoteBaseDir)
        {
            try
            {
                var srv = AppConfig.Instance.Server;
                using (var sftp = new SftpClient(srv.Host, srv.Port, srv.Username, srv.Password))
                {
                    sftp.Connect();

                    string remoteTargetDir = remoteBaseDir + "/" + folderName;

                    // 递归上传整个本地切片文件夹到远端
                    UploadDirectoryRecursively(sftp, localTileDir, remoteTargetDir);

                    sftp.Disconnect();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SFTP上传失败 [{folderName}]: {ex.Message}");
                UpdateStatusUI($"上传服务器失败: {folderName} (详情见控制台)");
            }
        }

        private void UploadDirectoryRecursively(SftpClient sftp, string localDir, string remoteDir)
        {
            // 如果远端不存在该目录，则创建
            //if (!sftp.Exists(remoteDir))
            //{
            //    sftp.CreateDirectory(remoteDir);
            //}
            CreateRemoteDirectorySafe(sftp, remoteDir);

            // 上传当前目录下的所有文件(包括png, json, xml)
            foreach (string file in Directory.GetFiles(localDir))
            {
                using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    string remoteFileName = remoteDir + "/" + Path.GetFileName(file);
                    sftp.UploadFile(fs, remoteFileName, true); // true表示覆盖同名文件
                }
            }

            // 递归遍历子目录
            foreach (string subDir in Directory.GetDirectories(localDir))
            {
                string dirName = Path.GetFileName(subDir);
                string newRemoteDir = remoteDir + "/" + dirName;
                UploadDirectoryRecursively(sftp, subDir, newRemoteDir);
            }
        }

        private void CreateRemoteDirectorySafe(SftpClient sftp, string path)
        {
            string currentPath = "";
            string[] folders = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string folder in folders)
            {
                currentPath += "/" + folder;
                if (!sftp.Exists(currentPath))
                {
                    sftp.CreateDirectory(currentPath);
                }
            }
        }

        private void btnRefreshTasks_Click(object sender, EventArgs e)
        {
            RefreshTaskList();
        }

        private void RefreshTaskList()
        {
            // 读取本地桥梁文件中的所有自动化任务
            List<TaskInfo> activeTasks = TaskBridge.GetAllTasks();

            // 核心兼容设计：强行在列表最前面插入一个“手动模式”的虚拟任务
            var MonitorCfg = AppConfig.Instance.Monitor;
            TaskInfo manualModeDummy = new TaskInfo
            {
                exec_id = "[default]",
                result_path = MonitorCfg.DefaultRemotePictureDir + "/results/default"
            };
            activeTasks.Insert(0, manualModeDummy);

            cboTasks.DataSource = null;
            cboTasks.DataSource = activeTasks;
            cboTasks.SelectedIndex = 0;
        }

        private void cboTasks_SelectedIndexChanged(object sender, EventArgs e)
        {
            TaskInfo selectedTask = cboTasks.SelectedItem as TaskInfo;
            if (selectedTask != null)
            {
                if (selectedTask.exec_id.StartsWith("["))
                {
                    txtRemoteBaseDir.Text = selectedTask.result_path;
                    txtRemoteBaseDir.Enabled = true;
                }
                else
                {
                    // 自动将文本框的内容替换为该任务的标准成果路径，并锁死防篡改
                    txtRemoteBaseDir.Text = selectedTask.result_path;
                    txtRemoteBaseDir.Enabled = false;

                    if (!string.IsNullOrWhiteSpace(selectedTask.local_result_path))
                    {
                        txtInputDir.Text = selectedTask.local_result_path;
                        //txtInputDir.Enabled = false;

                        txtOutputDir.Text = Path.Combine(AppConfig.Instance.Paths.TileCacheRoot, "Task_" + selectedTask.exec_id);
                        //txtOutputDir.Enabled = false;
                    }
                    else
                    {
                        txtInputDir.Enabled = true;
                        txtOutputDir.Enabled = true;
                    }
                }
            }
        }

        private void btnFinishTask_Click(object sender, EventArgs e)
        {
            TaskInfo selectedTask = cboTasks.SelectedItem as TaskInfo;

            if (selectedTask == null || selectedTask.exec_id.StartsWith("["))
            {
                MessageBox.Show("请先在下拉框中选择一个流水线任务！");
                return;
            }

            DialogResult result = MessageBox.Show(
                $"确定要完结工程【{selectedTask.exec_id}】吗？\n该任务将从本地彻底清除。",
                "完结确认", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                TaskBridge.RemoveTask(selectedTask.exec_id);
                RefreshTaskList();
                UpdateStatusUI($"任务{selectedTask.exec_id}已清理。");
            }
        }

        private void btnDirectUpload_Click(object sender, EventArgs e)
        {
            if (!_isSyncing)
            {
                TaskInfo selectedTask = cboTasks.SelectedItem as TaskInfo;
                bool isManualMode = (selectedTask == null || selectedTask.exec_id.StartsWith("["));
                TaskInfo taskToProcess = isManualMode ? null : selectedTask;

                string remotePath = txtRemoteBaseDir.Text.Replace("\\", "/").TrimEnd('/');
                if (string.IsNullOrWhiteSpace(remotePath))
                {
                    MessageBox.Show("请确保服务器上传路径已填写！");
                    return;
                }

                using (FolderBrowserDialog fbd = new FolderBrowserDialog())
                {
                    fbd.Description = "请选择需要实时同步的成果根目录";
                    if (fbd.ShowDialog() == DialogResult.OK)
                    {
                        string localSyncDir = fbd.SelectedPath;
                        string folderName = Path.GetFileName(localSyncDir);
                        string finalRemoteDir = remotePath + "/" + folderName;

                        // 调用实时监控启动函数
                        StartDirectorySync(localSyncDir, finalRemoteDir, taskToProcess);

                        // 按钮状态变为“停止”
                        btnDirectUpload.Text = "停止上传成果";
                        btnDirectUpload.BackColor = System.Drawing.Color.LightCoral;
                        _isSyncing = true;
                    }
                }
            }
            else
            {
                // 停止监控
                StopDirectorySync();

                // 按钮状态恢复
                btnDirectUpload.Text = "直接上传成果";
                btnDirectUpload.BackColor = System.Drawing.Color.LightSkyBlue;
                _isSyncing = false;
                UpdateStatusUI("已停止实时同步。");
            }
        }

        private void StartDirectorySync(string localDir, string remoteTargetDir, TaskInfo task)
        {
            _syncQueue = new BlockingCollection<string>();
            _syncCts = new CancellationTokenSource();
            _activeSyncLocks.Clear();

            _syncWatcher = new FileSystemWatcher(localDir);
            _syncWatcher.IncludeSubdirectories = true; // 监控Mip生成的所有嵌套子文件夹
            _syncWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
            _syncWatcher.Created += SyncWatcher_FileDetected;
            _syncWatcher.Changed += SyncWatcher_FileDetected;
            _syncWatcher.EnableRaisingEvents = true;

            UpdateStatusUI($"正在实时同步监控目录：{Path.GetFileName(localDir)}...");

            // 启动后台消费线程
            Task.Run(() => ProcessSyncQueue(localDir, remoteTargetDir, task, _syncCts.Token));
        }

        private void StopDirectorySync()
        {
            if (_syncWatcher != null)
            {
                _syncWatcher.EnableRaisingEvents = false;
                _syncWatcher.Dispose();
                _syncWatcher = null;
            }
            if (_syncQueue != null && !_syncQueue.IsAddingCompleted)
            {
                _syncQueue.CompleteAdding();
            }
            if (_syncCts != null && !_syncCts.IsCancellationRequested)
            {
                _syncCts.Cancel();
            }
        }

        private void SyncWatcher_FileDetected(object sender, FileSystemEventArgs e)
        {
            // 忽略文件夹本身的变动，只抓取文件
            if (Directory.Exists(e.FullPath)) return;

            if (_activeSyncLocks.TryAdd(e.FullPath, true))
            {
                _syncQueue.Add(e.FullPath);
            }
        }

        private void ProcessSyncQueue(string localBaseDir, string remoteBaseDir, TaskInfo task, CancellationToken token)
        {
            // API防抖计时器（防止Mip一秒生成10个文件，触发10次回调）
            DateTime lastUploadTime = DateTime.MinValue;
            bool apiCallbackPending = false;

            try
            {
                var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 5, CancellationToken = token };
                var partitioner = Partitioner.Create(_syncQueue.GetConsumingEnumerable(token), EnumerablePartitionerOptions.NoBuffering);

                Parallel.ForEach(partitioner, parallelOptions, filePath =>
                {
                    try
                    {
                        if (WaitUntilFileIsReady(filePath, 60))
                        {
                            string relativePath = filePath.Substring(localBaseDir.Length).TrimStart('\\', '/');
                            string remoteFilePath = remoteBaseDir + "/" + relativePath.Replace('\\', '/');
                            string remoteFileDir = remoteFilePath.Substring(0, remoteFilePath.LastIndexOf('/'));

                            UpdateStatusUI($"正在同步上传：{Path.GetFileName(filePath)}...");
                            UploadSingleFileWithTargetDir(filePath, remoteFileDir, remoteFilePath);

                            // 记录最后上传时间，标记需要回调
                            lock (_activeSyncLocks)
                            {
                                lastUploadTime = DateTime.Now;
                                apiCallbackPending = true;
                            }
                        }
                    }
                    finally
                    {
                        _activeSyncLocks.TryRemove(filePath, out _);
                    }
                });
            }
            catch (OperationCanceledException) { }

            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    bool shouldFireApi = false;

                    lock (_activeSyncLocks)
                    {
                        if (apiCallbackPending)
                        {
                            apiCallbackPending = false;
                            shouldFireApi = true;
                        }
                    }

                    if (shouldFireApi && task != null)
                    {
                        UpdateStatusUI($"触发UOS回调，通知前端刷新最新成果...");

                        string folderName = Path.GetFileName(localBaseDir);
                        await UosApiClient.NotifyUosBuildComplete(task.exec_id, task.result_path, folderName);
                        UpdateStatusUI($"前端回调已发送，继续保持监控...");
                    }

                    try
                    {
                        // 每几百毫秒回调一次
                        await Task.Delay(500, token);
                    }
                    catch (TaskCanceledException)
                    {
                        break; // 收到取消信号，安全退出
                    }
                }
            }, token);
        }

        private void UploadSingleFileWithTargetDir(string localFilePath, string remoteTargetDir, string remoteFilePath)
        {

            try
            {
                var srv = AppConfig.Instance.Server;
                using (var sftp = new SftpClient(srv.Host, srv.Port, srv.Username, srv.Password))
                {
                    sftp.Connect();
                    CreateRemoteDirectorySafe(sftp, remoteTargetDir); // 确保多级子目录存在
                    using (FileStream fs = new FileStream(localFilePath, FileMode.Open, FileAccess.Read))
                    {
                        sftp.UploadFile(fs, remoteFilePath, true); // true 代表暴力覆盖旧文件
                    }
                    sftp.Disconnect();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SFTP同步失败[{localFilePath}]：{ex.Message}");
            }
        }

        private void UploadSingleFileToSftp(string localFilePath, string remoteTargetDir)
        {

            try
            {
                var srv = AppConfig.Instance.Server;
                using (var sftp = new SftpClient(srv.Host, srv.Port, srv.Username, srv.Password))
                {
                    sftp.Connect();

                    // 确保远端目标目录存在
                    CreateRemoteDirectorySafe(sftp, remoteTargetDir);

                    // 拼接远端的最终文件路径
                    string fileName = Path.GetFileName(localFilePath);
                    string remoteFilePath = remoteTargetDir + "/" + fileName;

                    // 执行单文件强制覆盖上传
                    using (FileStream fs = new FileStream(localFilePath, FileMode.Open, FileAccess.Read))
                    {
                        sftp.UploadFile(fs, remoteFilePath, true);
                    }

                    sftp.Disconnect();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SFTP单文件上传失败[{localFilePath}]：{ex.Message}");
                throw;
            }
        }

        private void btnDirectUploadFile_Click(object sender, EventArgs e)
        {
            TaskInfo selectedTask = cboTasks.SelectedItem as TaskInfo;
            bool isManualMode = (selectedTask == null || selectedTask.exec_id.StartsWith("["));
            TaskInfo taskToProcess = isManualMode ? null : selectedTask;

            // 获取要上传的远端目标路径
            string remotePath = txtRemoteBaseDir.Text.Replace("\\", "/").TrimEnd('/');
            if (string.IsNullOrWhiteSpace(remotePath))
            {
                MessageBox.Show("请确保服务器上传路径已填写！");
                return;
            }

            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "请选择要上传的文件";
                ofd.Filter = "所有文件 (*.*)|*.*|JSON文件 (*.json)|*.json|影像文件 (*.tif;*.jpg;*.png)|*.tif;*.jpg;*.png";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string localFilePath = ofd.FileName;
                    string fileName = Path.GetFileName(localFilePath); // 仅提取文件名

                    btnDirectUploadFile.Enabled = false;
                    UpdateStatusUI($"准备上传单文件：{fileName}...");

                    Task.Run(async () =>
                    {
                        try
                        {
                            UpdateStatusUI($"正在执行单文件上传: {fileName}...");

                            UploadSingleFileToSftp(localFilePath, remotePath);

                            // 判断是否需要执行回调
                            if (taskToProcess != null)
                            {
                                UpdateStatusUI($"正在调用UOS回调接口：{fileName}...");
                                await UosApiClient.NotifyUosBuildComplete(taskToProcess.exec_id, taskToProcess.result_path, fileName);
                            }
                            else
                            {
                                Console.WriteLine("跳过UOS接口回调。");
                            }

                            UpdateStatusUI($"单文件上传完毕：{fileName}！");
                        }
                        catch (Exception ex)
                        {
                            UpdateStatusUI($"单文件上传发生异常：{ex.Message}");
                        }
                        finally
                        {
                            if (this.InvokeRequired)
                                this.Invoke(new Action(() => btnDirectUploadFile.Enabled = true));
                            else
                                btnDirectUploadFile.Enabled = true;
                        }
                    });
                }
            }
        }
    }
}
