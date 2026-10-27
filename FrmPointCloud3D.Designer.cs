namespace RtMap
{
    partial class FrmPointCloud3D
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
            this.chkLocalMode = new System.Windows.Forms.CheckBox();
            this.chkDemoMode = new System.Windows.Forms.CheckBox();
            this.numMaxThreads = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.cboPendingTasks = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnStartMonitor = new System.Windows.Forms.Button();
            this.txtInfoPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxThreads)).BeginInit();
            this.SuspendLayout();
            // 
            // chkLocalMode
            // 
            this.chkLocalMode.AutoSize = true;
            this.chkLocalMode.Font = new System.Drawing.Font("宋体", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.chkLocalMode.Location = new System.Drawing.Point(540, 280);
            this.chkLocalMode.Name = "chkLocalMode";
            this.chkLocalMode.Size = new System.Drawing.Size(233, 37);
            this.chkLocalMode.TabIndex = 49;
            this.chkLocalMode.Text = "监控本地路径";
            this.chkLocalMode.UseVisualStyleBackColor = true;
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
            this.chkDemoMode.TabIndex = 48;
            this.chkDemoMode.Text = "演示模式(允许运行旧任务)";
            this.chkDemoMode.UseVisualStyleBackColor = true;
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
            this.numMaxThreads.TabIndex = 47;
            this.numMaxThreads.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(69, 210);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(143, 33);
            this.label3.TabIndex = 46;
            this.label3.Text = "最大线程";
            // 
            // cboPendingTasks
            // 
            this.cboPendingTasks.Font = new System.Drawing.Font("宋体", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cboPendingTasks.FormattingEnabled = true;
            this.cboPendingTasks.Location = new System.Drawing.Point(235, 135);
            this.cboPendingTasks.Name = "cboPendingTasks";
            this.cboPendingTasks.Size = new System.Drawing.Size(350, 41);
            this.cboPendingTasks.TabIndex = 45;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(82, 140);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(111, 33);
            this.label2.TabIndex = 44;
            this.label2.Text = "任务ID";
            // 
            // btnStartMonitor
            // 
            this.btnStartMonitor.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnStartMonitor.Location = new System.Drawing.Point(620, 60);
            this.btnStartMonitor.Name = "btnStartMonitor";
            this.btnStartMonitor.Size = new System.Drawing.Size(153, 46);
            this.btnStartMonitor.TabIndex = 43;
            this.btnStartMonitor.Text = "开始监控";
            this.btnStartMonitor.UseVisualStyleBackColor = true;
            this.btnStartMonitor.Click += new System.EventHandler(this.btnStartMonitor_Click);
            // 
            // txtInfoPath
            // 
            this.txtInfoPath.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtInfoPath.Location = new System.Drawing.Point(235, 60);
            this.txtInfoPath.Name = "txtInfoPath";
            this.txtInfoPath.Size = new System.Drawing.Size(350, 43);
            this.txtInfoPath.TabIndex = 42;
            this.txtInfoPath.Text = "/data/docker-shuzilvtu/szlt-wd-picture/start/info";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(69, 69);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(143, 33);
            this.label1.TabIndex = 41;
            this.label1.Text = "通知目录";
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblStatus.Location = new System.Drawing.Point(69, 370);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(0, 33);
            this.lblStatus.TabIndex = 50;
            // 
            // FrmPointCloud3D
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(851, 443);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.chkLocalMode);
            this.Controls.Add(this.chkDemoMode);
            this.Controls.Add(this.numMaxThreads);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cboPendingTasks);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnStartMonitor);
            this.Controls.Add(this.txtInfoPath);
            this.Controls.Add(this.label1);
            this.Name = "FrmPointCloud3D";
            this.Text = "三维成图任务控制";
            this.Load += new System.EventHandler(this.FrmPointCloud3D_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numMaxThreads)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkLocalMode;
        private System.Windows.Forms.CheckBox chkDemoMode;
        private System.Windows.Forms.NumericUpDown numMaxThreads;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cboPendingTasks;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnStartMonitor;
        private System.Windows.Forms.TextBox txtInfoPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblStatus;
    }
}