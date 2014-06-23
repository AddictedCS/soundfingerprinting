namespace SoundFingerprinting.SoundTools
{
    partial class WinMain
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
            this._msMain = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fillDatabaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fillDatabaseToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.trainToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hashFingerprintsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.queryDatabaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.minHashPermGeneratorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.audioToolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.randomPermutationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fFMpegResamplerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bassResamplerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.similarityCalculationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.waveletDecompositionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._gbMain = new System.Windows.Forms.GroupBox();
            this.btnInsertSpectralImages = new System.Windows.Forms.Button();
            this._btnQueryDb = new System.Windows.Forms.Button();
            this._btnFillDatabase = new System.Windows.Forms.Button();
            this.btnTrainNeuralNetwork = new System.Windows.Forms.Button();
            this._msMain.SuspendLayout();
            this._gbMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // _msMain
            // 
            this._msMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.fillDatabaseToolStripMenuItem,
            this.toolsToolStripMenuItem});
            this._msMain.Location = new System.Drawing.Point(0, 0);
            this._msMain.Name = "_msMain";
            this._msMain.Size = new System.Drawing.Size(253, 24);
            this._msMain.TabIndex = 0;
            this._msMain.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.closeToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.closeToolStripMenuItem.Text = "Close";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.CloseToolStripMenuItemClick);
            // 
            // fillDatabaseToolStripMenuItem
            // 
            this.fillDatabaseToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fillDatabaseToolStripMenuItem1,
            this.trainToolStripMenuItem,
            this.hashFingerprintsToolStripMenuItem,
            this.queryDatabaseToolStripMenuItem});
            this.fillDatabaseToolStripMenuItem.Name = "fillDatabaseToolStripMenuItem";
            this.fillDatabaseToolStripMenuItem.Size = new System.Drawing.Size(46, 20);
            this.fillDatabaseToolStripMenuItem.Text = "Tasks";
            // 
            // fillDatabaseToolStripMenuItem1
            // 
            this.fillDatabaseToolStripMenuItem1.Name = "fillDatabaseToolStripMenuItem1";
            this.fillDatabaseToolStripMenuItem1.Size = new System.Drawing.Size(152, 22);
            this.fillDatabaseToolStripMenuItem1.Text = "Fill Database";
            this.fillDatabaseToolStripMenuItem1.Click += new System.EventHandler(this.FillDatabaseToolStripClick);
            // 
            // trainToolStripMenuItem
            // 
            this.trainToolStripMenuItem.Name = "trainToolStripMenuItem";
            this.trainToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            // 
            // hashFingerprintsToolStripMenuItem
            // 
            this.hashFingerprintsToolStripMenuItem.Name = "hashFingerprintsToolStripMenuItem";
            this.hashFingerprintsToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            // 
            // queryDatabaseToolStripMenuItem
            // 
            this.queryDatabaseToolStripMenuItem.Name = "queryDatabaseToolStripMenuItem";
            this.queryDatabaseToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.queryDatabaseToolStripMenuItem.Text = "Query database";
            this.queryDatabaseToolStripMenuItem.Click += new System.EventHandler(this.QueryDatabaseToolStripClick);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.minHashPermGeneratorToolStripMenuItem,
            this.audioToolToolStripMenuItem,
            this.randomPermutationToolStripMenuItem,
            this.fFMpegResamplerToolStripMenuItem,
            this.bassResamplerToolStripMenuItem,
            this.similarityCalculationToolStripMenuItem,
            this.waveletDecompositionToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // minHashPermGeneratorToolStripMenuItem
            // 
            this.minHashPermGeneratorToolStripMenuItem.Name = "minHashPermGeneratorToolStripMenuItem";
            this.minHashPermGeneratorToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.minHashPermGeneratorToolStripMenuItem.Text = "Min-Hash perm generator";
            this.minHashPermGeneratorToolStripMenuItem.Click += new System.EventHandler(this.MinHashPermGeneratorToolStripClick);
            // 
            // audioToolToolStripMenuItem
            // 
            this.audioToolToolStripMenuItem.Name = "audioToolToolStripMenuItem";
            this.audioToolToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.audioToolToolStripMenuItem.Text = "Drawning Tool";
            this.audioToolToolStripMenuItem.Click += new System.EventHandler(this.AudioToolToolStripMenuItemClick);
            // 
            // randomPermutationToolStripMenuItem
            // 
            this.randomPermutationToolStripMenuItem.Name = "randomPermutationToolStripMenuItem";
            this.randomPermutationToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.randomPermutationToolStripMenuItem.Text = "Random file permutation";
            this.randomPermutationToolStripMenuItem.Click += new System.EventHandler(this.RandomPermutationToolStripMenuItemClick);
            // 
            // fFMpegResamplerToolStripMenuItem
            // 
            this.fFMpegResamplerToolStripMenuItem.Name = "fFMpegResamplerToolStripMenuItem";
            this.fFMpegResamplerToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.fFMpegResamplerToolStripMenuItem.Text = "FFMpeg Resampler";
            this.fFMpegResamplerToolStripMenuItem.Click += new System.EventHandler(this.FFMpegResamplerToolStripMenuItemClick);
            // 
            // bassResamplerToolStripMenuItem
            // 
            this.bassResamplerToolStripMenuItem.Name = "bassResamplerToolStripMenuItem";
            this.bassResamplerToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.bassResamplerToolStripMenuItem.Text = "Bass Resampler";
            this.bassResamplerToolStripMenuItem.Click += new System.EventHandler(this.BassResamplerToolStripMenuItemClick);
            // 
            // similarityCalculationToolStripMenuItem
            // 
            this.similarityCalculationToolStripMenuItem.Name = "similarityCalculationToolStripMenuItem";
            this.similarityCalculationToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.similarityCalculationToolStripMenuItem.Text = "Similarity calculation";
            this.similarityCalculationToolStripMenuItem.Click += new System.EventHandler(this.SimilarityCalculationToolStripMenuItemClick);
            // 
            // waveletDecompositionToolStripMenuItem
            // 
            this.waveletDecompositionToolStripMenuItem.Name = "waveletDecompositionToolStripMenuItem";
            this.waveletDecompositionToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.waveletDecompositionToolStripMenuItem.Text = "Wavelet decomposition";
            this.waveletDecompositionToolStripMenuItem.Click += new System.EventHandler(this.WaveletDecompositionToolStripMenuItemClick);
            // 
            // _gbMain
            // 
            this._gbMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._gbMain.Controls.Add(this.btnTrainNeuralNetwork);
            this._gbMain.Controls.Add(this.btnInsertSpectralImages);
            this._gbMain.Controls.Add(this._btnQueryDb);
            this._gbMain.Controls.Add(this._btnFillDatabase);
            this._gbMain.Location = new System.Drawing.Point(12, 27);
            this._gbMain.Name = "_gbMain";
            this._gbMain.Size = new System.Drawing.Size(229, 192);
            this._gbMain.TabIndex = 1;
            this._gbMain.TabStop = false;
            this._gbMain.Text = "Main";
            // 
            // btnInsertSpectralImages
            // 
            this.btnInsertSpectralImages.Location = new System.Drawing.Point(40, 77);
            this.btnInsertSpectralImages.Name = "btnInsertSpectralImages";
            this.btnInsertSpectralImages.Size = new System.Drawing.Size(145, 23);
            this.btnInsertSpectralImages.TabIndex = 4;
            this.btnInsertSpectralImages.Text = "Create Training Set";
            this.btnInsertSpectralImages.UseVisualStyleBackColor = true;
            this.btnInsertSpectralImages.Click += new System.EventHandler(this.BtnInsertSpectralImagesClick);
            // 
            // _btnQueryDb
            // 
            this._btnQueryDb.Location = new System.Drawing.Point(40, 48);
            this._btnQueryDb.Name = "_btnQueryDb";
            this._btnQueryDb.Size = new System.Drawing.Size(145, 23);
            this._btnQueryDb.TabIndex = 3;
            this._btnQueryDb.Text = "Query database";
            this._btnQueryDb.UseVisualStyleBackColor = true;
            this._btnQueryDb.Click += new System.EventHandler(this.BtnQueryDbClick);
            // 
            // _btnFillDatabase
            // 
            this._btnFillDatabase.Location = new System.Drawing.Point(39, 19);
            this._btnFillDatabase.Name = "_btnFillDatabase";
            this._btnFillDatabase.Size = new System.Drawing.Size(146, 23);
            this._btnFillDatabase.TabIndex = 0;
            this._btnFillDatabase.Text = "Fill Database (LSH)";
            this._btnFillDatabase.UseVisualStyleBackColor = true;
            this._btnFillDatabase.Click += new System.EventHandler(this.BtnFillDatabaseClick);
            // 
            // btnTrainNeuralNetwork
            // 
            this.btnTrainNeuralNetwork.Location = new System.Drawing.Point(40, 107);
            this.btnTrainNeuralNetwork.Name = "btnTrainNeuralNetwork";
            this.btnTrainNeuralNetwork.Size = new System.Drawing.Size(145, 23);
            this.btnTrainNeuralNetwork.TabIndex = 5;
            this.btnTrainNeuralNetwork.Text = "Train Neural Nework";
            this.btnTrainNeuralNetwork.UseVisualStyleBackColor = true;
            // 
            // WinMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(253, 231);
            this.Controls.Add(this._gbMain);
            this.Controls.Add(this._msMain);
            this.MainMenuStrip = this._msMain;
            this.Name = "WinMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Main";
            this._msMain.ResumeLayout(false);
            this._msMain.PerformLayout();
            this._gbMain.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip _msMain;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fillDatabaseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fillDatabaseToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem trainToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hashFingerprintsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem randomPermutationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem minHashPermGeneratorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fFMpegResamplerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem audioToolToolStripMenuItem;
        private System.Windows.Forms.GroupBox _gbMain;
        private System.Windows.Forms.Button _btnFillDatabase;
        private System.Windows.Forms.ToolStripMenuItem queryDatabaseToolStripMenuItem;
        private System.Windows.Forms.Button _btnQueryDb;
        private System.Windows.Forms.ToolStripMenuItem bassResamplerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem similarityCalculationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem waveletDecompositionToolStripMenuItem;
        private System.Windows.Forms.Button btnInsertSpectralImages;
        private System.Windows.Forms.Button btnTrainNeuralNetwork;
    }
}

