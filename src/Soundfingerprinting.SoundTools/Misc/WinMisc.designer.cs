// Sound Fingerprinting framework
// https://code.google.com/p/soundfingerprinting/
// Code license: GNU General Public License v2
// ciumac.sergiu@gmail.com

namespace Soundfingerprinting.SoundTools.Misc
{
    partial class WinMisc
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
            this._labSim = new System.Windows.Forms.Label();
            this._tbOutputPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this._nudFreq = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this._nudTopWavelets = new System.Windows.Forms.NumericUpDown();
            this._btnDumpInfo = new System.Windows.Forms.Button();
            this._nudStride = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this._chbStride = new System.Windows.Forms.CheckBox();
            this._chbCompare = new System.Windows.Forms.CheckBox();
            this._tbSongToCompare = new System.Windows.Forms.TextBox();
            this._nudTables = new System.Windows.Forms.NumericUpDown();
            this._nudKeys = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this._nudQueryStride = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this._chbQueryStride = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this._nudNumberOfSubsequent = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this._nudFreq)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudTopWavelets)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudStride)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudTables)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudKeys)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudQueryStride)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudNumberOfSubsequent)).BeginInit();
            this.SuspendLayout();
            // 
            // _tbPathToFile
            // 
            this._tbPathToFile.Location = new System.Drawing.Point(12, 31);
            this._tbPathToFile.Name = "_tbPathToFile";
            this._tbPathToFile.Size = new System.Drawing.Size(340, 20);
            this._tbPathToFile.TabIndex = 1;
            this._tbPathToFile.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.TbPathToFileMouseDoubleClick);
            // 
            // _labSim
            // 
            this._labSim.AutoSize = true;
            this._labSim.Location = new System.Drawing.Point(13, 15);
            this._labSim.Name = "_labSim";
            this._labSim.Size = new System.Drawing.Size(108, 13);
            this._labSim.TabIndex = 2;
            this._labSim.Text = "Path to file to analyze";
            // 
            // _tbOutputPath
            // 
            this._tbOutputPath.Location = new System.Drawing.Point(12, 74);
            this._tbOutputPath.Name = "_tbOutputPath";
            this._tbOutputPath.Size = new System.Drawing.Size(340, 20);
            this._tbOutputPath.TabIndex = 3;
            this._tbOutputPath.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.TbOutputPathMouseDoubleClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 58);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Path to output";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 101);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Min Freq";
            // 
            // _nudFreq
            // 
            this._nudFreq.Location = new System.Drawing.Point(12, 117);
            this._nudFreq.Maximum = new decimal(new int[] {
            3000,
            0,
            0,
            0});
            this._nudFreq.Name = "_nudFreq";
            this._nudFreq.Size = new System.Drawing.Size(120, 20);
            this._nudFreq.TabIndex = 6;
            this._nudFreq.Value = new decimal(new int[] {
            318,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 140);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(74, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Top Wavelets";
            // 
            // _nudTopWavelets
            // 
            this._nudTopWavelets.Location = new System.Drawing.Point(12, 156);
            this._nudTopWavelets.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this._nudTopWavelets.Name = "_nudTopWavelets";
            this._nudTopWavelets.Size = new System.Drawing.Size(120, 20);
            this._nudTopWavelets.TabIndex = 8;
            this._nudTopWavelets.Value = new decimal(new int[] {
            200,
            0,
            0,
            0});
            // 
            // _btnDumpInfo
            // 
            this._btnDumpInfo.Location = new System.Drawing.Point(277, 281);
            this._btnDumpInfo.Name = "_btnDumpInfo";
            this._btnDumpInfo.Size = new System.Drawing.Size(75, 23);
            this._btnDumpInfo.TabIndex = 9;
            this._btnDumpInfo.Text = "Dump info!";
            this._btnDumpInfo.UseVisualStyleBackColor = true;
            this._btnDumpInfo.Click += new System.EventHandler(this.BtnDumpInfoClick);
            // 
            // _nudStride
            // 
            this._nudStride.Location = new System.Drawing.Point(148, 117);
            this._nudStride.Maximum = new decimal(new int[] {
            5512,
            0,
            0,
            0});
            this._nudStride.Name = "_nudStride";
            this._nudStride.Size = new System.Drawing.Size(120, 20);
            this._nudStride.TabIndex = 10;
            this._nudStride.Value = new decimal(new int[] {
            5115,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(148, 100);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(71, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "DB stride size";
            // 
            // _chbStride
            // 
            this._chbStride.AutoSize = true;
            this._chbStride.Location = new System.Drawing.Point(274, 120);
            this._chbStride.Name = "_chbStride";
            this._chbStride.Size = new System.Drawing.Size(66, 17);
            this._chbStride.TabIndex = 12;
            this._chbStride.Text = "Random";
            this._chbStride.UseVisualStyleBackColor = true;
            // 
            // _chbCompare
            // 
            this._chbCompare.AutoSize = true;
            this._chbCompare.Location = new System.Drawing.Point(12, 260);
            this._chbCompare.Name = "_chbCompare";
            this._chbCompare.Size = new System.Drawing.Size(155, 17);
            this._chbCompare.TabIndex = 13;
            this._chbCompare.Text = "Compare with another song";
            this._chbCompare.UseVisualStyleBackColor = true;
            this._chbCompare.CheckedChanged += new System.EventHandler(this.ChbCompareCheckedChanged);
            // 
            // _tbSongToCompare
            // 
            this._tbSongToCompare.Enabled = false;
            this._tbSongToCompare.Location = new System.Drawing.Point(12, 283);
            this._tbSongToCompare.Name = "_tbSongToCompare";
            this._tbSongToCompare.Size = new System.Drawing.Size(252, 20);
            this._tbSongToCompare.TabIndex = 14;
            this._tbSongToCompare.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.TbSongToCompareMouseDoubleClick);
            // 
            // _nudTables
            // 
            this._nudTables.Location = new System.Drawing.Point(12, 234);
            this._nudTables.Name = "_nudTables";
            this._nudTables.Size = new System.Drawing.Size(120, 20);
            this._nudTables.TabIndex = 15;
            this._nudTables.Value = new decimal(new int[] {
            25,
            0,
            0,
            0});
            // 
            // _nudKeys
            // 
            this._nudKeys.Location = new System.Drawing.Point(148, 234);
            this._nudKeys.Name = "_nudKeys";
            this._nudKeys.Size = new System.Drawing.Size(120, 20);
            this._nudKeys.TabIndex = 16;
            this._nudKeys.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 218);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(48, 13);
            this.label5.TabIndex = 17;
            this.label5.Text = "L Tables";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(148, 218);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(40, 13);
            this.label6.TabIndex = 18;
            this.label6.Text = "K Keys";
            // 
            // _nudQueryStride
            // 
            this._nudQueryStride.Location = new System.Drawing.Point(148, 156);
            this._nudQueryStride.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this._nudQueryStride.Name = "_nudQueryStride";
            this._nudQueryStride.Size = new System.Drawing.Size(120, 20);
            this._nudQueryStride.TabIndex = 19;
            this._nudQueryStride.Value = new decimal(new int[] {
            254,
            0,
            0,
            0});
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(148, 140);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(63, 13);
            this.label7.TabIndex = 20;
            this.label7.Text = "Query stride";
            // 
            // _chbQueryStride
            // 
            this._chbQueryStride.AutoSize = true;
            this._chbQueryStride.Checked = true;
            this._chbQueryStride.CheckState = System.Windows.Forms.CheckState.Checked;
            this._chbQueryStride.Location = new System.Drawing.Point(274, 159);
            this._chbQueryStride.Name = "_chbQueryStride";
            this._chbQueryStride.Size = new System.Drawing.Size(66, 17);
            this._chbQueryStride.TabIndex = 21;
            this._chbQueryStride.Text = "Random";
            this._chbQueryStride.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(148, 179);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(109, 13);
            this.label8.TabIndex = 22;
            this.label8.Text = "# of items to compare";
            // 
            // _nudNumberOfSubsequent
            // 
            this._nudNumberOfSubsequent.Location = new System.Drawing.Point(148, 195);
            this._nudNumberOfSubsequent.Name = "_nudNumberOfSubsequent";
            this._nudNumberOfSubsequent.Size = new System.Drawing.Size(120, 20);
            this._nudNumberOfSubsequent.TabIndex = 23;
            this._nudNumberOfSubsequent.Value = new decimal(new int[] {
            7,
            0,
            0,
            0});
            // 
            // WinMisc
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(364, 312);
            this.Controls.Add(this._nudNumberOfSubsequent);
            this.Controls.Add(this.label8);
            this.Controls.Add(this._chbQueryStride);
            this.Controls.Add(this.label7);
            this.Controls.Add(this._nudQueryStride);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this._nudKeys);
            this.Controls.Add(this._nudTables);
            this.Controls.Add(this._tbSongToCompare);
            this.Controls.Add(this._chbCompare);
            this.Controls.Add(this._chbStride);
            this.Controls.Add(this.label4);
            this.Controls.Add(this._nudStride);
            this.Controls.Add(this._btnDumpInfo);
            this.Controls.Add(this._nudTopWavelets);
            this.Controls.Add(this.label3);
            this.Controls.Add(this._nudFreq);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._tbOutputPath);
            this.Controls.Add(this._labSim);
            this.Controls.Add(this._tbPathToFile);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(380, 350);
            this.MinimumSize = new System.Drawing.Size(380, 350);
            this.Name = "WinMisc";
            this.Text = "Miscelaneous";
            this.Load += new System.EventHandler(this.WinMiscLoad);
            ((System.ComponentModel.ISupportInitialize)(this._nudFreq)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudTopWavelets)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudStride)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudTables)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudKeys)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudQueryStride)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudNumberOfSubsequent)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _tbPathToFile;
        private System.Windows.Forms.Label _labSim;
        private System.Windows.Forms.TextBox _tbOutputPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown _nudFreq;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown _nudTopWavelets;
        private System.Windows.Forms.Button _btnDumpInfo;
        private System.Windows.Forms.NumericUpDown _nudStride;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox _chbStride;
        private System.Windows.Forms.CheckBox _chbCompare;
        private System.Windows.Forms.TextBox _tbSongToCompare;
        private System.Windows.Forms.NumericUpDown _nudTables;
        private System.Windows.Forms.NumericUpDown _nudKeys;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown _nudQueryStride;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox _chbQueryStride;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown _nudNumberOfSubsequent;
    }
}