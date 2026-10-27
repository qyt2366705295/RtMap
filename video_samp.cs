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

namespace RtMap
{
    public partial class video_samp : Form
    {
        public video_samp()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void video_samp_Load(object sender, EventArgs e)
        {

        }

        private void txtInputDir_TextChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void txtOutputDir_TextChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnVideo_Samp_Click(object sender, EventArgs e)
        {
            string videoPath = txtInputDir.Text;
            string outputPath = txtOutputDir.Text;
            string extractParam = numParam.Value.ToString();

            if (string.IsNullOrWhiteSpace(videoPath) || string.IsNullOrWhiteSpace(outputPath) || string.IsNullOrWhiteSpace(extractParam))
            {
                MessageBox.Show("请确保视频路径、输出路径和参数都已填写完整！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string exePath = AppConfig.Instance.Paths.VideoSamplerExe;
            if (!File.Exists(exePath))
            {
                MessageBox.Show("找不到抽帧程序，请检查系统配置！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = exePath;
                psi.Arguments = $"\"{videoPath}\" \"{outputPath}\" {extractParam}";

                psi.CreateNoWindow = false;
                psi.UseShellExecute = false;

                Process.Start(psi);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("启动抽帧程序失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBrowseInput_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "视频文件(*.mp4)|*.mp4|所有文件(*.*)|*.*";
                ofd.Title = "请选择需要抽帧的MP4视频";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtInputDir.Text = ofd.FileName;
                }
            }
        }

        private void btnBrowseOutput_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "请选择输出目录";
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtOutputDir.Text = fbd.SelectedPath;
                }
            }
        }

        private void numParam_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
