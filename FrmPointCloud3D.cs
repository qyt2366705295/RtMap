using Newtonsoft.Json;
using Renci.SshNet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RtMap
{
    public partial class FrmPointCloud3D : Form
    {
        private CancellationTokenSource _cts;
        private bool _isMonitoring = false;
        private static ConcurrentDictionary<string, bool> _runningAutomationTasks = new ConcurrentDictionary<string, bool>();

        // 路径与可执行程序配置
        private string RT_SCEN_EXE => AppConfig.Instance.Paths.RtScenExe;
        private string OBJ_2_TILES_EXE => AppConfig.Instance.Paths.Obj2TilesExe;
        private string DEFAULT_REMOTE_PATH => AppConfig.Instance.Monitor.DefaultRemoteInfoDir3D;
        private string DEFAULT_LOCAL_PATH => AppConfig.Instance.Monitor.DefaultLocalInfoDir3D;
        public FrmPointCloud3D()
        {
            InitializeComponent();
        }

        private void FrmPointCloud3D_Load(object sender, EventArgs e)
        {
            chkLocalMode.CheckedChanged += ChkLocalMode_CheckedChanged;
            ToggleInfoPathByDefault();
            this.FormClosing += FrmPointCloud3D_FormClosing;
        }

        private void ChkLocalMode_CheckedChanged(object sender, EventArgs e)
        {
            ToggleInfoPathByDefault();
        }

        private void ToggleInfoPathByDefault()
        {
            if (_isMonitoring) return;
            txtInfoPath.Text = chkLocalMode.Checked ? DEFAULT_LOCAL_PATH : DEFAULT_REMOTE_PATH;
        }

        private void btnStartMonitor_Click(object sender, EventArgs e)
        {
            if (!_isMonitoring)
            {
                _cts = new CancellationTokenSource();
                string infoPath = txtInfoPath.Text.Trim();

                chkLocalMode.Enabled = false;
                txtInfoPath.Enabled = false;

                Task.Run(() => MonitorInfoDirectory(infoPath, _cts.Token));

                btnStartMonitor.Text = "停止监控";
                btnStartMonitor.BackColor = System.Drawing.Color.LightCoral;
                _isMonitoring = true;
                UpdateStatus("正在实时监控三维点云任务指令...");
            }
            else
            {
                _cts?.Cancel();
                chkLocalMode.Enabled = true;
                txtInfoPath.Enabled = true;
                btnStartMonitor.Text = "开始监控";
                btnStartMonitor.BackColor = System.Drawing.Color.LightSkyBlue;
                _isMonitoring = false;
                UpdateStatus("三维监控已停止。");
            }
        }

        private void MonitorInfoDirectory(string remoteInfoDir, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    int maxThreads = (int)numMaxThreads.Value;
                    bool isDemoMode = chkDemoMode.Checked;
                    bool isLocalMode = chkLocalMode.Checked;

                    if (_runningAutomationTasks.Count >= maxThreads)
                    {
                        UpdateStatus($"[系统] 三维解算已满载 ({_runningAutomationTasks.Count}/{maxThreads})，暂停认领...");
                        Task.Delay(5000, token).ContinueWith(t => { }).Wait();
                        continue;
                    }

                    if (isLocalMode)
                    {
                        if (Directory.Exists(remoteInfoDir))
                        {
                            var jsonFiles = Directory.GetFiles(remoteInfoDir, "*.json");
                            foreach (var file in jsonFiles)
                            {
                                if (_runningAutomationTasks.Count >= maxThreads) break;

                                string jsonContent = File.ReadAllText(file);
                                TaskInfo task = JsonConvert.DeserializeObject<TaskInfo>(jsonContent);
                                if (task == null) continue;

                                if (task.action == "START")
                                {
                                    if (_runningAutomationTasks.ContainsKey(task.exec_id)) continue;

                                    string lockFile = Path.Combine(remoteInfoDir, task.exec_id + ".lock");
                                    string doneFile = Path.Combine(remoteInfoDir, task.exec_id + ".done");

                                    if (File.Exists(lockFile) || (File.Exists(doneFile) && !isDemoMode)) continue;

                                    try
                                    {
                                        using (FileStream fs = new FileStream(lockFile, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                                        using (StreamWriter sw = new StreamWriter(fs))
                                        {
                                            sw.WriteLine($"Claimed by {Environment.MachineName} at {DateTime.Now}");
                                        }

                                        if (_runningAutomationTasks.TryAdd(task.exec_id, true))
                                        {
                                            Task.Run(() => ExecuteTaskPipeline3D(task), token);
                                        }
                                    }
                                    catch (IOException) { continue; }
                                }
                            }
                        }
                    }
                    else
                    {
                        var srv = AppConfig.Instance.Server;
                        using (var sftp = new SftpClient(srv.Host, srv.Port, srv.Username, srv.Password))
                        {
                            sftp.Connect();
                            if (sftp.Exists(remoteInfoDir))
                            {
                                var files = sftp.ListDirectory(remoteInfoDir).Where(f => f.Name.EndsWith(".json"));
                                foreach (var file in files)
                                {
                                    if (_runningAutomationTasks.Count >= maxThreads) break;

                                    string jsonContent = sftp.ReadAllText(file.FullName);
                                    TaskInfo task = JsonConvert.DeserializeObject<TaskInfo>(jsonContent);
                                    if (task == null) continue;

                                    if (task.action == "START")
                                    {
                                        if (_runningAutomationTasks.ContainsKey(task.exec_id)) continue;

                                        string lockFile = remoteInfoDir + "/" + task.exec_id + ".lock";
                                        string doneFile = remoteInfoDir + "/" + task.exec_id + ".done";

                                        if (sftp.Exists(lockFile) || (sftp.Exists(doneFile) && !isDemoMode)) continue;

                                        string myTmpLock = remoteInfoDir + $"/tmp_{Environment.MachineName}_{Guid.NewGuid().ToString().Substring(0, 5)}.tmp";
                                        try
                                        {
                                            sftp.WriteAllText(myTmpLock, $"Claimed by {Environment.MachineName} at {DateTime.Now}");
                                            sftp.RenameFile(myTmpLock, lockFile);

                                            if (_runningAutomationTasks.TryAdd(task.exec_id, true))
                                            {
                                                Task.Run(() => ExecuteTaskPipeline3D(task), token);
                                            }
                                        }
                                        catch (Exception)
                                        {
                                            if (sftp.Exists(myTmpLock)) sftp.DeleteFile(myTmpLock);
                                            continue;
                                        }
                                    }
                                }
                            }
                            sftp.Disconnect();
                        }
                    }
                }
                catch (Exception ex)
                {
                    UpdateStatus($"监控异常：{ex.Message}，正在重试...");
                }

                Task.Delay(5000, token).ContinueWith(t => { }).Wait();
            }
        }

        private async Task ExecuteTaskPipeline3D(TaskInfo task)
        {
            string execId = task.exec_id;
            UpdateStatus($"[3D任务{execId}] 认领成功，启动三维点云解算线程...");

            CancellationTokenSource taskCts = new CancellationTokenSource();
            FileSystemWatcher objWatcher = null;
            BlockingCollection<string> objQueue = new BlockingCollection<string>();
            ConcurrentDictionary<string, bool> activeObjLocks = new ConcurrentDictionary<string, bool>();

            try
            {
                // 初始化工作空间目录
                string localPtsDir = Path.Combine(AppConfig.Instance.Paths.PointCloudRoot, execId);
                string workspaceDir = Path.Combine(AppConfig.Instance.Paths.Workspace3DRoot, execId);
                string localTilesOutputDir = Path.Combine(workspaceDir, "3DTiles");

                Directory.CreateDirectory(localPtsDir);
                Directory.CreateDirectory(workspaceDir);
                Directory.CreateDirectory(localTilesOutputDir);

                task.local_result_path = localTilesOutputDir;
                TaskBridge.SaveOrUpdateTask(task);

                // 拷贝/下载点云文件到本地
                bool isLocalMode = chkLocalMode.Checked;
                UpdateStatus($"[3D任务{execId}] 正在准备点云数据(.txt)...");
                if (isLocalMode)
                {
                    await Task.Run(() => CopyLocalTxtFiles(task.image_path, localPtsDir, taskCts.Token));
                }
                else
                {
                    await Task.Run(() => DownloadRemoteTxtFiles(task.image_path, localPtsDir, taskCts.Token));
                }

                // 构造工程文件路径与参数，启动三维处理程序RtScen.exe
                string prjPath = Path.Combine(workspaceDir, $"{execId}.rts");
                string rtScenArgs = $"-headless -new -prj \"{prjPath}\" -pts \"{localPtsDir}\" -dom \"{task.dom_path}\" -centm {task.centm}";

                UpdateStatus($"[3D任务{execId}] 启动RtScen.exe建模引擎...");
                Process startRtScenProc = LaunchRtScenEngine(rtScenArgs, workspaceDir);

                // 开启3DTiles转换与同步上传消费者线程
                var objProcessingTask = Task.Run(() => ConsumerObjTo3DTilesLoop(objQueue, task, activeObjLocks, taskCts.Token));

                // 监听workspaceDir下生成的.obj三维模型变动
                objWatcher = new FileSystemWatcher(workspaceDir, "*.obj");
                objWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;

                Action<string> enqueueObj = (filePath) =>
                {
                    if (activeObjLocks.TryAdd(filePath, true))
                    {
                        objQueue.Add(filePath);
                        UpdateStatus($"[3D任务{execId}] 检测到新生成的OBJ模型：{Path.GetFileName(filePath)}，已排队转换...");
                    }
                };

                objWatcher.Created += (s, e) => enqueueObj(e.FullPath);
                objWatcher.Changed += (s, e) => enqueueObj(e.FullPath);
                objWatcher.EnableRaisingEvents = true;

                // 检查是否已有提前生成的.obj文件
                foreach (var existingObj in Directory.GetFiles(workspaceDir, "*.obj"))
                {
                    enqueueObj(existingObj);
                }

                // 等待底层RtScen.exe处理完成
                if (startRtScenProc != null && !startRtScenProc.HasExited)
                {
                    await Task.Run(() => startRtScenProc.WaitForExit());
                }

                // 结算OBJ队列
                objQueue.CompleteAdding();
                await objProcessingTask;

                UpdateStatus($"[3D任务{execId}] 三维建模、3DTiles转换与数据同步全部完成！");
            }
            catch (Exception ex)
            {
                UpdateStatus($"[3D任务{execId}] 流水线发生异常：{ex.Message}");
            }
            finally
            {
                if (objWatcher != null)
                {
                    objWatcher.EnableRaisingEvents = false;
                    objWatcher.Dispose();
                }
                taskCts.Dispose();

                // 释放锁
                ReleaseTaskLock(task);
                _runningAutomationTasks.TryRemove(execId, out _);
            }
        }

        private void ConsumerObjTo3DTilesLoop(BlockingCollection<string> queue, TaskInfo task, ConcurrentDictionary<string, bool> activeLocks, CancellationToken token)
        {
            try
            {
                foreach (var objPath in queue.GetConsumingEnumerable(token))
                {
                    string objFileName = Path.GetFileName(objPath);
                    string objNameNoExt = Path.GetFileNameWithoutExtension(objPath);

                    try
                    {
                        UpdateStatus($"[3D任务{task.exec_id}] 等待文件写就绪：{objFileName}...");

                        // 轮询等待RtScen.exe写完并释放.obj文件句柄
                        if (WaitUntilFileIsReady(objPath, 120))
                        {
                            string singleTilesOutputDir = Path.Combine(task.local_result_path, objNameNoExt);
                            Directory.CreateDirectory(singleTilesOutputDir);

                            UpdateStatus($"[3D任务{task.exec_id}] 正在调用Obj2Tiles.exe将{objFileName}转为3DTiles...");
                            RunObj2TilesConverter(objPath, singleTilesOutputDir);

                            // 同步上传至服务器
                            UpdateStatus($"[3D任务{task.exec_id}] 3DTiles转换成功，开始同步上传服务器：{objNameNoExt}...");
                            Upload3DTilesToSftp(singleTilesOutputDir, objNameNoExt, task.result_path);

                            // 每上传一次，触发一次回调
                            UpdateStatus($"[3D任务{task.exec_id}] 上传完成，发起UOS接口回调：{objNameNoExt}...");
                            UosApiClient.NotifyUosBuildComplete(task.exec_id, task.result_path, objNameNoExt).Wait();
                        }
                        else
                        {
                            UpdateStatus($"[3D任务{task.exec_id}] 警告：文件{objFileName}超时未被释放，跳过本次转换。");
                        }
                    }
                    catch (Exception ex)
                    {
                        UpdateStatus($"[3D任务{task.exec_id}] 处理模型{objFileName}失败：{ex.Message}");
                    }
                    finally
                    {
                        activeLocks.TryRemove(objPath, out _);
                    }
                }
            }
            catch (OperationCanceledException) { }
        }

        private void RunObj2TilesConverter(string inputObj, string outputTilesDir)
        {
            if (!File.Exists(OBJ_2_TILES_EXE))
            {
                throw new FileNotFoundException($"找不到3DTiles转换工具Obj2Tiles.exe：{OBJ_2_TILES_EXE}");
            }

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = OBJ_2_TILES_EXE,
                Arguments = $"\"{inputObj}\" \"{outputTilesDir}\"",
                CreateNoWindow = true,
                UseShellExecute = false
            };

            using (Process proc = Process.Start(psi))
            {
                proc.WaitForExit();
            }
        }

        private Process LaunchRtScenEngine(string arguments, string workingDir)
        {
            if (!File.Exists(RT_SCEN_EXE))
            {
                throw new FileNotFoundException($"找不到三维处理程序RtScen.exe：{RT_SCEN_EXE}");
            }

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = RT_SCEN_EXE,
                Arguments = arguments,
                WorkingDirectory = workingDir,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            return Process.Start(psi);
        }

        private void CopyLocalTxtFiles(string srcDir, string destDir, CancellationToken token)
        {
            if (!Directory.Exists(srcDir)) return;
            var files = Directory.GetFiles(srcDir, "*.txt");
            foreach (var file in files)
            {
                if (token.IsCancellationRequested) break;
                string dest = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, dest, true);
            }
        }

        private void DownloadRemoteTxtFiles(string remoteDir, string localDir, CancellationToken token)
        {
            var srv = AppConfig.Instance.Server;
            using (var sftp = new SftpClient(srv.Host, srv.Port, srv.Username, srv.Password))
            {
                sftp.Connect();
                if (sftp.Exists(remoteDir))
                {
                    var files = sftp.ListDirectory(remoteDir).Where(f => f.IsRegularFile && f.Name.EndsWith(".txt"));
                    foreach (var file in files)
                    {
                        if (token.IsCancellationRequested) break;
                        string localFile = Path.Combine(localDir, file.Name);
                        using (var fs = File.Create(localFile))
                        {
                            sftp.DownloadFile(file.FullName, fs);
                        }
                    }
                }
                sftp.Disconnect();
            }
        }

        private void Upload3DTilesToSftp(string localTileDir, string folderName, string remoteBaseDir)
        {
            var srv = AppConfig.Instance.Server;
            using (var sftp = new SftpClient(srv.Host, srv.Port, srv.Username, srv.Password))
            {
                sftp.Connect();
                string remoteTargetDir = remoteBaseDir.TrimEnd('/') + "/" + folderName;
                UploadDirectoryRecursively(sftp, localTileDir, remoteTargetDir);
                sftp.Disconnect();
            }
        }

        private void UploadDirectoryRecursively(SftpClient sftp, string localDir, string remoteDir)
        {
            CreateRemoteDirectorySafe(sftp, remoteDir);

            foreach (string file in Directory.GetFiles(localDir))
            {
                using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    string remoteFileName = remoteDir + "/" + Path.GetFileName(file);
                    sftp.UploadFile(fs, remoteFileName, true);
                }
            }

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

        private bool WaitUntilFileIsReady(string filePath, int timeoutSeconds)
        {
            DateTime startTime = DateTime.Now;
            while ((DateTime.Now - startTime).TotalSeconds < timeoutSeconds)
            {
                try
                {
                    using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        return true;
                    }
                }
                catch (IOException)
                {
                    Thread.Sleep(500);
                }
            }
            return false;
        }

        private void ReleaseTaskLock(TaskInfo task)
        {
            try
            {
                bool isLocalMode = chkLocalMode.Checked;
                string infoPath = txtInfoPath.Text.Trim();

                if (isLocalMode)
                {
                    string lockFile = Path.Combine(infoPath, task.exec_id + ".lock");
                    string doneFile = Path.Combine(infoPath, task.exec_id + ".done");
                    if (File.Exists(lockFile))
                    {
                        if (File.Exists(doneFile)) File.Delete(doneFile);
                        File.Move(lockFile, doneFile);
                    }
                }
                else
                {
                    var srv = AppConfig.Instance.Server;
                    using (var sftp = new SftpClient(srv.Host, srv.Port, srv.Username, srv.Password))
                    {
                        sftp.Connect();
                        string lockFile = infoPath + "/" + task.exec_id + ".lock";
                        string doneFile = infoPath + "/" + task.exec_id + ".done";

                        if (sftp.Exists(lockFile))
                        {
                            if (sftp.Exists(doneFile)) sftp.DeleteFile(doneFile);
                            sftp.RenameFile(lockFile, doneFile);
                        }
                        sftp.Disconnect();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"释放分布式锁异常：{ex.Message}");
            }
        }

        private void UpdateStatus(string msg)
        {
            if (InvokeRequired) Invoke(new Action(() => lblStatus.Text = msg));
            else lblStatus.Text = msg;
        }

        private void FrmPointCloud3D_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cts?.Cancel();
        }
    }
}
