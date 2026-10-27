namespace RtMap
{
    partial class video_samp
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
            this.btnBrowseOutput = new System.Windows.Forms.Button();
            this.btnBrowseInput = new System.Windows.Forms.Button();
            this.txtOutputDir = new System.Windows.Forms.TextBox();
            this.txtInputDir = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnVideo_Samp = new System.Windows.Forms.Button();
            this.numParam = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numParam)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(69, 69);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(207, 33);
            this.label1.TabIndex = 16;
            this.label1.Text = "选择视频文件";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // btnBrowseOutput
            // 
            this.btnBrowseOutput.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnBrowseOutput.Location = new System.Drawing.Point(676, 130);
            this.btnBrowseOutput.Name = "btnBrowseOutput";
            this.btnBrowseOutput.Size = new System.Drawing.Size(96, 46);
            this.btnBrowseOutput.TabIndex = 20;
            this.btnBrowseOutput.Text = "选择";
            this.btnBrowseOutput.UseVisualStyleBackColor = true;
            this.btnBrowseOutput.Click += new System.EventHandler(this.btnBrowseOutput_Click);
            // 
            // btnBrowseInput
            // 
            this.btnBrowseInput.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnBrowseInput.Location = new System.Drawing.Point(676, 60);
            this.btnBrowseInput.Name = "btnBrowseInput";
            this.btnBrowseInput.Size = new System.Drawing.Size(96, 46);
            this.btnBrowseInput.TabIndex = 19;
            this.btnBrowseInput.Text = "选择";
            this.btnBrowseInput.UseVisualStyleBackColor = true;
            this.btnBrowseInput.Click += new System.EventHandler(this.btnBrowseInput_Click);
            // 
            // txtOutputDir
            // 
            this.txtOutputDir.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtOutputDir.Location = new System.Drawing.Point(298, 130);
            this.txtOutputDir.Name = "txtOutputDir";
            this.txtOutputDir.Size = new System.Drawing.Size(350, 43);
            this.txtOutputDir.TabIndex = 18;
            this.txtOutputDir.TextChanged += new System.EventHandler(this.txtOutputDir_TextChanged);
            // 
            // txtInputDir
            // 
            this.txtInputDir.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtInputDir.Location = new System.Drawing.Point(298, 60);
            this.txtInputDir.Name = "txtInputDir";
            this.txtInputDir.Size = new System.Drawing.Size(350, 43);
            this.txtInputDir.TabIndex = 17;
            this.txtInputDir.TextChanged += new System.EventHandler(this.txtInputDir_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(106, 140);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(143, 33);
            this.label2.TabIndex = 21;
            this.label2.Text = "输出目录";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(69, 215);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(207, 33);
            this.label3.TabIndex = 24;
            this.label3.Text = "抽帧时间间隔";
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // btnVideo_Samp
            // 
            this.btnVideo_Samp.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btnVideo_Samp.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnVideo_Samp.Location = new System.Drawing.Point(590, 337);
            this.btnVideo_Samp.Name = "btnVideo_Samp";
            this.btnVideo_Samp.Size = new System.Drawing.Size(153, 46);
            this.btnVideo_Samp.TabIndex = 25;
            this.btnVideo_Samp.Text = "开始抽帧";
            this.btnVideo_Samp.UseVisualStyleBackColor = false;
            this.btnVideo_Samp.Click += new System.EventHandler(this.btnVideo_Samp_Click);
            // 
            // numParam
            // 
            this.numParam.DecimalPlaces = 1;
            this.numParam.Font = new System.Drawing.Font("宋体", 15.75F);
            this.numParam.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numParam.Location = new System.Drawing.Point(298, 205);
            this.numParam.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numParam.Name = "numParam";
            this.numParam.Size = new System.Drawing.Size(350, 43);
            this.numParam.TabIndex = 26;
            this.numParam.Value = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.numParam.ValueChanged += new System.EventHandler(this.numParam_ValueChanged);
            // 
            // video_samp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(851, 450);
            this.Controls.Add(this.numParam);
            this.Controls.Add(this.btnVideo_Samp);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnBrowseOutput);
            this.Controls.Add(this.btnBrowseInput);
            this.Controls.Add(this.txtOutputDir);
            this.Controls.Add(this.txtInputDir);
            this.Controls.Add(this.label1);
            this.Name = "video_samp";
            this.Text = "视频抽帧工具";
            this.Load += new System.EventHandler(this.video_samp_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numParam)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnBrowseOutput;
        private System.Windows.Forms.Button btnBrowseInput;
        private System.Windows.Forms.TextBox txtOutputDir;
        private System.Windows.Forms.TextBox txtInputDir;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnVideo_Samp;
        private System.Windows.Forms.NumericUpDown numParam;
    }
}