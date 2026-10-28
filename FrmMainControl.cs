using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Tsp;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;

namespace RtMap
{
    public partial class FrmMainControl : Form
    {
        private CancellationTokenSource _cts;
        private bool _isMonitoring = false;
        private BindingSource _taskBindingSource = new BindingSource();
        private List<TaskInfo> _pendingTasks = new List<TaskInfo>();

        private static ConcurrentDictionary<string, bool> _runningAutomationTasks = new ConcurrentDictionary<string, bool>();

        private string DEFAULT_REMOTE_PATH => AppConfig.Instance.Monitor.DefaultRemoteInfoDir;
        private string DEFAULT_LOCAL_PATH => AppConfig.Instance.Monitor.DefaultLocalInfoDir;

        public FrmMainControl()
        {
            InitializeComponent();
            cboPendingTasks.DataSource = _taskBindingSource;

            chkLocalMode.CheckedChanged += ChkLocalMode_CheckedChanged;
            ToggleInfoPathByDefault();

            this.FormClosing += FrmMainControl_FormClosing;
        }

        private void ChkLocalMode_CheckedChanged(object sender, EventArgs e)
        {
            ToggleInfoPathByDefault();
        }

        private void ToggleInfoPathByDefault()
        {
            // 如果处于监控运行中，不允许用户中途切模式，防止流水线混乱
            if (_isMonitoring) return;

            if (chkLocalMode.Checked)
            {
                txtInfoPath.Text = DEFAULT_LOCAL_PATH;
            }
            else
            {
                txtInfoPath.Text = DEFAULT_REMOTE_PATH;
            }
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
                UpdateStatus("正在实时监控服务器任务指令...");
            }
            else
            {
                _cts?.Cancel();
                chkLocalMode.Enabled = true;
                txtInfoPath.Enabled = true;
                btnStartMonitor.Text = "开始监控";
                btnStartMonitor.BackColor = System.Drawing.Color.LightSkyBlue;
                _isMonitoring = false;
                UpdateStatus("监控已停止。");
            }
        }

        private static string _lastInfoPath = AppConfig.Instance.Monitor.DefaultRemoteInfoDir;
        private void MonitorInfoDirectory(string remoteInfoDir, CancellationToken token)
        {
            _lastInfoPath = remoteInfoDir;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    int maxThreads = 1;
                    bool isDemoMode = false;
                    bool isLocalMode = false;

                    this.Invoke(new Action(() => {
                        maxThreads = (int)numMaxThreads.Value;
                        isDemoMode = chkDemoMode.Checked;
                        isLocalMode = chkLocalMode.Checked;
                    }));

                    if (_runningAutomationTasks.Count >= maxThreads)
                    {
                        UpdateStatus($"[系统] 本机解算已满载 ({_runningAutomationTasks.Count}/{maxThreads})，暂停认领...");
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

                                if (task.action == "STOP")
                                {
                                    FrmImageDownloader.StopDownloadForTask(task.exec_id);
                                    continue;
                                }

                                if (task.action == "START")
                                {
                                    if (_runningAutomationTasks.ContainsKey(task.exec_id)) continue;

                                    // 拼接本地/共享盘的锁文件路径
                                    string lockFile = Path.Combine(remoteInfoDir, task.exec_id + ".lock");
                                    string doneFile = Path.Combine(remoteInfoDir, task.exec_id + ".done");

                                    if (File.Exists(lockFile) || (File.Exists(doneFile) && !isDemoMode))
                                    {
                                        continue;
                                    }

                                    try
                                    {
                                        // 使用FileMode.CreateNew，如果别的电脑先创建了，抛出IOException
                                        using (FileStream fs = new FileStream(lockFile, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                                        {
                                            using (StreamWriter sw = new StreamWriter(fs))
                                            {
                                                sw.WriteLine($"Claimed by {Environment.MachineName} at {DateTime.Now}");
                                            }
                                        }

                                        // 成功创建文件，代表这台电脑抢锁成功
                                        if (_runningAutomationTasks.TryAdd(task.exec_id, true))
                                        {
                                            Task.Run(() => ExecuteTaskPipelineAutomated(task), token);
                                        }
                                    }
                                    catch (IOException)
                                    {
                                        // 进到这里说明被其他电脑截胡了，安全跳过
                                        continue;
                                    }
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
                                List<TaskInfo> currentScannedTasks = new List<TaskInfo>();

                                foreach (var file in files)
                                {
                                    if (_runningAutomationTasks.Count >= maxThreads) break;

                                    // 读取并解析JSON文件
                                    string jsonContent = sftp.ReadAllText(file.FullName);
                                    TaskInfo task = JsonConvert.DeserializeObject<TaskInfo>(jsonContent);
                                    if (task == null) continue;

                                    if (task.action == "STOP")
                                    {
                                        FrmImageDownloader.StopDownloadForTask(task.exec_id);
                                        continue;
                                    }

                                    if (task.action == "START")
                                    {
                                        currentScannedTasks.Add(task);

                                        // 如果本机已经在跑了，无需重复抢锁
                                        if (_runningAutomationTasks.ContainsKey(task.exec_id)) continue;

                                        // 定义分布式状态锁的文件路径
                                        string lockFile = remoteInfoDir + "/" + task.exec_id + ".lock";
                                        string doneFile = remoteInfoDir + "/" + task.exec_id + ".done";

                                        if (sftp.Exists(lockFile) || (sftp.Exists(doneFile) && !isDemoMode))
                                        {
                                            continue;
                                        }

                                        string myTmpLock = remoteInfoDir + $"/tmp_{Environment.MachineName}_{Guid.NewGuid().ToString().Substring(0, 5)}.tmp";
                                        try
                                        {
                                            sftp.WriteAllText(myTmpLock, $"Claimed by {Environment.MachineName} at {DateTime.Now}");
                                            sftp.RenameFile(myTmpLock, lockFile);

                                            if (_runningAutomationTasks.TryAdd(task.exec_id, true))
                                            {
                                                Task.Run(() => ExecuteTaskPipelineAutomated(task), token);
                                            }
                                        }
                                        catch (Exception)
                                        {
                                            // 抢锁失败，说明被别的电脑截胡了
                                            if (sftp.Exists(myTmpLock)) sftp.DeleteFile(myTmpLock);
                                            continue;
                                        }
                                    }
                                }

                                // 更新下拉框UI
                                this.Invoke(new Action(() =>
                                {
                                    UpdateTaskDropdown(currentScannedTasks);
                                }));
                            }
                            sftp.Disconnect();
                        }
                    }
                        
                }
                catch (Exception ex)
                {
                    UpdateStatus($"监控异常：{ex.Message}，正在重试...");
                }

                // 每5秒轮询一次
                Task.Delay(5000, token).ContinueWith(t => { }).Wait();
            }
        }

        #region 全自动核心流水线
        private async Task ExecuteTaskPipelineAutomated(TaskInfo task)
        {
            string execId = task.exec_id;
            UpdateStatus($"检测到新任务{execId}，开始执行自动化线程...");

            FileSystemWatcher tifWatcher = null;
            CancellationTokenSource downloadCts = null;

            try
            {
                // --------- 阶段1：环境初始化与下传路径自动映射 ---------
                string localImageDir = Path.Combine(AppConfig.Instance.Paths.ImageRoot, execId);
                Directory.CreateDirectory(localImageDir);

                downloadCts = new CancellationTokenSource();
                FrmImageDownloader.ActiveDownloads.TryAdd(execId, downloadCts);

                Task downloadWorker = null;
                bool isVideoTask = false;
                bool isLocalMode = false;
                this.Invoke(new Action(() => { isLocalMode = chkLocalMode.Checked; }));

                string targetVideoPath = null;
                bool hasHandle = false;
                // 兼容性路径探测：支持直接填.mp4或填包含了mp4的文件夹
                if (Uri.TryCreate(task.image_path, UriKind.Absolute, out Uri uriResult) &&
                    (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps || uriResult.Scheme == "rtsp"))
                {
                    targetVideoPath = task.image_path;
                    isVideoTask = true;
                    UpdateStatus($"[任务{execId}] 检测到视频流，拉起后台实时抽帧引擎...");

                    ProcessStartInfo videoPsi = new ProcessStartInfo();
                    videoPsi.FileName = AppConfig.Instance.Paths.VideoSamplerExe;
                    videoPsi.Arguments = $"\"{targetVideoPath}\" \"{localImageDir}\" 0.5";
                    videoPsi.CreateNoWindow = true;
                    videoPsi.UseShellExecute = false;
                    Process vProc = Process.Start(videoPsi);

                    // 将进程的等待释放包装进Task。这样它就在后台静默抽帧，同时完美融入现有流水线的生命周期
                    downloadWorker = Task.Run(() =>
                    {
                        vProc.WaitForExit();
                        vProc.Dispose();
                    });
                    hasHandle = true;
                }

                if (isLocalMode)
                {
                    if (File.Exists(task.image_path) && (task.image_path.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) || task.image_path.EndsWith(".avi", StringComparison.OrdinalIgnoreCase)))
                    {
                        targetVideoPath = task.image_path;
                    }
                    else if (Directory.Exists(task.image_path))
                    {
                        var videoFiles = Directory.GetFiles(task.image_path).Where(f => f.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".avi", StringComparison.OrdinalIgnoreCase)).ToList();
                        if (videoFiles.Count > 0) targetVideoPath = videoFiles[0];
                    }

                    // 如果是视频流任务，实行异步非阻塞流式处理
                    if (targetVideoPath != null && !hasHandle)
                    {
                        isVideoTask = true;
                        UpdateStatus($"[任务{execId}] 检测到视频流，拉起后台实时抽帧引擎...");

                        ProcessStartInfo videoPsi = new ProcessStartInfo();
                        videoPsi.FileName = AppConfig.Instance.Paths.VideoSamplerExe;
                        videoPsi.Arguments = $"\"{targetVideoPath}\" \"{localImageDir}\" 0.5";
                        videoPsi.CreateNoWindow = true;
                        videoPsi.UseShellExecute = false;
                        Process vProc = Process.Start(videoPsi);

                        // 将进程的等待释放包装进Task。这样它就在后台静默抽帧，同时完美融入现有流水线的生命周期
                        downloadWorker = Task.Run(() =>
                        {
                            vProc.WaitForExit();
                            vProc.Dispose();
                        });
                    }
                    else
                    {
                        // 普通本地照片文件夹拷贝监控
                        downloadWorker = Task.Run(() => MonitorLocalDirectoryHeadless(task.image_path, localImageDir, downloadCts.Token, task.exec_id));
                    }
                }
                else
                {
                    downloadWorker = Task.Run(() => MonitorRemoteDirectoryHeadless(task.image_path, localImageDir, downloadCts.Token));
                }

                // 自动化默认缺省配置：使用Sfm算法，相机类型S6100_X_40
                string defaultAlgorithm = "Sfm";
                string defaultCamera = "S6100_X_40";

                int minImageCountThreshold = 10;
                bool isCameraIdentified = false;

                while (!downloadCts.Token.IsCancellationRequested)
                {
                    // 扫描本地目录，寻找第一张落地的有效影像文件
                    var downloadedImages = Directory.EnumerateFiles(localImageDir)
                        .Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                    f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                                    f.EndsWith(".tif", StringComparison.OrdinalIgnoreCase) ||
                                    f.EndsWith(".tiff", StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (!isCameraIdentified && downloadedImages.Count > 0)
                    {
                        if (isVideoTask)
                        {
                            defaultCamera = "DJI_Video";
                            isCameraIdentified = true;
                        }
                        else
                        {
                            // 基于EXIF+外置JSON配置匹配
                            string detectedParamDir = null;
                            foreach (var imgPath in downloadedImages)
                            {
                                detectedParamDir = ExifHelper.GetCameraParamDirFromExif(imgPath);
                                if (!string.IsNullOrEmpty(detectedParamDir))
                                {
                                    defaultCamera = detectedParamDir;
                                    break;
                                }
                            }

                            // EXIF匹配失败时，回退到文件名推断（兼容旧数据/无EXIF图片）
                            if (string.IsNullOrEmpty(detectedParamDir))
                            {
                                string firstFileName = Path.GetFileName(downloadedImages[0]);

                                if (firstFileName.Contains("X_"))
                                {
                                    defaultCamera = "S6100_X_40";
                                }
                                //else if (firstFileName.StartsWith("DJI_", StringComparison.OrdinalIgnoreCase))
                                //{
                                //    defaultCamera = "M3M";
                                //}
                                else
                                {
                                    // 完全无法识别时保持默认S6100_X_40，或改为"Else"
                                    defaultCamera = "Else";
                                }
                                UpdateStatus($"[任务{execId}] EXIF未识别");
                            }
                            else
                            {
                                UpdateStatus($"[任务{execId}] EXIF识别相机型号为：{defaultCamera}");
                            }

                            isCameraIdentified = true;
                        }
                    }

                    if (isCameraIdentified && downloadedImages.Count >= minImageCountThreshold)
                    {
                        break;
                    }

                    // 健壮性检查：如果下载线程因为网络彻底崩溃或配错路径提前挂了，直接抛出异常
                    if (downloadWorker.IsCompleted || downloadWorker.IsFaulted)
                    {
                        if (downloadedImages.Count > 0)
                        {
                            // 如果总图数少于阈值但已经传完了，那别干等了，直接拉起引擎
                            break;
                        }
                        else
                        {
                            throw new Exception("服务器影像下传意外终止，未能成功获取任何影像。");
                        }
                    }

                    // 每隔1秒扫描一次本地目录
                    await Task.Delay(1000);
                }

                // 如果在等图期间，服务器直接传来了STOP信号，则直接安全退出流水线
                if (downloadCts.Token.IsCancellationRequested)
                {
                    UpdateStatus($"[任务{execId}] 接收到终止指令，退出线程。");
                    return;
                }

                // 自动化拷贝配置文件
                DeployAlgorithmConfigHeadless(localImageDir, defaultAlgorithm, defaultCamera);

                // --------- 阶段2：自动规划Sfm成果输出路径 ---------
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string workspaceDir = Path.Combine(AppConfig.Instance.Paths.SfmWorkspaceRoot, $"Proj_{execId}_{timestamp}");
                string dbPath = Path.Combine(workspaceDir, $"{execId}.db");
                string expectedLocalResultDir = Path.Combine(workspaceDir, "OutputOrtho"); // Tif生成目录

                Directory.CreateDirectory(workspaceDir);
                Directory.CreateDirectory(expectedLocalResultDir);

                // 更新本地和内存里的成果路径桥梁
                task.local_result_path = expectedLocalResultDir;
                TaskBridge.SaveOrUpdateTask(task);

                // --------- 阶段3：开启并行任务（下载监控、成图引擎、切片监控） ---------
                BlockingCollection<string> tifQueue = new BlockingCollection<string>();
                ConcurrentDictionary<string, bool> queuedTifFiles = new ConcurrentDictionary<string, bool>();
                var tilingWorker = Task.Run(() => ConsumerSliceAndUploadLoop(tifQueue, task, downloadCts.Token));

                // 启动外部实时成图引擎可执行程序 (OtFRunner.exe)
                Process engineProcess = LaunchEngineHeadless(dbPath, localImageDir);

                engineProcess.EnableRaisingEvents = true;
                engineProcess.Exited += (s, ev) =>
                {
                    // 无论引擎是自己跑完正常退出的，还是演示时手动关掉的
                    if (downloadCts != null && !downloadCts.IsCancellationRequested)
                    {
                        // 立刻对影像下载流拉闸，使后台的MonitorRemoteDirectoryHeadless跳出死循环，释放流水线
                        downloadCts.Cancel();
                    }
                };

                // 启动本地成果目录的Tif切片上传流(通过FileSystemWatcher实时捕获引擎吐出的Tif)
                tifWatcher = new FileSystemWatcher(expectedLocalResultDir, "*.tif");
                tifWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;

                Action<string> enqueueAction = (fullPath) =>
                {
                    // 去重控制：如果已经在队列里排队了，直接跳过
                    if (queuedTifFiles.TryAdd(fullPath, true))
                    {
                        tifQueue.Add(fullPath);
                        UpdateStatus($"[任务{execId}] 新成果{Path.GetFileName(fullPath)}已进入队列...");
                    }
                };

                tifWatcher.Created += (s, e) => enqueueAction(e.FullPath);
                tifWatcher.Changed += (s, e) => enqueueAction(e.FullPath);
                tifWatcher.EnableRaisingEvents = true;

                UpdateStatus($"[任务{execId}] 影像下载、实时成图引擎、Tif切片上传线程已启动...");

                // --------- 阶段4：挂起等待数据完结信号 ---------
                // 等待下载线程彻底结束（当服务器info中该ID变为STOP，或者数据传完触发软退出时）
                await downloadWorker;
                UpdateStatus($"[任务{execId}] 服务器影像下传完毕。");

                // 等待底层C++成图引擎把最后一批尾盘数据跑完并安全退出
                if (engineProcess != null && !engineProcess.HasExited)
                {
                    UpdateStatus($"[任务{execId}] 正在等待底层处理引擎解算最后成果并释放文件...");
                    await Task.Run(() => engineProcess.WaitForExit());
                }

                // 通知切片队列
                tifQueue.CompleteAdding();
                UpdateStatus($"[任务{execId}] 正在全力消化剩余切片，请勿关闭程序...");
                await tilingWorker;

                UpdateStatus($"[任务{execId}] 全流程处理完成，清理流水线。");
            }
            catch (Exception ex)
            {
                UpdateStatus($"[任务{execId}] 流水线发生崩溃异常：{ex.Message}，等待下一轮轮询自动重试。");
            }
            finally
            {
                // 销毁切片文件监控器
                if (tifWatcher != null)
                {
                    tifWatcher.EnableRaisingEvents = false;
                    tifWatcher.Dispose();
                }
                if (downloadCts != null)
                {
                    FrmImageDownloader.ActiveDownloads.TryRemove(execId, out _);
                    downloadCts.Dispose();
                }
                try
                {
                    string infoPath = "";
                    bool isLocalMode = false;
                    this.Invoke(new Action(() => {
                        infoPath = txtInfoPath.Text.Trim();
                        isLocalMode = chkLocalMode.Checked;
                    }));

                    if (isLocalMode)
                    {
                        // 本地/共享盘锁文件转换
                        string lockFile = Path.Combine(infoPath, execId + ".lock");
                        string doneFile = Path.Combine(infoPath, execId + ".done");

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
                            string lockFile = infoPath + "/" + execId + ".lock";
                            string doneFile = infoPath + "/" + execId + ".done";

                            // 如果正常跑完了，将锁文件转化为完结文件
                            if (sftp.Exists(lockFile))
                            {
                                if (sftp.Exists(doneFile))
                                {
                                    sftp.DeleteFile(doneFile);
                                }
                                sftp.RenameFile(lockFile, doneFile);
                            }
                            sftp.Disconnect();
                        }
                    }
  
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[分布式锁释放异常] 任务ID：{execId}, 详情：{ex.Message}");
                }
                // 移除运行锁，使该任务ID在未来可以再次被调度
                _runningAutomationTasks.TryRemove(execId, out _);
            }
        }

        private void ConsumerSliceAndUploadLoop(BlockingCollection<string> queue, TaskInfo task, CancellationToken token)
        {
            try
            {
                var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 3, CancellationToken = token };

                var partitioner = Partitioner.Create(
                    queue.GetConsumingEnumerable(token),
                    EnumerablePartitionerOptions.NoBuffering
                );

                Parallel.ForEach(partitioner, parallelOptions, tifPath =>
                {
                    string fileName = Path.GetFileName(tifPath);
                    try
                    {
                        ProcessAndUploadSingleTifAutomated(tifPath, task);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[并发异常] 处理文件{fileName}时崩溃，详情：{ex.Message}");
                    }
                });
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine($"[任务{task.exec_id}] 切片线程收到取消信号退出。");
            }
            catch (Exception ex)
            {
                UpdateStatus($"[任务{task.exec_id}] 切片消费流异常：{ex.Message}");
            }
        }
        #endregion

        #region 从各功能窗体抽离出来的纯后台移植方法
        // 移植自FrmImageDownloader：拷贝算法与相机配置
        private void DeployAlgorithmConfigHeadless(string localImageDir, string algorithm, string cameraType)
        {
            Directory.CreateDirectory(localImageDir);
            string parentDir = Directory.GetParent(localImageDir).FullName;
            string baseTemplateDir = AppConfig.Instance.Paths.CameraParametersRoot;

            if (algorithm == "Slm")
            {
                string sourceConfig = Path.Combine(baseTemplateDir, "config");
                string targetConfig = Path.Combine(parentDir, "config");
                CopyDirectorySimple(sourceConfig, targetConfig);
            }
            else if (algorithm == "Sfm" && cameraType != "Else")
            {
                string sourceXml = Path.Combine(baseTemplateDir, cameraType, "camera.xml");
                string targetXml = Path.Combine(localImageDir, "camera.xml");
                if (File.Exists(sourceXml))
                {
                    File.Copy(sourceXml, targetXml, true);
                }
            }
        }

        private void CopyDirectorySimple(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);
            foreach (var file in Directory.GetFiles(sourceDir))
                File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)), true);
            foreach (var dir in Directory.GetDirectories(sourceDir))
                CopyDirectorySimple(dir, Path.Combine(targetDir, Path.GetFileName(dir)));
        }

        // 移植自FrmImageDownloader：纯后台多线程SFTP下载轮询
        private void MonitorRemoteDirectoryHeadless(string remoteDir, string localDir, CancellationToken token)
        {
            HashSet<string> downloadedFiles = new HashSet<string>();

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var srv = AppConfig.Instance.Server;
                    using (var sftp = new SftpClient(srv.Host, srv.Port, srv.Username, srv.Password))
                    {
                        sftp.Connect();
                        if (sftp.Exists(remoteDir))
                        {
                            // 递归同步下载
                            SyncDirectoryHeadless(sftp, remoteDir, remoteDir, localDir, downloadedFiles, token);
                        }
                        sftp.Disconnect();
                    }
                }
                catch (Exception)
                {
                    // 网络闪断异常不中断线程，等待1秒后重试
                }

                try { Task.Delay(1000, token).Wait(); } catch { break; }
            }
        }

        private void MonitorLocalDirectoryHeadless(string srcDir, string destDir, CancellationToken token, string execId)
        {
            HashSet<string> copiedFiles = new HashSet<string>();

            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (!Directory.Exists(srcDir)) { Thread.Sleep(1000); continue; }

                    // 获取源目录下所有的有效影像文件
                    var allFiles = Directory.GetFiles(srcDir)
                        .Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                    f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                                    f.EndsWith(".tif", StringComparison.OrdinalIgnoreCase) ||
                                    f.EndsWith(".tiff", StringComparison.OrdinalIgnoreCase))
                        .OrderBy(f => f)
                        .ToList();

                    // 针对大疆多光谱（DJI_xx）进行特殊文件名分组排序
                    var fileGroups = allFiles
                        .Where(f => !copiedFiles.Contains(f))
                        .GroupBy(f => {
                            string name = Path.GetFileNameWithoutExtension(f);

                            var match = System.Text.RegularExpressions.Regex.Match(name, @"^DJI_.*_(\d{4})_(D|MS_.*)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                            if (match.Success)
                            {
                                return match.Groups[1].Value; // 返回 "0001", "0002" 等序号作为组键
                            }
                            return name; // 普通照片或frame_正常单张成组
                        })
                        .OrderBy(g => g.Key)
                        .ToList();

                    foreach (var group in fileGroups)
                    {
                        if (token.IsCancellationRequested) break;

                        List<string> readyToCopyInGroup = new List<string>();
                        List<string> finalDestPaths = new List<string>();
                        List<string> tempDestPaths = new List<string>();

                        foreach (var srcFile in group)
                        {
                            // 防抖检查：确保源文件不是正在生成的半截文件
                            FileInfo fi = new FileInfo(srcFile);
                            if ((DateTime.Now - fi.LastWriteTime).TotalSeconds < 2) continue;

                            string fileName = Path.GetFileName(srcFile);
                            string finalDest = Path.Combine(destDir, fileName);
                            string tempDest = finalDest + ".tmp"; // 写入共享/工作目录时带上 .tmp

                            readyToCopyInGroup.Add(srcFile);
                            finalDestPaths.Add(finalDest);
                            tempDestPaths.Add(tempDest);
                        }

                        if (readyToCopyInGroup.Count < group.Count()) continue;
                        // 执行组拷贝：把这个组里目前已就绪的所有波段先全拷过去变成.tmp
                        for (int i = 0; i < readyToCopyInGroup.Count; i++)
                        {
                            File.Copy(readyToCopyInGroup[i], tempDestPaths[i], true);
                        }

                        for (int i = 0; i < readyToCopyInGroup.Count; i++)
                        {
                            if (File.Exists(finalDestPaths[i])) File.Delete(finalDestPaths[i]);
                            File.Move(tempDestPaths[i], finalDestPaths[i]);

                            copiedFiles.Add(readyToCopyInGroup[i]); // 加入记忆库，防止重复拷贝
                        }

                        if (readyToCopyInGroup.Count > 0)
                        {
                            UpdateStatus($"[任务{execId}] 成功同步航点影像组：{group.Key} (含{readyToCopyInGroup.Count}个波段文件)");
                        }
                    }
                }
                catch (Exception) { /* 允许闪断重试 */ }

                try { Task.Delay(1000, token).Wait(); } catch { break; }
            }
        }

        private void SyncDirectoryHeadless(SftpClient sftp, string currentRemoteDir, string baseRemoteDir, string baseLocalDir, HashSet<string> downloaded, CancellationToken token)
        {
            if (token.IsCancellationRequested) return;

            var rawItems = sftp.ListDirectory(currentRemoteDir).ToList();
            var orderedFiles = rawItems.Where(f => f.IsRegularFile && !f.Name.StartsWith(".")).OrderBy(f => f.Name).ToList();
            var subDirectories = rawItems.Where(f => f.IsDirectory && f.Name != "." && f.Name != "..").ToList();

            foreach (var file in orderedFiles)
            {
                if (token.IsCancellationRequested) break;

                if (!downloaded.Contains(file.FullName))
                {
                    // 防抖：如果文件刚刚生成不足3秒，跳过，等下一轮下载，防止文件写一半被损毁
                    if ((DateTime.UtcNow - file.LastWriteTimeUtc).TotalSeconds < 3) continue;

                    string ext = Path.GetExtension(file.Name).ToLower();
                    if (ext == ".tif" || ext == ".tiff" || ext == ".jpg" || ext == ".png")
                    {
                        string relativeDir = currentRemoteDir.Substring(baseRemoteDir.Length).TrimStart('/');
                        string localTargetDir = string.IsNullOrEmpty(relativeDir) ? baseLocalDir : Path.Combine(baseLocalDir, relativeDir.Replace("/", "\\"));

                        Directory.CreateDirectory(localTargetDir);
                        string localFilePath = Path.Combine(localTargetDir, file.Name);
                        string tempFilePath = localFilePath + ".tmp";

                        try
                        {
                            using (Stream fileStream = File.OpenWrite(tempFilePath))
                            {
                                sftp.DownloadFile(file.FullName, fileStream);
                            }

                            if (File.Exists(localFilePath))
                            {
                                File.Delete(localFilePath);
                            }

                            File.Move(tempFilePath, localFilePath);

                            downloaded.Add(file.FullName);
                        }
                        catch (Exception ex)
                        {
                            if (File.Exists(tempFilePath))
                            {
                                try { File.Delete(tempFilePath); } catch { }
                            }
                            throw;
                        }

                    }
                }
            }

            foreach (var dir in subDirectories)
            {
                if (token.IsCancellationRequested) break;
                SyncDirectoryHeadless(sftp, dir.FullName, baseRemoteDir, baseLocalDir, downloaded, token);
            }
        }

        // 移植自FrmImageDownloader：纯后台启动Sfm解算引擎进程
        private Process LaunchEngineHeadless(string dbPath, string localImageDir)
        {
            string exePath = AppConfig.Instance.Paths.OtFRunnerExe;
            string workDir = Path.GetDirectoryName(exePath);

            if (!File.Exists(exePath)) throw new FileNotFoundException("找不到处理引擎OtFRunner.exe程序！");

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = exePath;
            psi.Arguments = $"\"{dbPath}\" \"{localImageDir}\"";
            psi.WorkingDirectory = workDir;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = false; // 纯静默自动执行，隐藏命令行黑框

            return Process.Start(psi);
        }

        // 移植自gdal2tiles：自动创建本地Tif切片监控
        private FileSystemWatcher StartTilingWatcherHeadless(string targetTifDir, TaskInfo task)
        {
            FileSystemWatcher watcher = new FileSystemWatcher(targetTifDir, "*.tif");
            watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;

            // 发生变更或创建新Tif时直接扔到后台线程处理切片和上传
            watcher.Created += (s, e) => Task.Run(() => ProcessAndUploadSingleTifAutomated(e.FullPath, task));
            watcher.Changed += (s, e) => Task.Run(() => ProcessAndUploadSingleTifAutomated(e.FullPath, task));
            watcher.EnableRaisingEvents = true;

            return watcher;
        }

        // 核心链条闭环：切片 -> 上传 -> UOS接口网络回调
        private void ProcessAndUploadSingleTifAutomated(string tifPath, TaskInfo task)
        {
            try
            {
                // 轮询等待底层解算引擎写完并释放Tif文件句柄
                if (!WaitUntilFileReadySimple(tifPath, 60)) return;

                CopyToSharedDrive(tifPath, task.exec_id);

                bool isLocalMode = false;
                this.Invoke(new Action(() => { isLocalMode = chkLocalMode.Checked; }));
                if (isLocalMode)
                {
                    return;
                }

                string tileOutputDir = Path.Combine(task.local_result_path, Path.GetFileNameWithoutExtension(tifPath) + "_Tiles");
                Directory.CreateDirectory(tileOutputDir);

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

                psi.Arguments = $"\"{gdalScriptPath}\" -p mercator -w none \"{tifPath}\" \"{tileOutputDir}\"";
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                
                using (Process proc = Process.Start(psi))
                {
                    proc.WaitForExit();
                }

                // 上传切片成果到SFTP服务器对应的results成果目录
                string folderName = Path.GetFileNameWithoutExtension(tifPath);
                string remoteTargetDir = task.result_path + "/" + folderName;

                var srv = AppConfig.Instance.Server;
                using (var sftp = new SftpClient(srv.Host, srv.Port, srv.Username, srv.Password))
                {
                    sftp.Connect();
                    UploadDirectoryRecursivelySimple(sftp, tileOutputDir, remoteTargetDir);
                    sftp.Disconnect();
                }

                // 发送UOS前段网关HTTP回调接口，通知前端渲染刷新
                UosApiClient.NotifyUosBuildComplete(task.exec_id, task.result_path, folderName).Wait();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"自动化切片失败：{ex.Message}");
            }
        }

        private bool WaitUntilFileReadySimple(string filePath, int timeoutSeconds)
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
                    Thread.Sleep(500); // 被占用则每隔半秒重试
                }
            }
            return false;
        }

        private void UploadDirectoryRecursivelySimple(SftpClient sftp, string localDir, string remoteDir)
        {
            // 安全创建多级远端目录
            string currentPath = "";
            string[] folders = remoteDir.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string folder in folders)
            {
                currentPath += "/" + folder;
                if (!sftp.Exists(currentPath)) sftp.CreateDirectory(currentPath);
            }

            foreach (string file in Directory.GetFiles(localDir))
            {
                using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    sftp.UploadFile(fs, remoteDir + "/" + Path.GetFileName(file), true);
                }
            }

            foreach (string subDir in Directory.GetDirectories(localDir))
            {
                UploadDirectoryRecursivelySimple(sftp, subDir, remoteDir + "/" + Path.GetFileName(subDir));
            }
        }
        #endregion

        private void UpdateTaskDropdown(List<TaskInfo> latestStartTasks)
        {
            string previouslySelectedExecId = null;
            if (cboPendingTasks.SelectedItem is TaskInfo currentSelected)
            {
                previouslySelectedExecId = currentSelected.exec_id;
            }

            _pendingTasks.Clear();
            _pendingTasks.AddRange(latestStartTasks);

            _taskBindingSource.DataSource = null;
            _taskBindingSource.DataSource = _pendingTasks;

            if (!string.IsNullOrEmpty(previouslySelectedExecId))
            {
                for (int i = 0; i < cboPendingTasks.Items.Count; i++)
                {
                    if (cboPendingTasks.Items[i] is TaskInfo task && task.exec_id == previouslySelectedExecId)
                    {
                        cboPendingTasks.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        private void btnOpenDownloader_Click(object sender, EventArgs e)
        {
            TaskInfo selectedTask = cboPendingTasks.SelectedItem as TaskInfo;
            if (selectedTask == null)
            {
                MessageBox.Show("请先在下拉框中选择一个任务！");
                return;
            }

            // 打开下传工具，并把任务信息传进去
            FrmImageDownloader downloader = new FrmImageDownloader(selectedTask);
            downloader.Show();
        }

        private void btnOpenUploader_Click(object sender, EventArgs e)
        {
            gdal2tiles slicer = new gdal2tiles();
            slicer.Show();
        }

        private void UpdateStatus(string msg)
        {
            if (InvokeRequired) Invoke(new Action(() => lblStatus.Text = msg));
            else lblStatus.Text = msg;
        }

        public static void ReleaseAllLocalLocks()
        {
            if (_runningAutomationTasks.Count == 0) return;

            try
            {
                // 判断最后一次记录的info路径是本地路径还是Linux远程路径
                bool isLocalPath = _lastInfoPath.Contains(":") || !_lastInfoPath.StartsWith("/");

                if (isLocalPath)
                {
                    foreach (var execId in _runningAutomationTasks.Keys)
                    {
                        string lockFile = Path.Combine(_lastInfoPath, execId + ".lock");
                        string doneFile = Path.Combine(_lastInfoPath, execId + ".done");

                        if (File.Exists(lockFile))
                        {
                            if (File.Exists(doneFile)) File.Delete(doneFile);
                            File.Move(lockFile, doneFile);
                        }
                    }
                }
                else
                {
                    var srv = AppConfig.Instance.Server;
                    using (var sftp = new SftpClient(srv.Host, srv.Port, srv.Username, srv.Password))
                    {
                        sftp.Connect();

                        foreach (var execId in _runningAutomationTasks.Keys)
                        {
                            string lockFile = _lastInfoPath + "/" + execId + ".lock";
                            string doneFile = _lastInfoPath + "/" + execId + ".done";

                            if (sftp.Exists(lockFile))
                            {
                                if (sftp.Exists(doneFile)) sftp.DeleteFile(doneFile);

                                sftp.RenameFile(lockFile, doneFile);
                            }
                        }
                        sftp.Disconnect();
                    }
                }
                    
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"全局分布式锁释放失败：{ex.Message}");
            }
            finally
            {
                _runningAutomationTasks.Clear();
            }
        }

        private void FrmMainControl_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cts?.Cancel();
            ReleaseAllLocalLocks();
        }

        private void CopyToSharedDrive(string srcTifPath, string execId)
        {
            try
            {
                string dateStr = DateTime.Now.ToString("yyyyMMdd");
                string targetBaseDir = Path.Combine(AppConfig.Instance.Paths.SharedOutputRoot, $"Sfm_{dateStr}");
                string targetExecDir = Path.Combine(targetBaseDir, execId);

                if (!Directory.Exists(targetExecDir))
                {
                    Directory.CreateDirectory(targetExecDir);
                }

                string fileName = Path.GetFileName(srcTifPath);
                string destFilePath = Path.Combine(targetExecDir, fileName);
                string tempFilePath = destFilePath + ".tmp";

                File.Copy(srcTifPath, tempFilePath, true);

                if (File.Exists(destFilePath))
                {
                    File.Delete(destFilePath);
                }
                File.Move(tempFilePath, destFilePath);

                //Debug.WriteLine($"[共享盘同步成功] {fileName} -> {destFilePath}");
            }
            catch (Exception ex)
            {
                // 捕获异常，防止共享盘不可用时导致你本地的核心成图流水线崩溃
                //Debug.WriteLine($"[共享盘同步失败] 任务ID：{execId}, 详情：{ex.Message}");
            }
        }
    }
}
