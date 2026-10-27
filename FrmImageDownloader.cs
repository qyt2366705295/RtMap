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
    public partial class FrmImageDownloader : Form
    {
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isMonitoring = false;
        private TaskInfo _currentTask;

        // 全局静态字典：用于主控制台接收到STOP时，强行掐断对应的下传线程
        public static ConcurrentDictionary<string, CancellationTokenSource> ActiveDownloads = new ConcurrentDictionary<string, CancellationTokenSource>();
        public static ConcurrentDictionary<string, bool> SoftStopSignals = new ConcurrentDictionary<string, bool>();

        // 记忆库，用来记录哪些远程文件已经下载过了
        private HashSet<string> _downloadedFiles = new HashSet<string>();

        public FrmImageDownloader()
        {
            InitializeComponent();
            _currentTask = null; // 标记为无任务模式
            this.Text = "影像下传工具 - 手动独立模式";

            // 手动模式下，允许用户自由修改服务器路径
            var MonitorCfg = AppConfig.Instance.Monitor;
            txtInputDir.Text = MonitorCfg.DefaultRemotePictureDir;
            txtInputDir.Enabled = true;

            try
            {

                txtOutputDir.Text = Path.Combine(AppConfig.Instance.Paths.ImageRoot, "Manual_Download");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"生成默认手动路径失败: {ex.Message}");
            }

            SetupUI();
        }

        public FrmImageDownloader(TaskInfo task)
        {
            InitializeComponent();
            _currentTask = task; // 记录档案袋
            this.Text = $"影像下传工具 - 当前工程：{task.exec_id}";

            // 自动化模式下，自动填入影像路径，并锁定禁止修改
            txtInputDir.Text = task.image_path;
            txtInputDir.Enabled = false;

            try
            {
                string defaultLocalPath = Path.Combine(AppConfig.Instance.Paths.ImageRoot, task.exec_id);
                txtOutputDir.Text = defaultLocalPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"自动生成下传路径失败：{ex.Message}");
            }

            SetupUI();
        }

        private void SetupUI()
        {
            cboCameraType.Items.Clear();

            try
            {
                var config = CameraMapConfig.Load();
                foreach (var mapping in config.Mappings)
                {
                    if (mapping.Id == "DJI_Video") continue;
                    cboCameraType.Items.Add(mapping.Id);
                }
            }
            catch (Exception ex)
            {
                // 如果JSON读不到，保留硬编码兜底
                cboCameraType.Items.Add("S6100_X_40");
                cboCameraType.Items.Add("M3M");
                cboCameraType.Items.Add("P4M");
            }
            cboCameraType.Items.Add("Else");

            rdoSfm.CheckedChanged += (s, e) => cboCameraType.Enabled = rdoSfm.Checked;
            cboCameraType.SelectedIndex = 0;
            this.FormClosing += FrmImageDownloader_FormClosing;

            chkLocalSource.CheckedChanged += ChkLocalSource_CheckedChanged;
            ToggleSourcePathMode();
        }

        private void ChkLocalSource_CheckedChanged(object sender, EventArgs e)
        {
            ToggleSourcePathMode();
        }

        private void ToggleSourcePathMode()
        {
            if (_isMonitoring) return;

            if (chkLocalSource.Checked)
            {
                label1.Text = "本地输入路径";
                txtInputDir.Text = AppConfig.Instance.Paths.LocalInputRoot;
            }
            else
            {
                label1.Text = "服务器路径";
                var MonitorCfg = AppConfig.Instance.Monitor;
                txtInputDir.Text = MonitorCfg.DefaultRemotePictureDir;
            }
        }

        private void btnBrowseOutput_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "请选择影像下传的本地保存目录";
                if (fbd.ShowDialog() == DialogResult.OK)
                    txtOutputDir.Text = fbd.SelectedPath;
            }
        }

        private void btnImageDownloader_Click(object sender, EventArgs e)
        {
            if (!_isMonitoring)
            {
                string remotePath = txtInputDir.Text;
                string localPath = txtOutputDir.Text;

                if (string.IsNullOrWhiteSpace(remotePath) || string.IsNullOrWhiteSpace(localPath))
                {
                    MessageBox.Show("请确保远程路径和本地目录都已填写完整！");
                    return;
                }

                bool isVideoTask = false;
                string targetVideoPath = null;

                if (chkLocalSource.Checked)
                {
                    if (Uri.TryCreate(remotePath, UriKind.Absolute, out Uri uriResult) &&
                        (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps || uriResult.Scheme == "rtsp"))
                    {
                        targetVideoPath = remotePath; // 直接将网络URL链接作为输入源投递给抽帧引擎
                        isVideoTask = true;
                    }
                    else if (File.Exists(remotePath) && (remotePath.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) || remotePath.EndsWith(".avi", StringComparison.OrdinalIgnoreCase)))
                    {
                        targetVideoPath = remotePath;
                        isVideoTask = true;
                    }
                    else if (Directory.Exists(remotePath))
                    {
                        var videoFiles = Directory.GetFiles(remotePath).Where(f => f.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".avi", StringComparison.OrdinalIgnoreCase)).ToList();
                        if (videoFiles.Count > 0)
                        {
                            targetVideoPath = videoFiles[0];
                            isVideoTask = true;
                        }
                    }
                }

                if (isVideoTask)
                {
                    // 视频模式：创建本地工作目录
                    try
                    {
                        Directory.CreateDirectory(localPath);
                        string sourceXml = Path.Combine(AppConfig.Instance.Paths.CameraParametersRoot, "DJI_Video", "camera.xml");
                        string targetXml = Path.Combine(localPath, "camera.xml");
                        if (File.Exists(sourceXml))
                        {
                            File.Copy(sourceXml, targetXml, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"部署视频相机参数失败：{ex.Message}");
                        return;
                    }
                }
                else
                {
                    // 部署算法环境（拷贝配置）
                    if (!DeployAlgorithmConfig(localPath)) return;
                }

                _cancellationTokenSource = new CancellationTokenSource();
                if (_currentTask != null)
                {
                    // 写入本地桥梁，并注册到全局字典受主控台管控
                    TaskBridge.SaveOrUpdateTask(_currentTask);
                    ActiveDownloads.TryAdd(_currentTask.exec_id, _cancellationTokenSource);
                }
                else
                {
                    Console.WriteLine("当前为手动独立模式，不计入自动化流水线记录。");
                }

                btnImageDownloader.Text = "停止下传";
                btnImageDownloader.BackColor = System.Drawing.Color.LightCoral;
                txtInputDir.Enabled = false;
                txtOutputDir.Enabled = false;
                chkLocalSource.Enabled = false;
                _isMonitoring = true;
                lblStatus.Text = "开始实时监控影像...";

                if (chkLocalSource.Checked)
                {
                    if (isVideoTask)
                    {
                        lblStatus.Text = "本地视频流模式启动，正在后台实时抽帧...";
                        // 启动全新的非阻塞本地视频流实时抽帧监控线程
                        Task.Run(() => MonitorLocalVideoStreaming(targetVideoPath, localPath, _cancellationTokenSource.Token));
                    }
                    else
                    {
                        lblStatus.Text = "本地离线模式启动，正在监控输入目录...";
                        // 启动纯本地离线监视线程
                        Task.Run(() => MonitorLocalSourceDirectory(remotePath, localPath, _cancellationTokenSource.Token));
                    }
                        
                }
                else
                {
                    lblStatus.Text = "正在连接服务器，开始实时监控云端影像...";
                    Task.Run(() => MonitorRemoteDirectory(remotePath, localPath, _cancellationTokenSource.Token));
                }
                    
            }
            else
            {
                StopMonitoring();

                btnImageDownloader.Text = "开始下传";
                btnImageDownloader.BackColor = System.Drawing.Color.LightSkyBlue;
                if (_currentTask == null) txtInputDir.Enabled = true;
                txtOutputDir.Enabled = true;
                chkLocalSource.Enabled = true;
                _isMonitoring = false;
                lblStatus.Text = "已停止监控。";
            }
        }

        private void StopMonitoring()
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
            }

            // 如果是自动化模式，从全局活动字典中注销
            if (_currentTask != null)
            {
                ActiveDownloads.TryRemove(_currentTask.exec_id, out _);
            }
        }

        private bool DeployAlgorithmConfig(string localImageDir)
        {
            try
            {
                Directory.CreateDirectory(localImageDir);
                string parentDir = Directory.GetParent(localImageDir).FullName;

                // 存放标准配置文件库的路径
                string baseTemplateDir = AppConfig.Instance.Paths.CameraParametersRoot;

                if (rdoSlm.Checked)
                {
                    // Slm：拷贝config文件夹到影像上级目录
                    string sourceConfig = Path.Combine(baseTemplateDir, "config");
                    string targetConfig = Path.Combine(parentDir, "config");
                    CopyDirectory(sourceConfig, targetConfig);
                }
                else if (rdoSfm.Checked)
                {
                    // Sfm：拷贝对应相机的camera.xml到影像目录
                    string selectedCamera = cboCameraType.SelectedItem.ToString();
                    if (selectedCamera == "Else")
                    {
                        Console.WriteLine("跳过camera.xml拷贝流程。");
                        return true;
                    }

                    // 从外置配置中查找对应的参数目录名
                    string paramDir = selectedCamera;
                    try
                    {
                        var config = CameraMapConfig.Load();
                        var mapping = config.Mappings.FirstOrDefault(m => m.Id == selectedCamera);
                        if (mapping != null && !string.IsNullOrEmpty(mapping.ParamDir))
                        {
                            paramDir = mapping.ParamDir;
                        }
                    }
                    catch { /* 读不到配置就按下拉框原名兜底 */ }

                    string sourceXml = Path.Combine(baseTemplateDir, paramDir, "camera.xml");
                    string targetXml = Path.Combine(localImageDir, "camera.xml");

                    if (!File.Exists(sourceXml))
                    {
                        MessageBox.Show($"找不到相机配置文件：{sourceXml}");
                        return false;
                    }
                    File.Copy(sourceXml, targetXml, true);
                }
                // Mip不做处理

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"配置拷贝失败: {ex.Message}");
                return false;
            }
        }

        private void CopyDirectory(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);
            foreach (var file in Directory.GetFiles(sourceDir))
                File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)), true);
            foreach (var dir in Directory.GetDirectories(sourceDir))
                CopyDirectory(dir, Path.Combine(targetDir, Path.GetFileName(dir)));
        }

        // 后台持续轮询线程
        private void MonitorRemoteDirectory(string baseRemoteDir, string baseLocalDir, CancellationToken token)
        {
            string currentExecId = _currentTask?.exec_id;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var srv = AppConfig.Instance.Server;
                    using (var sftp = new SftpClient(srv.Host, srv.Port, srv.Username, srv.Password))
                    {
                        sftp.Connect();

                        if (sftp.Exists(baseRemoteDir))
                        {
                            // 执行递归同步
                            SyncDirectoryRecursively(sftp, baseRemoteDir, baseRemoteDir, baseLocalDir, token);
                        }
                        else
                        {
                            UpdateStatusUI($"警告：远程目录{baseRemoteDir}不存在，等待创建...");
                        }

                        sftp.Disconnect();
                    }
                }
                catch (Exception ex)
                {
                    // 捕获网络闪断等异常，防止线程崩溃
                    UpdateStatusUI($"SFTP同步发生错误：{ex.Message}，正在重试...");
                }

                if (currentExecId != null && SoftStopSignals.TryGetValue(currentExecId, out bool isSoftStopped) && isSoftStopped)
                {
                    UpdateStatusUI($"收到任务完结指令，尾盘数据已全部下传完毕，安全退出监控。");

                    SoftStopSignals.TryRemove(currentExecId, out _);
                    ActiveDownloads.TryRemove(currentExecId, out _);

                    if (this.InvokeRequired)
                    {
                        this.Invoke(new Action(() => {
                            btnImageDownloader.Text = "开始下传";
                            btnImageDownloader.BackColor = System.Drawing.Color.LightSkyBlue;
                            txtOutputDir.Enabled = true;
                            _isMonitoring = false;
                        }));
                    }

                    break;
                }

                try
                {
                    // 每隔1秒扫描一次服务器
                    Task.Delay(1000, token).Wait();
                }
                catch (AggregateException)
                {
                    // Task.Delay被Cancel时会抛出异常，安全退出
                    break;
                }
            }

            Console.WriteLine("影像下传监控线程安全退出。");
        }

        private void MonitorLocalSourceDirectory(string srcDir, string destDir, CancellationToken token)
        {
            string currentExecId = _currentTask?.exec_id;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (!Directory.Exists(srcDir))
                    {
                        UpdateStatusUI($"警告：离线输入目录{srcDir}不存在，等待创建...");
                        Thread.Sleep(1000);
                        continue;
                    }

                    var allFiles = Directory.GetFiles(srcDir)
                        .Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                    f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                                    f.EndsWith(".tif", StringComparison.OrdinalIgnoreCase) ||
                                    f.EndsWith(".tiff", StringComparison.OrdinalIgnoreCase))
                        .OrderBy(f => f)
                        .ToList();

                    var fileGroups = allFiles
                        .Where(f => !_downloadedFiles.Contains(f))
                        .GroupBy(f => {
                            string name = Path.GetFileNameWithoutExtension(f);
                            var match = System.Text.RegularExpressions.Regex.Match(name, @"^DJI_.*_(\d{4})_(D|MS_.*)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                            if (match.Success)
                            {
                                return match.Groups[1].Value;
                            }
                            return name; // 非大疆格式按单张图像成组
                        })
                        .OrderBy(g => g.Key) // 严格按照升序投递，确保实时成图解算不乱序
                        .ToList();

                    foreach (var group in fileGroups)
                    {
                        if (token.IsCancellationRequested) break;

                        List<string> readyToCopyInGroup = new List<string>();
                        List<string> finalDestPaths = new List<string>();
                        List<string> tempDestPaths = new List<string>();

                        foreach (var srcFile in group)
                        {
                            FileInfo fi = new FileInfo(srcFile);
                            // 防抖检查：如果外部塞入程序还没写完当前照片，本轮整个组先略过
                            if ((DateTime.Now - fi.LastWriteTime).TotalSeconds < 2) continue;

                            string fileName = Path.GetFileName(srcFile);
                            string finalDest = Path.Combine(destDir, fileName);
                            string tempDest = finalDest + ".tmp"; // 写入过渡后缀

                            readyToCopyInGroup.Add(srcFile);
                            finalDestPaths.Add(finalDest);
                            tempDestPaths.Add(tempDest);
                        }

                        // 防漏检查：如果某个多光谱波段没写完，本轮拒绝提交，等待文件齐备
                        if (readyToCopyInGroup.Count < group.Count()) continue;

                        for (int i = 0; i < readyToCopyInGroup.Count; i++)
                        {
                            File.Copy(readyToCopyInGroup[i], tempDestPaths[i], true);
                        }

                        for (int i = 0; i < readyToCopyInGroup.Count; i++)
                        {
                            if (File.Exists(finalDestPaths[i])) File.Delete(finalDestPaths[i]);
                            File.Move(tempDestPaths[i], finalDestPaths[i]);

                            _downloadedFiles.Add(readyToCopyInGroup[i]); // 录入记忆库
                        }

                        UpdateStatusUI($"[离线模式] 成功精准清洗并投递影像航点组：{group.Key}");
                    }
                }
                catch (Exception ex)
                {
                    UpdateStatusUI($"本地传输发生异常：{ex.Message}，自动重试中...");
                }

                if (currentExecId != null && SoftStopSignals.TryGetValue(currentExecId, out bool isSoftStopped) && isSoftStopped)
                {
                    UpdateStatusUI($"离线数据传输已全部清洗处理完毕，安全退出监控。");

                    SoftStopSignals.TryRemove(currentExecId, out _);
                    ActiveDownloads.TryRemove(currentExecId, out _);

                    if (this.InvokeRequired)
                    {
                        this.Invoke(new Action(() => {
                            btnImageDownloader.Text = "开始下传";
                            btnImageDownloader.BackColor = System.Drawing.Color.LightSkyBlue;
                            txtOutputDir.Enabled = true;
                            chkLocalSource.Enabled = true;
                            _isMonitoring = false;
                        }));
                    }
                    break;
                }

                try { Task.Delay(1000, token).Wait(); } catch { break; }
            }
        }

        private void MonitorLocalVideoStreaming(string videoPath, string destDir, CancellationToken token)
        {
            string currentExecId = _currentTask?.exec_id;

            try
            {
                ProcessStartInfo videoPsi = new ProcessStartInfo();
                videoPsi.FileName = AppConfig.Instance.Paths.VideoSamplerExe;
                // 传参给抽帧工具，向工作空间目录高频输出frame_开头的照片，时间间隔为0.5秒
                videoPsi.Arguments = $"\"{videoPath}\" \"{destDir}\" 0.5";
                videoPsi.CreateNoWindow = true;
                videoPsi.UseShellExecute = false;

                using (Process vProc = Process.Start(videoPsi))
                {
                    // 后台静默守护进程，直到抽帧程序自然结束，或者接收到点击停止/主控台STOP信号
                    while (!vProc.HasExited)
                    {
                        if (token.IsCancellationRequested)
                        {
                            try { vProc.Kill(); } catch { }
                            break;
                        }

                        if (currentExecId != null && SoftStopSignals.TryGetValue(currentExecId, out bool isSoftStopped) && isSoftStopped)
                        {
                            try { vProc.Kill(); } catch { }
                            break;
                        }

                        Thread.Sleep(500); // 半秒挂起轮询一次进程状态
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateStatusUI($"视频流实时处理异常：{ex.Message}");
            }

            UpdateStatusUI($"本地视频流抽帧已结束，共生成{Directory.GetFiles(destDir, "frame_*").Length}张影像。");

            if (currentExecId != null)
            {
                SoftStopSignals.TryRemove(currentExecId, out _);
                ActiveDownloads.TryRemove(currentExecId, out _);
            }

            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => {
                    btnImageDownloader.Text = "开始下传";
                    btnImageDownloader.BackColor = System.Drawing.Color.LightSkyBlue;
                    txtOutputDir.Enabled = true;
                    chkLocalSource.Enabled = true;
                    _isMonitoring = false;
                }));
            }
        }

        // 递归扫描与动态目录映射
        private void SyncDirectoryRecursively(SftpClient sftp, string currentRemoteDir, string baseRemoteDir, string baseLocalDir, CancellationToken token)
        {
            if (token.IsCancellationRequested) return;

            // 获取当前目录下所有的原始文件/文件夹
            var rawItems = sftp.ListDirectory(currentRemoteDir).ToList();

            var orderedFiles = rawItems
                .Where(f => f.IsRegularFile && !f.Name.StartsWith("."))
                .OrderBy(f => f.Name)
                .ToList();

            var subDirectories = rawItems
                .Where(f => f.IsDirectory && f.Name != "." && f.Name != "..")
                .ToList();

            foreach (var file in orderedFiles)
            {
                if (token.IsCancellationRequested) break;

                string remoteFilePath = file.FullName;

                if (!_downloadedFiles.Contains(remoteFilePath))
                {
                    // 如果文件最后修改时间在3秒内，说明可能还在写入，跳过它等下一轮
                    if ((DateTime.UtcNow - file.LastWriteTimeUtc).TotalSeconds < 3)
                    {
                        continue;
                    }

                    string ext = Path.GetExtension(file.Name).ToLower();
                    if (ext == ".tif" || ext == ".tiff" || ext == ".jpg" || ext == ".png")
                    {
                        // 计算这个文件在服务器上的相对路径
                        string relativeDir = currentRemoteDir.Substring(baseRemoteDir.Length).TrimStart('/');

                        // 拼接出本地对应的目标文件夹路径
                        string localTargetDir = string.IsNullOrEmpty(relativeDir)
                            ? baseLocalDir
                            : Path.Combine(baseLocalDir, relativeDir.Replace("/", "\\"));

                        if (!Directory.Exists(localTargetDir))
                        {
                            Directory.CreateDirectory(localTargetDir);
                        }

                        string localFilePath = Path.Combine(localTargetDir, file.Name);

                        UpdateStatusUI($"正在下传：{file.Name}...");

                        using (Stream fileStream = File.OpenWrite(localFilePath))
                        {
                            sftp.DownloadFile(remoteFilePath, fileStream);
                        }

                        // 下载成功后，将其加入记忆库
                        _downloadedFiles.Add(remoteFilePath);
                        UpdateStatusUI($"下传成功：{file.Name}，继续监控...");
                    }
                }
            }
            foreach (var dir in subDirectories)
            {
                if (token.IsCancellationRequested) break;
                SyncDirectoryRecursively(sftp, dir.FullName, baseRemoteDir, baseLocalDir, token);
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

        private void FrmImageDownloader_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopMonitoring();
        }

        public static void StopDownloadForTask(string execId)
        {
            //if (ActiveDownloads.TryGetValue(execId, out CancellationTokenSource cts))
            //{
            //    cts.Cancel();
            //    ActiveDownloads.TryRemove(execId, out _);
            //}

            SoftStopSignals.TryAdd(execId, true);
            Console.WriteLine($"已接收到{execId}的STOP信号，等待当前轮次扫尾完成后自动退出...");
        }

        private void btnLaunchSfm_Click(object sender, EventArgs e)
        {
            string localImageDir = txtOutputDir.Text;

            if (string.IsNullOrWhiteSpace(localImageDir) || !Directory.Exists(localImageDir))
            {
                MessageBox.Show("当前影像下传目录为空或未生成，请先启动影像下传！");
                return;
            }

            try
            {

                string dateFolder = "Sfm_" + DateTime.Now.ToString("yyyyMMdd");
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string workspaceDir = Path.Combine(AppConfig.Instance.Paths.SfmWorkspaceRoot, dateFolder, $"Proj_{timestamp}");
                string dbPath = Path.Combine(workspaceDir, "proj.db");

                string expectedLocalResultDir = Path.Combine(workspaceDir, "OutputOrtho");

                Directory.CreateDirectory(workspaceDir);
                Directory .CreateDirectory(expectedLocalResultDir);

                if (_currentTask != null)
                {
                    _currentTask.local_result_path = expectedLocalResultDir;
                    TaskBridge.SaveOrUpdateTask(_currentTask); // 更新本地json文件
                }

                string exePath = AppConfig.Instance.Paths.OtFRunnerExe;
                string workDir = Path.GetDirectoryName(exePath);

                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = exePath;
                psi.Arguments = $"\"{dbPath}\" \"{localImageDir}\"";
                psi.WorkingDirectory = workDir;

                psi.UseShellExecute = false;
                psi.CreateNoWindow = false;

                Process.Start(psi);

                lblStatus.Text = $"已成功启动RtMap_Sfm处理引擎。";

            }
            catch (Exception ex)
            {
                MessageBox.Show($"启动RtMap_Sfm失败: {ex.Message}");
            }
        }

        private string ConvertToWslPath(string winPath)
        {
            return AppConfig.Instance.Wsl.ConvertWindowsPath(winPath);
        }

        private void UpdateSlmConfigIni(string iniPath, string wslImages, string wslWorkdir, string wslOutput)
        {
            if (!File.Exists(iniPath))
            {
                throw new FileNotFoundException("找不到配置文件：" + iniPath);
            }

            string[] lines = File.ReadAllLines(iniPath);

            for (int i = 0; i < lines.Length; i++)
            {
                string trimmedLine = lines[i].Trim();
                if (trimmedLine.StartsWith("images =") || trimmedLine.StartsWith("images="))
                {
                    lines[i] = $"images = \"{wslImages}\"";
                }
                else if (trimmedLine.StartsWith("workdir =") || trimmedLine.StartsWith("workdir="))
                {
                    lines[i] = $"workdir = \"{wslWorkdir}\"";
                }
                else if (trimmedLine.StartsWith("output =") || trimmedLine.StartsWith("output="))
                {
                    lines[i] = $"output = \"{wslOutput}\"";
                }
            }

            File.WriteAllLines(iniPath, lines);
        }

        private void btnLaunchSlm_Click(object sender, EventArgs e)
        {
            string localImageDir = txtOutputDir.Text;

            if (string.IsNullOrWhiteSpace(localImageDir) || !Directory.Exists(localImageDir))
            {
                MessageBox.Show("当前影像下传目录为空或未生成，请先启动影像下传！");
                return;
            }

            try
            {
                DirectoryInfo workDirInfo = Directory.GetParent(localImageDir);
                if (workDirInfo == null)
                {
                    MessageBox.Show("影像目录不能是根目录！");
                    return;
                }
                string workdirPath = workDirInfo.FullName;

                // Slm 配置位于影像目录的上级目录。
                string iniFilePath = Path.Combine(workdirPath, "config", "config.ini");
                if (!File.Exists(iniFilePath))
                {
                    MessageBox.Show($"找不到标准的Slm配置文件：{iniFilePath}\n请确认之前是否成功勾选并执行了[Slm配置环境]拷贝！");
                    return;
                }

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string outputDir = Path.Combine(AppConfig.Instance.Paths.SfmWorkspaceRoot, $"Slm_{timestamp}");

                string expectedLocalResultDir = Path.Combine(outputDir, "gtiff");

                Directory.CreateDirectory(outputDir);
                Directory.CreateDirectory(expectedLocalResultDir);

                string imagesWslPath = ConvertToWslPath(localImageDir);
                string workdirWslPath = ConvertToWslPath(workdirPath);
                string outputWslPath = ConvertToWslPath(outputDir);

                UpdateSlmConfigIni(iniFilePath, imagesWslPath, workdirWslPath, outputWslPath);

                if (_currentTask != null)
                {
                    _currentTask.local_result_path = expectedLocalResultDir;
                    TaskBridge.SaveOrUpdateTask(_currentTask); // 更新本地缓存json
                }

                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = AppConfig.Instance.Paths.WslExe;
                string wslDistroName = AppConfig.Instance.Wsl.Distribution;
                string linuxCodeDir = AppConfig.Instance.Wsl.CodeDirectory;

                string pythonCmd = $"{AppConfig.Instance.Wsl.PythonCommand} main.py --workdir {workdirWslPath}";
                string bashScript = $"echo '----------------------------------------'; " +
                                    $"echo '二维实时成图模块RtMap_Slm已就绪'; " +
                                    $"echo '即将执行代码: {pythonCmd}'; " +
                                    $"echo '----------------------------------------'; " +
                                    $"echo ''; " +
                                    $"read -p '【请按回车键开始运行...】'; " +
                                    $"{pythonCmd}; " +
                                    $"exec bash";

                psi.Arguments = $"-d {wslDistroName} --cd {linuxCodeDir} -e bash -c \"{bashScript}\"";
                psi.CreateNoWindow = false;
                psi.UseShellExecute = false;
                psi.WindowStyle = ProcessWindowStyle.Normal;

                Process.Start(psi);

                lblStatus.Text = $"已成功启动RtMap_Slm处理引擎。";
            }
            catch (Exception ex)
            {
                MessageBox.Show("启动RtMap_Slm失败：" + ex.Message);
            }
        }
    }
}
