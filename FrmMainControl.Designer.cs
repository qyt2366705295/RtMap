namespace RtMap
{
    partial class FrmMainControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.txtInfoPath = new System.Windows.Forms.TextBox();
            this.btnStartMonitor = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.cboPendingTasks = new System.Windows.Forms.ComboBox();
            this.btnOpenDownloader = new System.Windows.Forms.Button();
            this.btnOpenUploader = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.numMaxThreads = new System.Windows.Forms.NumericUpDown();
            this.chkDemoMode = new System.Windows.Forms.CheckBox();
            this.chkLocalMode = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxThreads)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(69, 69);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(143, 33);
            this.label1.TabIndex = 24;
            this.label1.Text = "通知目录";
            // 
            // txtInfoPath
            // 
            this.txtInfoPath.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtInfoPath.Location = new System.Drawing.Point(235, 60);
            this.txtInfoPath.Name = "txtInfoPath";
            this.txtInfoPath.Size = new System.Drawing.Size(350, 43);
            this.txtInfoPath.TabIndex = 25;
            this.txtInfoPath.Text = "";
            // 
            // btnStartMonitor
            // 
            this.btnStartMonitor.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnStartMonitor.Location = new System.Drawing.Point(620, 60);
            this.btnStartMonitor.Name = "btnStartMonitor";
            this.btnStartMonitor.Size = new System.Drawing.Size(153, 46);
            this.btnStartMonitor.TabIndex = 26;
            this.btnStartMonitor.Text = "开始监控";
            this.btnStartMonitor.UseVisualStyleBackColor = true;
            this.btnStartMonitor.Click += new System.EventHandler(this.btnStartMonitor_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(82, 140);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(111, 33);
            this.label2.TabIndex = 29;
            this.label2.Text = "任务ID";
            // 
            // cboPendingTasks
            // 
            this.cboPendingTasks.Font = new System.Drawing.Font("宋体", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cboPendingTasks.FormattingEnabled = true;
            this.cboPendingTasks.Location = new System.Drawing.Point(235, 135);
            this.cboPendingTasks.Name = "cboPendingTasks";
            this.cboPendingTasks.Size = new System.Drawing.Size(350, 41);
            this.cboPendingTasks.TabIndex = 30;
            // 
            // btnOpenDownloader
            // 
            this.btnOpenDownloader.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btnOpenDownloader.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnOpenDownloader.Location = new System.Drawing.Point(75, 370);
            this.btnOpenDownloader.Name = "btnOpenDownloader";
            this.btnOpenDownloader.Size = new System.Drawing.Size(214, 46);
            this.btnOpenDownloader.TabIndex = 31;
            this.btnOpenDownloader.Text = "影像下传";
            this.btnOpenDownloader.UseVisualStyleBackColor = false;
            this.btnOpenDownloader.Click += new System.EventHandler(this.btnOpenDownloader_Click);
            // 
            // btnOpenUploader
            // 
            this.btnOpenUploader.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btnOpenUploader.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnOpenUploader.Location = new System.Drawing.Point(556, 370);
            this.btnOpenUploader.Name = "btnOpenUploader";
            this.btnOpenUploader.Size = new System.Drawing.Size(214, 46);
            this.btnOpenUploader.TabIndex = 32;
            this.btnOpenUploader.Text = "正射Tif切片";
            this.btnOpenUploader.UseVisualStyleBackColor = false;
            this.btnOpenUploader.Click += new System.EventHandler(this.btnOpenUploader_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblStatus.Location = new System.Drawing.Point(69, 470);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(0, 33);
            this.lblStatus.TabIndex = 33;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(69, 210);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(143, 33);
            this.label3.TabIndex = 34;
            this.label3.Text = "最大线程";
            // 
            // numMaxThreads
            // 
            this.numMaxThreads.Font = new System.Drawing.Font("宋体", 15.75F);
            this.numMaxThreads.Location = new System.Drawing.Point(235, 205);
            this.numMaxThreads.Maximum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.numMaxThreads.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numMaxThreads.Name = "numMaxThreads";
            this.numMaxThreads.Size = new System.Drawing.Size(350, 43);
            this.numMaxThreads.TabIndex = 35;
            this.numMaxThreads.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // chkDemoMode
            // 
            this.chkDemoMode.AutoSize = true;
            this.chkDemoMode.Checked = true;
            this.chkDemoMode.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDemoMode.Font = new System.Drawing.Font("宋体", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.chkDemoMode.Location = new System.Drawing.Point(75, 280);
            this.chkDemoMode.Name = "chkDemoMode";
            this.chkDemoMode.Size = new System.Drawing.Size(425, 37);
            this.chkDemoMode.TabIndex = 39;
            this.chkDemoMode.Text = "演示模式(允许运行旧任务)";
            this.chkDemoMode.UseVisualStyleBackColor = true;
            // 
            // chkLocalMode
            // 
            this.chkLocalMode.AutoSize = true;
            this.chkLocalMode.Font = new System.Drawing.Font("宋体", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.chkLocalMode.Location = new System.Drawing.Point(540, 280);
            this.chkLocalMode.Name = "chkLocalMode";
            this.chkLocalMode.Size = new System.Drawing.Size(233, 37);
            this.chkLocalMode.TabIndex = 40;
            this.chkLocalMode.Text = "监控本地路径";
            this.chkLocalMode.UseVisualStyleBackColor = true;
            // 
            // FrmMainControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(851, 543);
            this.Controls.Add(this.chkLocalMode);
            this.Controls.Add(this.chkDemoMode);
            this.Controls.Add(this.numMaxThreads);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnOpenUploader);
            this.Controls.Add(this.btnOpenDownloader);
            this.Controls.Add(this.cboPendingTasks);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnStartMonitor);
            this.Controls.Add(this.txtInfoPath);
            this.Controls.Add(this.label1);
            this.Name = "FrmMainControl";
            this.Text = "多任务架次控制";
            ((System.ComponentModel.ISupportInitialize)(this.numMaxThreads)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtInfoPath;
        private System.Windows.Forms.Button btnStartMonitor;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cboPendingTasks;
        private System.Windows.Forms.Button btnOpenDownloader;
        private System.Windows.Forms.Button btnOpenUploader;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numMaxThreads;
        private System.Windows.Forms.CheckBox chkDemoMode;
        private System.Windows.Forms.CheckBox chkLocalMode;
    }
}
