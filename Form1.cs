using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace RtMap
{
    public partial class RtMap : Form
    {
        public RtMap()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void txtInputDir_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnBrowseInput_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "请选择无人机影像所在目录";
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtInputDir.Text = fbd.SelectedPath; // 将选中的路径显示在文本框中
                    CheckPathsAndEnableButtons();
                }
            }
        }

        private void CheckPathsAndEnableButtons()
        {
            bool isReady = !string.IsNullOrWhiteSpace(txtInputDir.Text) &&
                           !string.IsNullOrWhiteSpace(txtOutputDir.Text);

            //btnRtMap_Mip.Enabled = isReady;
            btnRtMap_Slm.Enabled = isReady;
            btnRtMap_Sfm.Enabled = isReady;
            btnRtSen_Las.Enabled = isReady;
            //btnRtSen_Mip.Enabled = isReady;
        }

        private void btnBrowseOutput_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "请选择输出目录";
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtOutputDir.Text = fbd.SelectedPath;
                    CheckPathsAndEnableButtons();
                }
            }
        }

        private Process viewerServerProcess = null;
        private void btnRtViewer_Click(object sender, EventArgs e)
        {
            try
            {
                //if (viewerServerProcess != null && !viewerServerProcess.HasExited)
                //{
                //    MessageBox.Show("RtViewer已经在运行中！");
                //    return;
                //}

                ProcessStartInfo serverInfo = new ProcessStartInfo();
                serverInfo.FileName = @"D:\software\RtMapView\Release\RtMapView\server.exe";
                serverInfo.WorkingDirectory = @"D:\software\RtMapView\Release\RtMapView";
                
                // 根据情况，如果不想显示黑框，可以隐藏它
                serverInfo.CreateNoWindow = true;
                serverInfo.UseShellExecute = false;
                //serverInfo.WindowStyle = ProcessWindowStyle.Hidden; 
                //Process.Start(serverInfo);
                viewerServerProcess = Process.Start(serverInfo);

                // 等待一小会儿，确保服务启动完成
                System.Threading.Thread.Sleep(1500);

                /*string url = "http://localhost:8080/index.html";
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });*/
            }
            catch (Exception ex)
            {
                MessageBox.Show("RtViewer启动失败：" + ex.Message);
            }
        }

        private string ConvertToWslPath(string winPath)
        {
            if (string.IsNullOrWhiteSpace(winPath)) return string.Empty;

            // 提取盘符并转小写
            string drive = winPath.Substring(0, 1).ToLower();
            string restPath = winPath.Substring(3).Replace("\\", "/");

            return $"/mnt/{drive}/{restPath}";
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

        private void btnRtMap_Slm_Click(object sender, EventArgs e)
        {
            string imgPath = txtInputDir.Text;
            string outputPath = txtOutputDir.Text;

            if (string.IsNullOrWhiteSpace(imgPath) || string.IsNullOrWhiteSpace(outputPath))
            {
                MessageBox.Show("请先选择输入和输出目录！");
                return;
            }

            try
            {
                DirectoryInfo workDirInfo = Directory.GetParent(imgPath);
                if (workDirInfo == null)
                {
                    MessageBox.Show("影像目录不能是根目录！");
                    return;
                }

                string workdirPath = workDirInfo.FullName;
                string iniFilePath = Path.Combine(workdirPath, "config", "config.ini");

                string imagesWslPath = ConvertToWslPath(imgPath);
                string workdirWslPath = ConvertToWslPath(workdirPath);
                string outputWslPath = ConvertToWslPath(outputPath);

                UpdateSlmConfigIni(iniFilePath, imagesWslPath, workdirWslPath, outputWslPath);

                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "wsl.exe";
                string wslDistroName = "Ubuntu-22.04";
                string linuxCodeDir = "/home/dys/code/orthophoto/";

                //psi.Arguments = $"-d {wslDistroName} --cd {linuxCodeDir} -e python3 main.py --workdir {workdirWslPath}";
                string pythonCmd = $"python3 main.py --workdir {workdirWslPath}";
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

            }
            catch (Exception ex)
            {
                MessageBox.Show("RtMap_Slm启动失败：" + ex.Message);
            }

        }

        private void txtOutputDir_TextChanged(object sender, EventArgs e)
        {

        }

        private void UpdateMipMap2DConfig(string outPath)
        {
            try
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string configPath = Path.Combine(appDataPath, "mipmap-realtime-2d", "config.json");

                if (!File.Exists(configPath))
                {
                    MessageBox.Show("未找到RtMap_Mip的config.json文件，请确保该软件至少运行过一次。");
                    return;
                }

                string jsonContent = File.ReadAllText(configPath);

                string escapedPath = outPath.Replace("\\", "\\\\");

                string pattern = @"(""workspace""\s*:\s*"")[^""]*("")";
                string newJsonContent = Regex.Replace(jsonContent, pattern, "${1}" + escapedPath + "${2}");

                File.WriteAllText(configPath, newJsonContent);
            }
            catch (Exception ex)
            {
                MessageBox.Show("修改RtMap_Mip配置文件失败：" + ex.Message);
            }
        }

        private void btnRtMap_Mip_Click(object sender, EventArgs e)
        {
            string outPath = txtOutputDir.Text;
            if (!string.IsNullOrWhiteSpace(outPath)) {
                UpdateMipMap2DConfig(outPath);
            }

            try
            {
                ProcessStartInfo mipInfo = new ProcessStartInfo();
                mipInfo.FileName = @"D:\software\Mipmap\RtMap_Mip\RtMap-Mip.exe";
                Process.Start(mipInfo);
                System.Threading.Thread.Sleep(1500);
            }
            catch(Exception ex)
            {
                MessageBox.Show("RtMap_Mip启动失败：" + ex.Message);
            }
        }

        private void btnRtMap_Sfm_Click(object sender, EventArgs e)
        {
            //string exePath = @"D:\MSVCProj\sfm_onthefly\x64\RelWithDebInfo\UI.exe";
            //string workDir = @"D:\MSVCProj\sfm_onthefly\x64\RelWithDebInfo";
            string exePath = @"D:\MSVCProj\sfm_onthefly\x64\Release\UI.exe";
            string workDir = @"D:\MSVCProj\sfm_onthefly\x64\Release";
            //string thirdPartyPath = @"D:\MSVCProj\sfm_onthefly\thirdparty";
            //string baseLibPath = @"D:\MSVCProj\BaseLib";

            if (!File.Exists(exePath))
            {
                MessageBox.Show("找不到RtMap_Sfm，请检查路径是否正确！");
                return;
            }

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = exePath;
                psi.WorkingDirectory = workDir;

                //psi.UseShellExecute = false;
                //string currentPath = psi.EnvironmentVariables["PATH"];

                //string newPath = $"{thirdPartyPath};{baseLibPath};{currentPath}";
                //psi.EnvironmentVariables["PATH"] = newPath;

                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show("RtMap_Sfm 启动失败：" + ex.Message);
            }
        }

        private void UpdateLasSettingIni(string iniPath, string lasDir, string mapDir, string outDir)
        {
            if (!File.Exists(iniPath))
            {
                throw new FileNotFoundException("找不到配置文件：" + iniPath);
            }

            // 将反斜杠替换为正斜杠
            lasDir = lasDir.Replace("\\", "/");
            mapDir = mapDir.Replace("\\", "/");
            outDir = outDir.Replace("\\", "/");

            string[] lines = File.ReadAllLines(iniPath);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("LasDirectory="))
                {
                    lines[i] = "LasDirectory=" + lasDir;
                }
                else if (lines[i].StartsWith("MapDirectory="))
                {
                    lines[i] = "MapDirectory=" + mapDir;
                }
                else if (lines[i].StartsWith("OutDirectory="))
                {
                    lines[i] = "OutDirectory=" + outDir;
                }
            }

            File.WriteAllLines(iniPath, lines);
        }

        private void RunLasInstance(string moduleFolder, string lasDir, string mapDir, string outDir, ref Process targetProcess)
        {
            try
            {
                string iniPath = Path.Combine(moduleFolder, "setting.ini");
                UpdateLasSettingIni(iniPath, lasDir, mapDir, outDir);
                string exePath = Path.Combine(moduleFolder, "三维重建.exe");

                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = exePath;
                psi.WorkingDirectory = moduleFolder;

                // psi.CreateNoWindow = true;
                // psi.UseShellExecute = false;

                targetProcess = Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"从{moduleFolder}启动RtSen_Las失败：" + ex.Message);
            }
        }

        private Process processLasA = null;
        private Process processLasB = null;
        private void btnRtSen_Las_Click(object sender, EventArgs e)
        {
            string lasPath = txtInputDir.Text;
            string outPath = txtOutputDir.Text;
            if (string.IsNullOrWhiteSpace(lasPath) || string.IsNullOrWhiteSpace(outPath))
            {
                MessageBox.Show("请先选择输入目录和输出目录！");
                return;
            }

            bool isSlotAFree = (processLasA == null || processLasA.HasExited);
            bool isSlotBFree = (processLasB == null || processLasB.HasExited);

            string mapPath = "";
            try
            {
                // 获取输入目录的上级目录
                DirectoryInfo parentDir = Directory.GetParent(lasPath);

                if (parentDir != null)
                {
                    mapPath = Path.Combine(parentDir.FullName, "maps");
                }
                else
                {
                    MessageBox.Show("输入目录不能是磁盘根目录，无法推导maps影像目录！");
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("解析影像目录路径时出错：" + ex.Message);
                return;
            }

            string moduleFolderA = @"D:\MODEL";
            string moduleFolderB = @"D:\software\MODEL";
            if (isSlotAFree)
            {
                RunLasInstance(moduleFolderA, lasPath, mapPath, outPath, ref processLasA);
            }
            else if (isSlotBFree)
            {
                RunLasInstance(moduleFolderB, lasPath, mapPath, outPath, ref processLasB);
            }
            else
            {
                MessageBox.Show("系统繁忙！当前已达到最大并发数，请等待其中一个任务完成后再试。");
            }

            /*
            string moduleFolder = @"D:\MODEL";
            string iniFilePath = Path.Combine(moduleFolder, "setting.ini");
            string exeFilePath = Path.Combine(moduleFolder, "三维重建.exe");

            try
            {
                UpdateLasSettingIni(iniFilePath, lasPath, mapPath, outPath);

                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = exeFilePath;
                psi.WorkingDirectory = moduleFolder;

                // psi.CreateNoWindow = true;
                // psi.UseShellExecute = false;

                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show("RtSen_Las启动失败：" + ex.Message);
            }*/

        }

        private void RtMap_FormClosing(object sender, FormClosingEventArgs e)
        {
            FrmMainControl.ReleaseAllLocalLocks();
            if (viewerServerProcess != null && !viewerServerProcess.HasExited)
            {
                try
                {
                    viewerServerProcess.Kill();
                }
                catch {}
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void UpdateMipMap3DConfig(string outPath)
        {
            try
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string configPath = Path.Combine(appDataPath, "mipmap-realtime-3d", "config.json");

                if (!File.Exists(configPath))
                {
                    MessageBox.Show("未找到RtSen_Mip的config.json文件，请确保该软件至少运行过一次。");
                    return;
                }

                string jsonContent = File.ReadAllText(configPath);

                string escapedPath = outPath.Replace("\\", "\\\\");

                string pattern = @"(""workspace""\s*:\s*"")[^""]*("")";
                string newJsonContent = Regex.Replace(jsonContent, pattern, "${1}" + escapedPath + "${2}");

                File.WriteAllText(configPath, newJsonContent);
            }
            catch (Exception ex)
            {
                MessageBox.Show("修改RtSen_Mip配置文件失败：" + ex.Message);
            }
        }

        private void btnRtSen_Mip_Click(object sender, EventArgs e)
        {
            string outPath = txtOutputDir.Text;
            if (!string.IsNullOrWhiteSpace(outPath))
            {
                UpdateMipMap3DConfig(outPath);
            }

            try
            {
                ProcessStartInfo mipInfo = new ProcessStartInfo();
                mipInfo.FileName = @"D:\software\Mipmap\RtSen_Mip\RtSen-Mip.exe";
                Process.Start(mipInfo);
                System.Threading.Thread.Sleep(1500);
            }
            catch (Exception ex)
            {
                MessageBox.Show("RtSen_Mip启动失败：" + ex.Message);
            }
        }

        private void btnVideo_Samp_Click(object sender, EventArgs e)
        {
            video_samp toolForm = new video_samp();
            toolForm.Show();
        }

        private void btnGdal2tiles_Click(object sender, EventArgs e)
        {
            gdal2tiles toolForm = new gdal2tiles();
            toolForm.Show();
        }

        private void btnImageDownloader_Click(object sender, EventArgs e)
        {
            FrmImageDownloader downloaderForm = new FrmImageDownloader();
            downloaderForm.Show();
        }

        private void btnMainControl_Click(object sender, EventArgs e)
        {
            FrmMainControl mainControl = new FrmMainControl();
            mainControl.Show();
        }

        private void btn3DControl_Click(object sender, EventArgs e)
        {
            FrmPointCloud3D mainControl = new FrmPointCloud3D();
            mainControl.Show();
        }
    }
}
