namespace RtMap
{
    partial class RtMap
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.txtInputDir = new System.Windows.Forms.TextBox();
            this.txtOutputDir = new System.Windows.Forms.TextBox();
            this.btnBrowseInput = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.btnBrowseOutput = new System.Windows.Forms.Button();
            this.btnRtMap_Mip = new System.Windows.Forms.Button();
            this.btnRtMap_Slm = new System.Windows.Forms.Button();
            this.btnRtMap_Sfm = new System.Windows.Forms.Button();
            this.btnRtSen_Las = new System.Windows.Forms.Button();
            this.btnRtViewer = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnRtSen_Mip = new System.Windows.Forms.Button();
            this.btnVideo_Samp = new System.Windows.Forms.Button();
            this.btnGdal2tiles = new System.Windows.Forms.Button();
            this.btnImageDownloader = new System.Windows.Forms.Button();
            this.btnMainControl = new System.Windows.Forms.Button();
            this.btn3DControl = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtInputDir
            // 
            this.txtInputDir.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtInputDir.Location = new System.Drawing.Point(297, 60);
            this.txtInputDir.Name = "txtInputDir";
            this.txtInputDir.Size = new System.Drawing.Size(350, 43);
            this.txtInputDir.TabIndex = 0;
            this.txtInputDir.TextChanged += new System.EventHandler(this.txtInputDir_TextChanged);
            // 
            // txtOutputDir
            // 
            this.txtOutputDir.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtOutputDir.Location = new System.Drawing.Point(297, 130);
            this.txtOutputDir.Name = "txtOutputDir";
            this.txtOutputDir.Size = new System.Drawing.Size(350, 43);
            this.txtOutputDir.TabIndex = 1;
            this.txtOutputDir.TextChanged += new System.EventHandler(this.txtOutputDir_TextChanged);
            // 
            // btnBrowseInput
            // 
            this.btnBrowseInput.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnBrowseInput.Location = new System.Drawing.Point(675, 60);
            this.btnBrowseInput.Name = "btnBrowseInput";
            this.btnBrowseInput.Size = new System.Drawing.Size(96, 46);
            this.btnBrowseInput.TabIndex = 2;
            this.btnBrowseInput.Text = "选择";
            this.btnBrowseInput.UseVisualStyleBackColor = true;
            this.btnBrowseInput.Click += new System.EventHandler(this.btnBrowseInput_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(111, 244);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(0, 0);
            this.button2.TabIndex = 3;
            this.button2.Text = "button2";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // btnBrowseOutput
            // 
            this.btnBrowseOutput.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnBrowseOutput.Location = new System.Drawing.Point(675, 130);
            this.btnBrowseOutput.Name = "btnBrowseOutput";
            this.btnBrowseOutput.Size = new System.Drawing.Size(96, 46);
            this.btnBrowseOutput.TabIndex = 4;
            this.btnBrowseOutput.Text = "选择";
            this.btnBrowseOutput.UseVisualStyleBackColor = true;
            this.btnBrowseOutput.Click += new System.EventHandler(this.btnBrowseOutput_Click);
            // 
            // btnRtMap_Mip
            // 
            this.btnRtMap_Mip.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnRtMap_Mip.Location = new System.Drawing.Point(75, 244);
            this.btnRtMap_Mip.Name = "btnRtMap_Mip";
            this.btnRtMap_Mip.Size = new System.Drawing.Size(214, 46);
            this.btnRtMap_Mip.TabIndex = 5;
            this.btnRtMap_Mip.Text = "RtMap_Mip";
            this.btnRtMap_Mip.UseVisualStyleBackColor = true;
            this.btnRtMap_Mip.Click += new System.EventHandler(this.btnRtMap_Mip_Click);
            // 
            // btnRtMap_Slm
            // 
            this.btnRtMap_Slm.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnRtMap_Slm.Location = new System.Drawing.Point(315, 244);
            this.btnRtMap_Slm.Name = "btnRtMap_Slm";
            this.btnRtMap_Slm.Size = new System.Drawing.Size(214, 46);
            this.btnRtMap_Slm.TabIndex = 11;
            this.btnRtMap_Slm.Text = "RtMap_Slm";
            this.btnRtMap_Slm.UseVisualStyleBackColor = true;
            this.btnRtMap_Slm.Click += new System.EventHandler(this.btnRtMap_Slm_Click);
            // 
            // btnRtMap_Sfm
            // 
            this.btnRtMap_Sfm.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnRtMap_Sfm.Location = new System.Drawing.Point(556, 244);
            this.btnRtMap_Sfm.Name = "btnRtMap_Sfm";
            this.btnRtMap_Sfm.Size = new System.Drawing.Size(214, 46);
            this.btnRtMap_Sfm.TabIndex = 12;
            this.btnRtMap_Sfm.Text = "RtMap_Sfm";
            this.btnRtMap_Sfm.UseVisualStyleBackColor = true;
            this.btnRtMap_Sfm.Click += new System.EventHandler(this.btnRtMap_Sfm_Click);
            // 
            // btnRtSen_Las
            // 
            this.btnRtSen_Las.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnRtSen_Las.Location = new System.Drawing.Point(75, 338);
            this.btnRtSen_Las.Name = "btnRtSen_Las";
            this.btnRtSen_Las.Size = new System.Drawing.Size(214, 46);
            this.btnRtSen_Las.TabIndex = 13;
            this.btnRtSen_Las.Text = "RtSen_Las";
            this.btnRtSen_Las.UseVisualStyleBackColor = true;
            this.btnRtSen_Las.Click += new System.EventHandler(this.btnRtSen_Las_Click);
            // 
            // btnRtViewer
            // 
            this.btnRtViewer.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnRtViewer.Location = new System.Drawing.Point(556, 338);
            this.btnRtViewer.Name = "btnRtViewer";
            this.btnRtViewer.Size = new System.Drawing.Size(214, 46);
            this.btnRtViewer.TabIndex = 14;
            this.btnRtViewer.Text = "RtViewer";
            this.btnRtViewer.UseVisualStyleBackColor = true;
            this.btnRtViewer.Click += new System.EventHandler(this.btnRtViewer_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(69, 69);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(223, 33);
            this.label1.TabIndex = 15;
            this.label1.Text = "影像/点云目录";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(106, 140);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(143, 33);
            this.label2.TabIndex = 16;
            this.label2.Text = "输出目录";
            // 
            // btnClose
            // 
            this.btnClose.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btnClose.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnClose.Location = new System.Drawing.Point(648, 545);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(122, 46);
            this.btnClose.TabIndex = 17;
            this.btnClose.Text = "关闭";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnRtSen_Mip
            // 
            this.btnRtSen_Mip.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnRtSen_Mip.Location = new System.Drawing.Point(315, 338);
            this.btnRtSen_Mip.Name = "btnRtSen_Mip";
            this.btnRtSen_Mip.Size = new System.Drawing.Size(214, 46);
            this.btnRtSen_Mip.TabIndex = 18;
            this.btnRtSen_Mip.Text = "RtSen_Mip";
            this.btnRtSen_Mip.UseVisualStyleBackColor = true;
            this.btnRtSen_Mip.Click += new System.EventHandler(this.btnRtSen_Mip_Click);
            // 
            // btnVideo_Samp
            // 
            this.btnVideo_Samp.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btnVideo_Samp.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnVideo_Samp.Location = new System.Drawing.Point(554, 431);
            this.btnVideo_Samp.Name = "btnVideo_Samp";
            this.btnVideo_Samp.Size = new System.Drawing.Size(217, 46);
            this.btnVideo_Samp.TabIndex = 26;
            this.btnVideo_Samp.Text = "视频抽帧工具";
            this.btnVideo_Samp.UseVisualStyleBackColor = false;
            this.btnVideo_Samp.Click += new System.EventHandler(this.btnVideo_Samp_Click);
            // 
            // btnGdal2tiles
            // 
            this.btnGdal2tiles.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btnGdal2tiles.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnGdal2tiles.Location = new System.Drawing.Point(315, 431);
            this.btnGdal2tiles.Name = "btnGdal2tiles";
            this.btnGdal2tiles.Size = new System.Drawing.Size(214, 46);
            this.btnGdal2tiles.TabIndex = 27;
            this.btnGdal2tiles.Text = "正射Tif切片";
            this.btnGdal2tiles.UseVisualStyleBackColor = false;
            this.btnGdal2tiles.Click += new System.EventHandler(this.btnGdal2tiles_Click);
            // 
            // btnImageDownloader
            // 
            this.btnImageDownloader.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btnImageDownloader.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnImageDownloader.Location = new System.Drawing.Point(75, 431);
            this.btnImageDownloader.Name = "btnImageDownloader";
            this.btnImageDownloader.Size = new System.Drawing.Size(214, 46);
            this.btnImageDownloader.TabIndex = 28;
            this.btnImageDownloader.Text = "影像下传";
            this.btnImageDownloader.UseVisualStyleBackColor = false;
            this.btnImageDownloader.Click += new System.EventHandler(this.btnImageDownloader_Click);
            // 
            // btnMainControl
            // 
            this.btnMainControl.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btnMainControl.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnMainControl.Location = new System.Drawing.Point(75, 545);
            this.btnMainControl.Name = "btnMainControl";
            this.btnMainControl.Size = new System.Drawing.Size(251, 46);
            this.btnMainControl.TabIndex = 29;
            this.btnMainControl.Text = "多架次任务控制";
            this.btnMainControl.UseVisualStyleBackColor = false;
            this.btnMainControl.Click += new System.EventHandler(this.btnMainControl_Click);
            // 
            // btn3DControl
            // 
            this.btn3DControl.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btn3DControl.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn3DControl.Location = new System.Drawing.Point(346, 545);
            this.btn3DControl.Name = "btn3DControl";
            this.btn3DControl.Size = new System.Drawing.Size(279, 46);
            this.btn3DControl.TabIndex = 30;
            this.btn3DControl.Text = "三维成图任务控制";
            this.btn3DControl.UseVisualStyleBackColor = false;
            this.btn3DControl.Click += new System.EventHandler(this.btn3DControl_Click);
            // 
            // RtMap
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(848, 644);
            this.Controls.Add(this.btn3DControl);
            this.Controls.Add(this.btnMainControl);
            this.Controls.Add(this.btnImageDownloader);
            this.Controls.Add(this.btnGdal2tiles);
            this.Controls.Add(this.btnVideo_Samp);
            this.Controls.Add(this.btnRtSen_Mip);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnRtViewer);
            this.Controls.Add(this.btnRtSen_Las);
            this.Controls.Add(this.btnRtMap_Sfm);
            this.Controls.Add(this.btnRtMap_Slm);
            this.Controls.Add(this.btnRtMap_Mip);
            this.Controls.Add(this.btnBrowseOutput);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.btnBrowseInput);
            this.Controls.Add(this.txtOutputDir);
            this.Controls.Add(this.txtInputDir);
            this.Name = "RtMap";
            this.Text = "RtMap";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RtMap_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtInputDir;
        private System.Windows.Forms.TextBox txtOutputDir;
        private System.Windows.Forms.Button btnBrowseInput;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button btnBrowseOutput;
        private System.Windows.Forms.Button btnRtMap_Mip;
        private System.Windows.Forms.Button btnRtMap_Slm;
        private System.Windows.Forms.Button btnRtMap_Sfm;
        private System.Windows.Forms.Button btnRtSen_Las;
        private System.Windows.Forms.Button btnRtViewer;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnRtSen_Mip;
        private System.Windows.Forms.Button btnVideo_Samp;
        private System.Windows.Forms.Button btnGdal2tiles;
        private System.Windows.Forms.Button btnImageDownloader;
        private System.Windows.Forms.Button btnMainControl;
        private System.Windows.Forms.Button btn3DControl;
    }
}

