namespace SoundFingerprinting.SoundTools.BassResampler
{
    partial class WinBassResampler
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WinBassResampler));
            this._tbPathToFile = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this._btnResample = new System.Windows.Forms.Button();
            this._nudSampleRate = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this._nudSampleRate)).BeginInit();
            this.SuspendLayout();
            // 
            // _tbPathToFile
            // 
            this._tbPathToFile.Location = new System.Drawing.Point(12, 25);
            this._tbPathToFile.Name = "_tbPathToFile";
            this._tbPathToFile.Size = new System.Drawing.Size(288, 20);
            this._tbPathToFile.TabIndex = 0;
            this._tbPathToFile.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.TbPathToFileMouseDoubleClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(119, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Select a file to resample";
            // 
            // _btnResample
            // 
            this._btnResample.Location = new System.Drawing.Point(225, 48);
            this._btnResample.Name = "_btnResample";
            this._btnResample.Size = new System.Drawing.Size(75, 23);
            this._btnResample.TabIndex = 2;
            this._btnResample.Text = "Resample";
            this._btnResample.UseVisualStyleBackColor = true;
            this._btnResample.Click += new System.EventHandler(this.BtnResampleClick);
            // 
            // _nudSampleRate
            // 
            this._nudSampleRate.Location = new System.Drawing.Point(12, 51);
            this._nudSampleRate.Maximum = new decimal(new int[] {
            44000,
            0,
            0,
            0});
            this._nudSampleRate.Name = "_nudSampleRate";
            this._nudSampleRate.Size = new System.Drawing.Size(120, 20);
            this._nudSampleRate.TabIndex = 3;
            this._nudSampleRate.Value = new decimal(new int[] {
            5512,
            0,
            0,
            0});
            // 
            // WinBassResampler
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(309, 82);
            this.Controls.Add(this._nudSampleRate);
            this.Controls.Add(this._btnResample);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._tbPathToFile);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(325, 120);
            this.MinimumSize = new System.Drawing.Size(325, 120);
            this.Name = "WinBassResampler";
            this.Text = "Bass resampler";
            ((System.ComponentModel.ISupportInitialize)(this._nudSampleRate)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _tbPathToFile;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button _btnResample;
        private System.Windows.Forms.NumericUpDown _nudSampleRate;
    }
}