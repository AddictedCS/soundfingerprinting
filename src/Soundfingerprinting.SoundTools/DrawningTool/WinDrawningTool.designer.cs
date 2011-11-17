namespace Soundfingerprinting.SoundTools.DrawningTool
{
    partial class WinDrawningTool
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
            this._tbPathToFile = new System.Windows.Forms.TextBox();
            this._labPathToDrawFile = new System.Windows.Forms.Label();
            this._btnDraw = new System.Windows.Forms.Button();
            this._nudStride = new System.Windows.Forms.NumericUpDown();
            this._labStride = new System.Windows.Forms.Label();
            this._lbImageTypes = new System.Windows.Forms.ListBox();
            this._btnDrawSignal = new System.Windows.Forms.Button();
            this._btnDrawSpectrum = new System.Windows.Forms.Button();
            this._nudHeight = new System.Windows.Forms.NumericUpDown();
            this._nudWidth = new System.Windows.Forms.NumericUpDown();
            this._labHeight = new System.Windows.Forms.Label();
            this._labWidth = new System.Windows.Forms.Label();
            this._btnDrawWavelets = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this._nudStride)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudWidth)).BeginInit();
            this.SuspendLayout();
            // 
            // _tbPathToFile
            // 
            this._tbPathToFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._tbPathToFile.Location = new System.Drawing.Point(15, 25);
            this._tbPathToFile.Name = "_tbPathToFile";
            this._tbPathToFile.Size = new System.Drawing.Size(337, 20);
            this._tbPathToFile.TabIndex = 0;
            this._tbPathToFile.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.TbPathToFileMouseDoubleClick);
            // 
            // _labPathToDrawFile
            // 
            this._labPathToDrawFile.AutoSize = true;
            this._labPathToDrawFile.Location = new System.Drawing.Point(12, 9);
            this._labPathToDrawFile.Name = "_labPathToDrawFile";
            this._labPathToDrawFile.Size = new System.Drawing.Size(228, 13);
            this._labPathToDrawFile.TabIndex = 1;
            this._labPathToDrawFile.Text = "Path to the file to draw (double click to browse)";
            // 
            // _btnDraw
            // 
            this._btnDraw.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._btnDraw.Location = new System.Drawing.Point(15, 178);
            this._btnDraw.Name = "_btnDraw";
            this._btnDraw.Size = new System.Drawing.Size(123, 23);
            this._btnDraw.TabIndex = 2;
            this._btnDraw.Text = "Draw fingerprints!";
            this._btnDraw.UseVisualStyleBackColor = true;
            this._btnDraw.Click += new System.EventHandler(this.BtnDrawFingerprintsClick);
            // 
            // _nudStride
            // 
            this._nudStride.Location = new System.Drawing.Point(15, 64);
            this._nudStride.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this._nudStride.Name = "_nudStride";
            this._nudStride.Size = new System.Drawing.Size(123, 20);
            this._nudStride.TabIndex = 11;
            this._nudStride.Value = new decimal(new int[] {
            1102,
            0,
            0,
            0});
            // 
            // _labStride
            // 
            this._labStride.AutoSize = true;
            this._labStride.Location = new System.Drawing.Point(12, 48);
            this._labStride.Name = "_labStride";
            this._labStride.Size = new System.Drawing.Size(117, 13);
            this._labStride.TabIndex = 16;
            this._labStride.Text = "Select stride in samples";
            // 
            // _lbImageTypes
            // 
            this._lbImageTypes.FormattingEnabled = true;
            this._lbImageTypes.Location = new System.Drawing.Point(15, 90);
            this._lbImageTypes.Name = "_lbImageTypes";
            this._lbImageTypes.Size = new System.Drawing.Size(123, 82);
            this._lbImageTypes.TabIndex = 17;
            // 
            // _btnDrawSignal
            // 
            this._btnDrawSignal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._btnDrawSignal.Location = new System.Drawing.Point(227, 120);
            this._btnDrawSignal.Name = "_btnDrawSignal";
            this._btnDrawSignal.Size = new System.Drawing.Size(125, 23);
            this._btnDrawSignal.TabIndex = 18;
            this._btnDrawSignal.Text = "Draw signal";
            this._btnDrawSignal.UseVisualStyleBackColor = true;
            this._btnDrawSignal.Click += new System.EventHandler(this.BtnDrawSignalClick);
            // 
            // _btnDrawSpectrum
            // 
            this._btnDrawSpectrum.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._btnDrawSpectrum.Location = new System.Drawing.Point(227, 149);
            this._btnDrawSpectrum.Name = "_btnDrawSpectrum";
            this._btnDrawSpectrum.Size = new System.Drawing.Size(125, 23);
            this._btnDrawSpectrum.TabIndex = 19;
            this._btnDrawSpectrum.Text = "Draw spectrum";
            this._btnDrawSpectrum.UseVisualStyleBackColor = true;
            this._btnDrawSpectrum.Click += new System.EventHandler(this.BtnDrawSpectrumClick);
            // 
            // _nudHeight
            // 
            this._nudHeight.Location = new System.Drawing.Point(227, 64);
            this._nudHeight.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this._nudHeight.Name = "_nudHeight";
            this._nudHeight.Size = new System.Drawing.Size(123, 20);
            this._nudHeight.TabIndex = 20;
            this._nudHeight.Value = new decimal(new int[] {
            800,
            0,
            0,
            0});
            // 
            // _nudWidth
            // 
            this._nudWidth.Location = new System.Drawing.Point(227, 90);
            this._nudWidth.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this._nudWidth.Name = "_nudWidth";
            this._nudWidth.Size = new System.Drawing.Size(123, 20);
            this._nudWidth.TabIndex = 21;
            this._nudWidth.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // _labHeight
            // 
            this._labHeight.AutoSize = true;
            this._labHeight.Location = new System.Drawing.Point(183, 66);
            this._labHeight.Name = "_labHeight";
            this._labHeight.Size = new System.Drawing.Size(38, 13);
            this._labHeight.TabIndex = 22;
            this._labHeight.Text = "Height";
            // 
            // _labWidth
            // 
            this._labWidth.AutoSize = true;
            this._labWidth.Location = new System.Drawing.Point(183, 92);
            this._labWidth.Name = "_labWidth";
            this._labWidth.Size = new System.Drawing.Size(35, 13);
            this._labWidth.TabIndex = 23;
            this._labWidth.Text = "Width";
            // 
            // _btnDrawWavelets
            // 
            this._btnDrawWavelets.Location = new System.Drawing.Point(15, 207);
            this._btnDrawWavelets.Name = "_btnDrawWavelets";
            this._btnDrawWavelets.Size = new System.Drawing.Size(123, 23);
            this._btnDrawWavelets.TabIndex = 24;
            this._btnDrawWavelets.Text = "Draw wavelets!";
            this._btnDrawWavelets.UseVisualStyleBackColor = true;
            this._btnDrawWavelets.Click += new System.EventHandler(this.BtnDrawWaveletsClick);
            // 
            // WinDrawningTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(359, 245);
            this.Controls.Add(this._btnDrawWavelets);
            this.Controls.Add(this._labWidth);
            this.Controls.Add(this._labHeight);
            this.Controls.Add(this._nudWidth);
            this.Controls.Add(this._nudHeight);
            this.Controls.Add(this._btnDrawSpectrum);
            this.Controls.Add(this._btnDrawSignal);
            this.Controls.Add(this._lbImageTypes);
            this.Controls.Add(this._labStride);
            this.Controls.Add(this._nudStride);
            this.Controls.Add(this._btnDraw);
            this.Controls.Add(this._labPathToDrawFile);
            this.Controls.Add(this._tbPathToFile);
            this.MaximizeBox = false;
            this.Name = "WinDrawningTool";
            this.Text = "Draw Fingerprints";
            ((System.ComponentModel.ISupportInitialize)(this._nudStride)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudWidth)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _tbPathToFile;
        private System.Windows.Forms.Label _labPathToDrawFile;
        private System.Windows.Forms.Button _btnDraw;
        private System.Windows.Forms.NumericUpDown _nudStride;
        private System.Windows.Forms.Label _labStride;
        private System.Windows.Forms.ListBox _lbImageTypes;
        private System.Windows.Forms.Button _btnDrawSignal;
        private System.Windows.Forms.Button _btnDrawSpectrum;
        private System.Windows.Forms.NumericUpDown _nudHeight;
        private System.Windows.Forms.NumericUpDown _nudWidth;
        private System.Windows.Forms.Label _labHeight;
        private System.Windows.Forms.Label _labWidth;
        private System.Windows.Forms.Button _btnDrawWavelets;
    }
}