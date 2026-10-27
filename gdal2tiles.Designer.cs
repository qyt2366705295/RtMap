namespace RtMap
{
    partial class gdal2tiles
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
            this.label2 = new System.Windows.Forms.Label();
            this.btnBrowseOutput = new System.Windows.Forms.Button();
            this.btnBrowseInput = new System.Windows.Forms.Button();
            this.txtOutputDir = new System.Windows.Forms.TextBox();
            this.txtInputDir = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnGdal2tiles = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.cboTasks = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtRemoteBaseDir = new System.Windows.Forms.TextBox();
            this.btnRefreshTasks = new System.Windows.Forms.Button();
            this.btnFinishTask = new System.Windows.Forms.Button();
            this.btnDirectUpload = new System.Windows.Forms.Button();
            this.btnDirectUploadFile = new System.Windows.Forms.Button();
            this.chkAutoClean = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(97, 279);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(143, 33);
            this.label2.TabIndex = 27;
            this.label2.Text = "输出目录";
            // 
            // btnBrowseOutput
            // 
            this.btnBrowseOutput.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnBrowseOutput.Location = new System.Drawing.Point(676, 270);
            this.btnBrowseOutput.Name = "btnBrowseOutput";
            this.btnBrowseOutput.Size = new System.Drawing.Size(96, 46);
            this.btnBrowseOutput.TabIndex = 26;
            this.btnBrowseOutput.Text = "选择";
            this.btnBrowseOutput.UseVisualStyleBackColor = true;
            this.btnBrowseOutput.Click += new System.EventHandler(this.btnBrowseOutput_Click);
            // 
            // btnBrowseInput
            // 
            this.btnBrowseInput.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnBrowseInput.Location = new System.Drawing.Point(676, 200);
            this.btnBrowseInput.Name = "btnBrowseInput";
            this.btnBrowseInput.Size = new System.Drawing.Size(96, 46);
            this.btnBrowseInput.TabIndex = 25;
            this.btnBrowseInput.Text = "选择";
            this.btnBrowseInput.UseVisualStyleBackColor = true;
            this.btnBrowseInput.Click += new System.EventHandler(this.btnBrowseInput_Click);
            // 
            // txtOutputDir
            // 
            this.txtOutputDir.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtOutputDir.Location = new System.Drawing.Point(298, 271);
            this.txtOutputDir.Name = "txtOutputDir";
            this.txtOutputDir.Size = new System.Drawing.Size(350, 43);
            this.txtOutputDir.TabIndex = 24;
            // 
            // txtInputDir
            // 
            this.txtInputDir.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtInputDir.Location = new System.Drawing.Point(298, 201);
            this.txtInputDir.Name = "txtInputDir";
            this.txtInputDir.Size = new System.Drawing.Size(350, 43);
            this.txtInputDir.TabIndex = 23;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(69, 209);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(191, 33);
            this.label1.TabIndex = 22;
            this.label1.Text = "选择Tif目录";
            // 
            // btnGdal2tiles
            // 
            this.btnGdal2tiles.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btnGdal2tiles.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnGdal2tiles.Location = new System.Drawing.Point(69, 477);
            this.btnGdal2tiles.Name = "btnGdal2tiles";
            this.btnGdal2tiles.Size = new System.Drawing.Size(153, 46);
            this.btnGdal2tiles.TabIndex = 28;
            this.btnGdal2tiles.Text = "开始切片上传";
            this.btnGdal2tiles.UseVisualStyleBackColor = false;
            this.btnGdal2tiles.Click += new System.EventHandler(this.btnGdal2tiles_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblStatus.Location = new System.Drawing.Point(69, 410);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(0, 33);
            this.lblStatus.TabIndex = 29;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(69, 69);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(175, 33);
            this.label3.TabIndex = 30;
            this.label3.Text = "服务器路径";
            // 
            // cboTasks
            // 
            this.cboTasks.Font = new System.Drawing.Font("宋体", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cboTasks.FormattingEnabled = true;
            this.cboTasks.Location = new System.Drawing.Point(298, 131);
            this.cboTasks.Name = "cboTasks";
            this.cboTasks.Size = new System.Drawing.Size(350, 41);
            this.cboTasks.TabIndex = 31;
            this.cboTasks.SelectedIndexChanged += new System.EventHandler(this.cboTasks_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.Location = new System.Drawing.Point(101, 139);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(111, 33);
            this.label4.TabIndex = 32;
            this.label4.Text = "任务ID";
            // 
            // txtRemoteBaseDir
            // 
            this.txtRemoteBaseDir.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtRemoteBaseDir.Location = new System.Drawing.Point(298, 61);
            this.txtRemoteBaseDir.Name = "txtRemoteBaseDir";
            this.txtRemoteBaseDir.Size = new System.Drawing.Size(350, 43);
            this.txtRemoteBaseDir.TabIndex = 33;
            // 
            // btnRefreshTasks
            // 
            this.btnRefreshTasks.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnRefreshTasks.Location = new System.Drawing.Point(676, 128);
            this.btnRefreshTasks.Name = "btnRefreshTasks";
            this.btnRefreshTasks.Size = new System.Drawing.Size(96, 46);
            this.btnRefreshTasks.TabIndex = 34;
            this.btnRefreshTasks.Text = "刷新";
            this.btnRefreshTasks.UseVisualStyleBackColor = true;
            this.btnRefreshTasks.Click += new System.EventHandler(this.btnRefreshTasks_Click);
            // 
            // btnFinishTask
            // 
            this.btnFinishTask.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btnFinishTask.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnFinishTask.Location = new System.Drawing.Point(583, 567);
            this.btnFinishTask.Name = "btnFinishTask";
            this.btnFinishTask.Size = new System.Drawing.Size(189, 46);
            this.btnFinishTask.TabIndex = 35;
            this.btnFinishTask.Text = "完成该任务";
            this.btnFinishTask.UseVisualStyleBackColor = false;
            this.btnFinishTask.Click += new System.EventHandler(this.btnFinishTask_Click);
            // 
            // btnDirectUpload
            // 
            this.btnDirectUpload.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btnDirectUpload.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnDirectUpload.Location = new System.Drawing.Point(267, 477);
            this.btnDirectUpload.Name = "btnDirectUpload";
            this.btnDirectUpload.Size = new System.Drawing.Size(230, 46);
            this.btnDirectUpload.TabIndex = 36;
            this.btnDirectUpload.Text = "直接上传成果";
            this.btnDirectUpload.UseVisualStyleBackColor = false;
            this.btnDirectUpload.Click += new System.EventHandler(this.btnDirectUpload_Click);
            // 
            // btnDirectUploadFile
            // 
            this.btnDirectUploadFile.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btnDirectUploadFile.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnDirectUploadFile.Location = new System.Drawing.Point(542, 477);
            this.btnDirectUploadFile.Name = "btnDirectUploadFile";
            this.btnDirectUploadFile.Size = new System.Drawing.Size(230, 46);
            this.btnDirectUploadFile.TabIndex = 37;
            this.btnDirectUploadFile.Text = "上传单个文件";
            this.btnDirectUploadFile.UseVisualStyleBackColor = false;
            this.btnDirectUploadFile.Click += new System.EventHandler(this.btnDirectUploadFile_Click);
            // 
            // chkAutoClean
            // 
            this.chkAutoClean.AutoSize = true;
            this.chkAutoClean.Font = new System.Drawing.Font("宋体", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.chkAutoClean.Location = new System.Drawing.Point(69, 349);
            this.chkAutoClean.Name = "chkAutoClean";
            this.chkAutoClean.Size = new System.Drawing.Size(457, 37);
            this.chkAutoClean.TabIndex = 38;
            this.chkAutoClean.Text = "上传完成后清理本地切片数据";
            this.chkAutoClean.UseVisualStyleBackColor = true;
            // 
            // gdal2tiles
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(851, 663);
            this.Controls.Add(this.chkAutoClean);
            this.Controls.Add(this.btnDirectUploadFile);
            this.Controls.Add(this.btnDirectUpload);
            this.Controls.Add(this.btnFinishTask);
            this.Controls.Add(this.btnRefreshTasks);
            this.Controls.Add(this.txtRemoteBaseDir);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cboTasks);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnGdal2tiles);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnBrowseOutput);
            this.Controls.Add(this.btnBrowseInput);
            this.Controls.Add(this.txtOutputDir);
            this.Controls.Add(this.txtInputDir);
            this.Controls.Add(this.label1);
            this.Name = "gdal2tiles";
            this.Text = "Tif影像切片与上传";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnBrowseOutput;
        private System.Windows.Forms.Button btnBrowseInput;
        private System.Windows.Forms.TextBox txtOutputDir;
        private System.Windows.Forms.TextBox txtInputDir;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnGdal2tiles;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cboTasks;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtRemoteBaseDir;
        private System.Windows.Forms.Button btnRefreshTasks;
        private System.Windows.Forms.Button btnFinishTask;
        private System.Windows.Forms.Button btnDirectUpload;
        private System.Windows.Forms.Button btnDirectUploadFile;
        private System.Windows.Forms.CheckBox chkAutoClean;
    }
}