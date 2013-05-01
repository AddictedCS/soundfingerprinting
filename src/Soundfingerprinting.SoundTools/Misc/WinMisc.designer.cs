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
            this._nudMinFrequency = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this._nudTopWavelets = new System.Windows.Forms.NumericUpDown();
            this._btnDumpInfo = new System.Windows.Forms.Button();
            this._nudDatabaseStride = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this._chbDatabaseStride = new System.Windows.Forms.CheckBox();
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
            this._nudCandidateThreshold = new System.Windows.Forms.NumericUpDown();
            this._nudSecondsToProcess = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this._nudStartAtSecond = new System.Windows.Forms.NumericUpDown();
            this.label11 = new System.Windows.Forms.Label();
            this._cbUseNoWindow = new System.Windows.Forms.CheckBox();
            this._cbNormalize = new System.Windows.Forms.CheckBox();
            this._nudIterations = new System.Windows.Forms.NumericUpDown();
            this.label12 = new System.Windows.Forms.Label();
            this._nudFirstQueryStride = new System.Windows.Forms.NumericUpDown();
            this.label13 = new System.Windows.Forms.Label();
            this._cbDynamicLog = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this._nudMinFrequency)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudTopWavelets)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudDatabaseStride)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudTables)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudKeys)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudQueryStride)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudCandidateThreshold)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudSecondsToProcess)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudStartAtSecond)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudIterations)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudFirstQueryStride)).BeginInit();
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
            // _nudMinFrequency
            // 
            this._nudMinFrequency.Location = new System.Drawing.Point(12, 117);
            this._nudMinFrequency.Maximum = new decimal(new int[] {
            3000,
            0,
            0,
            0});
            this._nudMinFrequency.Name = "_nudMinFrequency";
            this._nudMinFrequency.Size = new System.Drawing.Size(120, 20);
            this._nudMinFrequency.TabIndex = 6;
            this._nudMinFrequency.Value = new decimal(new int[] {
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
            this._btnDumpInfo.Location = new System.Drawing.Point(435, 298);
            this._btnDumpInfo.Name = "_btnDumpInfo";
            this._btnDumpInfo.Size = new System.Drawing.Size(75, 23);
            this._btnDumpInfo.TabIndex = 9;
            this._btnDumpInfo.Text = "Dump info!";
            this._btnDumpInfo.UseVisualStyleBackColor = true;
            this._btnDumpInfo.Click += new System.EventHandler(this.BtnDumpInfoClick);
            // 
            // _nudDatabaseStride
            // 
            this._nudDatabaseStride.Location = new System.Drawing.Point(148, 117);
            this._nudDatabaseStride.Maximum = new decimal(new int[] {
            5512,
            0,
            0,
            0});
            this._nudDatabaseStride.Name = "_nudDatabaseStride";
            this._nudDatabaseStride.Size = new System.Drawing.Size(120, 20);
            this._nudDatabaseStride.TabIndex = 10;
            this._nudDatabaseStride.Value = new decimal(new int[] {
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
            // _chbDatabaseStride
            // 
            this._chbDatabaseStride.AutoSize = true;
            this._chbDatabaseStride.Location = new System.Drawing.Point(274, 120);
            this._chbDatabaseStride.Name = "_chbDatabaseStride";
            this._chbDatabaseStride.Size = new System.Drawing.Size(66, 17);
            this._chbDatabaseStride.TabIndex = 12;
            this._chbDatabaseStride.Text = "Random";
            this._chbDatabaseStride.UseVisualStyleBackColor = true;
            // 
            // _chbCompare
            // 
            this._chbCompare.AutoSize = true;
            this._chbCompare.Location = new System.Drawing.Point(12, 278);
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
            this._tbSongToCompare.Location = new System.Drawing.Point(12, 301);
            this._tbSongToCompare.Name = "_tbSongToCompare";
            this._tbSongToCompare.Size = new System.Drawing.Size(252, 20);
            this._tbSongToCompare.TabIndex = 14;
            this._tbSongToCompare.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.TbSongToCompareMouseDoubleClick);
            // 
            // _nudTables
            // 
            this._nudTables.Location = new System.Drawing.Point(12, 252);
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
            this._nudKeys.Location = new System.Drawing.Point(148, 252);
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
            this.label5.Location = new System.Drawing.Point(12, 236);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(48, 13);
            this.label5.TabIndex = 17;
            this.label5.Text = "L Tables";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(145, 236);
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
            5515,
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
            this.label8.Location = new System.Drawing.Point(274, 236);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(120, 13);
            this.label8.TabIndex = 22;
            this.label8.Text = "# of votes for candidate";
            // 
            // _nudCandidateThreshold
            // 
            this._nudCandidateThreshold.Location = new System.Drawing.Point(277, 252);
            this._nudCandidateThreshold.Name = "_nudCandidateThreshold";
            this._nudCandidateThreshold.Size = new System.Drawing.Size(120, 20);
            this._nudCandidateThreshold.TabIndex = 23;
            this._nudCandidateThreshold.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // _nudSecondsToProcess
            // 
            this._nudSecondsToProcess.Location = new System.Drawing.Point(12, 203);
            this._nudSecondsToProcess.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this._nudSecondsToProcess.Name = "_nudSecondsToProcess";
            this._nudSecondsToProcess.Size = new System.Drawing.Size(120, 20);
            this._nudSecondsToProcess.TabIndex = 24;
            this._nudSecondsToProcess.Value = new decimal(new int[] {
            25,
            0,
            0,
            0});
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(9, 187);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(101, 13);
            this.label9.TabIndex = 25;
            this.label9.Text = "Seconds to process";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(148, 187);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(79, 13);
            this.label10.TabIndex = 26;
            this.label10.Text = "Start at second";
            // 
            // _nudStartAtSecond
            // 
            this._nudStartAtSecond.Location = new System.Drawing.Point(148, 203);
            this._nudStartAtSecond.Name = "_nudStartAtSecond";
            this._nudStartAtSecond.Size = new System.Drawing.Size(120, 20);
            this._nudStartAtSecond.TabIndex = 27;
            this._nudStartAtSecond.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(277, 187);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(87, 13);
            this.label11.TabIndex = 29;
            this.label11.Text = "Window function";
            // 
            // _cbUseNoWindow
            // 
            this._cbUseNoWindow.AutoSize = true;
            this._cbUseNoWindow.Checked = true;
            this._cbUseNoWindow.CheckState = System.Windows.Forms.CheckState.Checked;
            this._cbUseNoWindow.Location = new System.Drawing.Point(280, 204);
            this._cbUseNoWindow.Name = "_cbUseNoWindow";
            this._cbUseNoWindow.Size = new System.Drawing.Size(82, 17);
            this._cbUseNoWindow.TabIndex = 30;
            this._cbUseNoWindow.Text = "No Window";
            this._cbUseNoWindow.UseVisualStyleBackColor = true;
            // 
            // _cbNormalize
            // 
            this._cbNormalize.AutoSize = true;
            this._cbNormalize.Checked = true;
            this._cbNormalize.CheckState = System.Windows.Forms.CheckState.Checked;
            this._cbNormalize.Location = new System.Drawing.Point(390, 204);
            this._cbNormalize.Name = "_cbNormalize";
            this._cbNormalize.Size = new System.Drawing.Size(102, 17);
            this._cbNormalize.TabIndex = 31;
            this._cbNormalize.Text = "Normalize signal";
            this._cbNormalize.UseVisualStyleBackColor = true;
            // 
            // _nudIterations
            // 
            this._nudIterations.Location = new System.Drawing.Point(390, 117);
            this._nudIterations.Name = "_nudIterations";
            this._nudIterations.Size = new System.Drawing.Size(120, 20);
            this._nudIterations.TabIndex = 32;
            this._nudIterations.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(390, 101);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(50, 13);
            this.label12.TabIndex = 33;
            this.label12.Text = "Iterations";
            // 
            // _nudFirstQueryStride
            // 
            this._nudFirstQueryStride.Location = new System.Drawing.Point(390, 156);
            this._nudFirstQueryStride.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this._nudFirstQueryStride.Name = "_nudFirstQueryStride";
            this._nudFirstQueryStride.Size = new System.Drawing.Size(120, 20);
            this._nudFirstQueryStride.TabIndex = 34;
            this._nudFirstQueryStride.Value = new decimal(new int[] {
            2557,
            0,
            0,
            0});
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(387, 140);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(83, 13);
            this.label13.TabIndex = 35;
            this.label13.Text = "First query stride";
            // 
            // _cbDynamicLog
            // 
            this._cbDynamicLog.AutoSize = true;
            this._cbDynamicLog.Checked = true;
            this._cbDynamicLog.CheckState = System.Windows.Forms.CheckState.Checked;
            this._cbDynamicLog.Location = new System.Drawing.Point(390, 187);
            this._cbDynamicLog.Name = "_cbDynamicLog";
            this._cbDynamicLog.Size = new System.Drawing.Size(111, 17);
            this._cbDynamicLog.TabIndex = 36;
            this._cbDynamicLog.Text = "Dynamic Logbase";
            this._cbDynamicLog.UseVisualStyleBackColor = true;
            // 
            // WinMisc
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(520, 335);
            this.Controls.Add(this._cbDynamicLog);
            this.Controls.Add(this.label13);
            this.Controls.Add(this._nudFirstQueryStride);
            this.Controls.Add(this.label12);
            this.Controls.Add(this._nudIterations);
            this.Controls.Add(this._cbNormalize);
            this.Controls.Add(this._cbUseNoWindow);
            this.Controls.Add(this.label11);
            this.Controls.Add(this._nudStartAtSecond);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label9);
            this.Controls.Add(this._nudSecondsToProcess);
            this.Controls.Add(this._nudCandidateThreshold);
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
            this.Controls.Add(this._chbDatabaseStride);
            this.Controls.Add(this.label4);
            this.Controls.Add(this._nudDatabaseStride);
            this.Controls.Add(this._btnDumpInfo);
            this.Controls.Add(this._nudTopWavelets);
            this.Controls.Add(this.label3);
            this.Controls.Add(this._nudMinFrequency);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._tbOutputPath);
            this.Controls.Add(this._labSim);
            this.Controls.Add(this._tbPathToFile);
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(380, 350);
            this.Name = "WinMisc";
            this.Text = "Miscelaneous";
            ((System.ComponentModel.ISupportInitialize)(this._nudMinFrequency)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudTopWavelets)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudDatabaseStride)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudTables)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudKeys)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudQueryStride)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudCandidateThreshold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudSecondsToProcess)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudStartAtSecond)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudIterations)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudFirstQueryStride)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _tbPathToFile;
        private System.Windows.Forms.Label _labSim;
        private System.Windows.Forms.TextBox _tbOutputPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown _nudMinFrequency;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown _nudTopWavelets;
        private System.Windows.Forms.Button _btnDumpInfo;
        private System.Windows.Forms.NumericUpDown _nudDatabaseStride;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox _chbDatabaseStride;
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
        private System.Windows.Forms.NumericUpDown _nudCandidateThreshold;
        private System.Windows.Forms.NumericUpDown _nudSecondsToProcess;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.NumericUpDown _nudStartAtSecond;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.CheckBox _cbUseNoWindow;
        private System.Windows.Forms.CheckBox _cbNormalize;
        private System.Windows.Forms.NumericUpDown _nudIterations;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.NumericUpDown _nudFirstQueryStride;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.CheckBox _cbDynamicLog;
    }
}