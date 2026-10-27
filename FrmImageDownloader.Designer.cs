namespace RtMap
{
    partial class FrmImageDownloader
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
            this.txtInputDir = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtOutputDir = new System.Windows.Forms.TextBox();
            this.btnBrowseOutput = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnImageDownloader = new System.Windows.Forms.Button();
            this.rdoMip = new System.Windows.Forms.RadioButton();
            this.rdoSlm = new System.Windows.Forms.RadioButton();
            this.rdoSfm = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.cboCameraType = new System.Windows.Forms.ComboBox();
            this.btnLaunchSfm = new System.Windows.Forms.Button();
            this.btnLaunchSlm = new System.Windows.Forms.Button();
            this.chkLocalSource = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(69, 69);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(175, 33);
            this.label1.TabIndex = 23;
            this.label1.Text = "服务器路径";
            // 
            // txtInputDir
            // 
            this.txtInputDir.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtInputDir.Location = new System.Drawing.Point(298, 60);
            this.txtInputDir.Name = "txtInputDir";
            this.txtInputDir.Size = new System.Drawing.Size(350, 43);
            this.txtInputDir.TabIndex = 24;
            this.txtInputDir.Text = "/data/docker-shuzilvtu/szlt-wd-picture";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(88, 140);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(143, 33);
            this.label2.TabIndex = 28;
            this.label2.Text = "影像目录";
            // 
            // txtOutputDir
            // 
            this.txtOutputDir.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtOutputDir.Location = new System.Drawing.Point(298, 130);
            this.txtOutputDir.Name = "txtOutputDir";
            this.txtOutputDir.Size = new System.Drawing.Size(350, 43);
            this.txtOutputDir.TabIndex = 29;
            // 
            // btnBrowseOutput
            // 
            this.btnBrowseOutput.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnBrowseOutput.Location = new System.Drawing.Point(676, 130);
            this.btnBrowseOutput.Name = "btnBrowseOutput";
            this.btnBrowseOutput.Size = new System.Drawing.Size(96, 46);
            this.btnBrowseOutput.TabIndex = 30;
            this.btnBrowseOutput.Text = "选择";
            this.btnBrowseOutput.UseVisualStyleBackColor = true;
            this.btnBrowseOutput.Click += new System.EventHandler(this.btnBrowseOutput_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblStatus.Location = new System.Drawing.Point(69, 471);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(0, 33);
            this.lblStatus.TabIndex = 31;
            // 
            // btnImageDownloader
            // 
            this.btnImageDownloader.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btnImageDownloader.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnImageDownloader.Location = new System.Drawing.Point(69, 537);
            this.btnImageDownloader.Name = "btnImageDownloader";
            this.btnImageDownloader.Size = new System.Drawing.Size(153, 46);
            this.btnImageDownloader.TabIndex = 32;
            this.btnImageDownloader.Text = "开始下传";
            this.btnImageDownloader.UseVisualStyleBackColor = false;
            this.btnImageDownloader.Click += new System.EventHandler(this.btnImageDownloader_Click);
            // 
            // rdoMip
            // 
            this.rdoMip.AutoSize = true;
            this.rdoMip.Font = new System.Drawing.Font("宋体", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.rdoMip.Location = new System.Drawing.Point(24, 45);
            this.rdoMip.Name = "rdoMip";
            this.rdoMip.Size = new System.Drawing.Size(184, 37);
            this.rdoMip.TabIndex = 33;
            this.rdoMip.Text = "RtMap_Mip";
            this.rdoMip.UseVisualStyleBackColor = true;
            // 
            // rdoSlm
            // 
            this.rdoSlm.AutoSize = true;
            this.rdoSlm.Font = new System.Drawing.Font("宋体", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.rdoSlm.Location = new System.Drawing.Point(264, 45);
            this.rdoSlm.Name = "rdoSlm";
            this.rdoSlm.Size = new System.Drawing.Size(184, 37);
            this.rdoSlm.TabIndex = 34;
            this.rdoSlm.Text = "RtMap_Slm";
            this.rdoSlm.UseVisualStyleBackColor = true;
            // 
            // rdoSfm
            // 
            this.rdoSfm.AutoSize = true;
            this.rdoSfm.Checked = true;
            this.rdoSfm.Font = new System.Drawing.Font("宋体", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.rdoSfm.Location = new System.Drawing.Point(505, 45);
            this.rdoSfm.Name = "rdoSfm";
            this.rdoSfm.Size = new System.Drawing.Size(184, 37);
            this.rdoSfm.TabIndex = 35;
            this.rdoSfm.TabStop = true;
            this.rdoSfm.Text = "RtMap_Sfm";
            this.rdoSfm.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rdoSfm);
            this.groupBox1.Controls.Add(this.rdoSlm);
            this.groupBox1.Controls.Add(this.rdoMip);
            this.groupBox1.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox1.Location = new System.Drawing.Point(75, 196);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(697, 120);
            this.groupBox1.TabIndex = 36;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "选择算法";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.cboCameraType);
            this.groupBox2.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox2.Location = new System.Drawing.Point(75, 322);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(396, 120);
            this.groupBox2.TabIndex = 37;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "选择相机";
            // 
            // cboCameraType
            // 
            this.cboCameraType.Font = new System.Drawing.Font("宋体", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cboCameraType.FormattingEnabled = true;
            this.cboCameraType.Location = new System.Drawing.Point(24, 50);
            this.cboCameraType.Name = "cboCameraType";
            this.cboCameraType.Size = new System.Drawing.Size(350, 41);
            this.cboCameraType.TabIndex = 31;
            this.cboCameraType.Text = "S6100_X_40";
            // 
            // btnLaunchSfm
            // 
            this.btnLaunchSfm.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btnLaunchSfm.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnLaunchSfm.Location = new System.Drawing.Point(532, 537);
            this.btnLaunchSfm.Name = "btnLaunchSfm";
            this.btnLaunchSfm.Size = new System.Drawing.Size(240, 46);
            this.btnLaunchSfm.TabIndex = 38;
            this.btnLaunchSfm.Text = "启动RtMap_Sfm";
            this.btnLaunchSfm.UseVisualStyleBackColor = false;
            this.btnLaunchSfm.Click += new System.EventHandler(this.btnLaunchSfm_Click);
            // 
            // btnLaunchSlm
            // 
            this.btnLaunchSlm.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btnLaunchSlm.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnLaunchSlm.Location = new System.Drawing.Point(257, 537);
            this.btnLaunchSlm.Name = "btnLaunchSlm";
            this.btnLaunchSlm.Size = new System.Drawing.Size(240, 46);
            this.btnLaunchSlm.TabIndex = 39;
            this.btnLaunchSlm.Text = "启动RtMap_Slm";
            this.btnLaunchSlm.UseVisualStyleBackColor = false;
            this.btnLaunchSlm.Click += new System.EventHandler(this.btnLaunchSlm_Click);
            // 
            // chkLocalSource
            // 
            this.chkLocalSource.AutoSize = true;
            this.chkLocalSource.Font = new System.Drawing.Font("宋体", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.chkLocalSource.Location = new System.Drawing.Point(539, 374);
            this.chkLocalSource.Name = "chkLocalSource";
            this.chkLocalSource.Size = new System.Drawing.Size(233, 37);
            this.chkLocalSource.TabIndex = 41;
            this.chkLocalSource.Text = "监控本地路径";
            this.chkLocalSource.UseVisualStyleBackColor = true;
            // 
            // FrmImageDownloader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(851, 653);
            this.Controls.Add(this.chkLocalSource);
            this.Controls.Add(this.btnLaunchSlm);
            this.Controls.Add(this.btnLaunchSfm);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnImageDownloader);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnBrowseOutput);
            this.Controls.Add(this.txtOutputDir);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtInputDir);
            this.Controls.Add(this.label1);
            this.Name = "FrmImageDownloader";
            this.Text = "影像下传";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtInputDir;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtOutputDir;
        private System.Windows.Forms.Button btnBrowseOutput;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnImageDownloader;
        private System.Windows.Forms.RadioButton rdoMip;
        private System.Windows.Forms.RadioButton rdoSlm;
        private System.Windows.Forms.RadioButton rdoSfm;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ComboBox cboCameraType;
        private System.Windows.Forms.Button btnLaunchSfm;
        private System.Windows.Forms.Button btnLaunchSlm;
        private System.Windows.Forms.CheckBox chkLocalSource;
    }
}