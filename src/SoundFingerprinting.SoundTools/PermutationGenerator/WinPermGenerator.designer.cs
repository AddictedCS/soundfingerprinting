namespace SoundFingerprinting.SoundTools.PermutationGenerator
{
    partial class WinPermGenerator
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
            this._btnSave = new System.Windows.Forms.Button();
            this._nudFrom = new System.Windows.Forms.NumericUpDown();
            this._nudTo = new System.Windows.Forms.NumericUpDown();
            this._gbRange = new System.Windows.Forms.GroupBox();
            this._labTo = new System.Windows.Forms.Label();
            this._labfrom = new System.Windows.Forms.Label();
            this._labpermCount = new System.Windows.Forms.Label();
            this._nudPermsCount = new System.Windows.Forms.NumericUpDown();
            this._nudLTables = new System.Windows.Forms.NumericUpDown();
            this._nudKeys = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this._pbProgress = new System.Windows.Forms.ProgressBar();
            this._btnSv = new System.Windows.Forms.Button();
            this._cbmAlgorithm = new System.Windows.Forms.ComboBox();
            this._labAlgorithm = new System.Windows.Forms.Label();
            this._lPermGen = new System.Windows.Forms.Label();
            this._btnGenerateHash = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this._lRows = new System.Windows.Forms.Label();
            this._nudBands = new System.Windows.Forms.NumericUpDown();
            this._nudRows = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this._nudStartPerm = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this._nudEndPerm = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this._nudFrom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudTo)).BeginInit();
            this._gbRange.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._nudPermsCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudLTables)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudKeys)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudBands)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudRows)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudStartPerm)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudEndPerm)).BeginInit();
            this.SuspendLayout();
            // 
            // _btnSave
            // 
            this._btnSave.Location = new System.Drawing.Point(486, 98);
            this._btnSave.Name = "_btnSave";
            this._btnSave.Size = new System.Drawing.Size(75, 23);
            this._btnSave.TabIndex = 2;
            this._btnSave.Text = "Generate!";
            this._btnSave.UseVisualStyleBackColor = true;
            this._btnSave.Click += new System.EventHandler(this.BtnSaveClick);
            // 
            // _nudFrom
            // 
            this._nudFrom.Location = new System.Drawing.Point(6, 36);
            this._nudFrom.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this._nudFrom.Name = "_nudFrom";
            this._nudFrom.Size = new System.Drawing.Size(120, 20);
            this._nudFrom.TabIndex = 6;
            // 
            // _nudTo
            // 
            this._nudTo.Location = new System.Drawing.Point(140, 36);
            this._nudTo.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this._nudTo.Name = "_nudTo";
            this._nudTo.Size = new System.Drawing.Size(114, 20);
            this._nudTo.TabIndex = 7;
            // 
            // _gbRange
            // 
            this._gbRange.Controls.Add(this._labTo);
            this._gbRange.Controls.Add(this._labfrom);
            this._gbRange.Controls.Add(this._nudFrom);
            this._gbRange.Controls.Add(this._nudTo);
            this._gbRange.Location = new System.Drawing.Point(18, 88);
            this._gbRange.Name = "_gbRange";
            this._gbRange.Size = new System.Drawing.Size(260, 64);
            this._gbRange.TabIndex = 8;
            this._gbRange.TabStop = false;
            this._gbRange.Text = "Range";
            // 
            // _labTo
            // 
            this._labTo.AutoSize = true;
            this._labTo.Location = new System.Drawing.Point(143, 20);
            this._labTo.Name = "_labTo";
            this._labTo.Size = new System.Drawing.Size(52, 13);
            this._labTo.TabIndex = 9;
            this._labTo.Text = "EndIndex";
            // 
            // _labfrom
            // 
            this._labfrom.AutoSize = true;
            this._labfrom.Location = new System.Drawing.Point(6, 20);
            this._labfrom.Name = "_labfrom";
            this._labfrom.Size = new System.Drawing.Size(58, 13);
            this._labfrom.TabIndex = 8;
            this._labfrom.Text = "Start Index";
            // 
            // _labpermCount
            // 
            this._labpermCount.AutoSize = true;
            this._labpermCount.Location = new System.Drawing.Point(15, 46);
            this._labpermCount.Name = "_labpermCount";
            this._labpermCount.Size = new System.Drawing.Size(98, 13);
            this._labpermCount.TabIndex = 9;
            this._labpermCount.Text = "Permutations count";
            // 
            // _nudPermsCount
            // 
            this._nudPermsCount.Location = new System.Drawing.Point(18, 62);
            this._nudPermsCount.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this._nudPermsCount.Name = "_nudPermsCount";
            this._nudPermsCount.Size = new System.Drawing.Size(120, 20);
            this._nudPermsCount.TabIndex = 10;
            // 
            // _nudLTables
            // 
            this._nudLTables.Location = new System.Drawing.Point(18, 23);
            this._nudLTables.Name = "_nudLTables";
            this._nudLTables.Size = new System.Drawing.Size(120, 20);
            this._nudLTables.TabIndex = 11;
            // 
            // _nudKeys
            // 
            this._nudKeys.Location = new System.Drawing.Point(158, 23);
            this._nudKeys.Name = "_nudKeys";
            this._nudKeys.Size = new System.Drawing.Size(120, 20);
            this._nudKeys.TabIndex = 12;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 13);
            this.label1.TabIndex = 13;
            this.label1.Text = "L Hash Tables";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(158, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(84, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "K Keys per table";
            // 
            // _pbProgress
            // 
            this._pbProgress.Location = new System.Drawing.Point(295, 59);
            this._pbProgress.Name = "_pbProgress";
            this._pbProgress.Size = new System.Drawing.Size(266, 23);
            this._pbProgress.TabIndex = 15;
            // 
            // _btnSv
            // 
            this._btnSv.Location = new System.Drawing.Point(486, 129);
            this._btnSv.Name = "_btnSv";
            this._btnSv.Size = new System.Drawing.Size(75, 23);
            this._btnSv.TabIndex = 16;
            this._btnSv.Text = "Save";
            this._btnSv.UseVisualStyleBackColor = true;
            this._btnSv.Click += new System.EventHandler(this.BtnSvClick);
            // 
            // _cbmAlgorithm
            // 
            this._cbmAlgorithm.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cbmAlgorithm.FormattingEnabled = true;
            this._cbmAlgorithm.Items.AddRange(new object[] {
            "Unique indexes across permutation (weak)",
            "Agressive selector (weak)",
            "Summed accross selector (average)",
            "Conservative selector (good)"});
            this._cbmAlgorithm.Location = new System.Drawing.Point(295, 22);
            this._cbmAlgorithm.Name = "_cbmAlgorithm";
            this._cbmAlgorithm.Size = new System.Drawing.Size(266, 21);
            this._cbmAlgorithm.TabIndex = 17;
            // 
            // _labAlgorithm
            // 
            this._labAlgorithm.AutoSize = true;
            this._labAlgorithm.Location = new System.Drawing.Point(292, 6);
            this._labAlgorithm.Name = "_labAlgorithm";
            this._labAlgorithm.Size = new System.Drawing.Size(50, 13);
            this._labAlgorithm.TabIndex = 18;
            this._labAlgorithm.Text = "Algorithm";
            // 
            // _lPermGen
            // 
            this._lPermGen.AutoSize = true;
            this._lPermGen.Location = new System.Drawing.Point(18, 169);
            this._lPermGen.Name = "_lPermGen";
            this._lPermGen.Size = new System.Drawing.Size(120, 13);
            this._lPermGen.TabIndex = 19;
            this._lPermGen.Text = "Perm generator for hash";
            // 
            // _btnGenerateHash
            // 
            this._btnGenerateHash.Location = new System.Drawing.Point(441, 243);
            this._btnGenerateHash.Name = "_btnGenerateHash";
            this._btnGenerateHash.Size = new System.Drawing.Size(120, 23);
            this._btnGenerateHash.TabIndex = 20;
            this._btnGenerateHash.Text = "Generate";
            this._btnGenerateHash.UseVisualStyleBackColor = true;
            this._btnGenerateHash.Click += new System.EventHandler(this.BtnGenerateHashClick);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(18, 191);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(37, 13);
            this.label3.TabIndex = 21;
            this.label3.Text = "Bands";
            // 
            // _lRows
            // 
            this._lRows.AutoSize = true;
            this._lRows.Location = new System.Drawing.Point(155, 191);
            this._lRows.Name = "_lRows";
            this._lRows.Size = new System.Drawing.Size(34, 13);
            this._lRows.TabIndex = 22;
            this._lRows.Text = "Rows";
            // 
            // _nudBands
            // 
            this._nudBands.Location = new System.Drawing.Point(18, 207);
            this._nudBands.Name = "_nudBands";
            this._nudBands.Size = new System.Drawing.Size(120, 20);
            this._nudBands.TabIndex = 23;
            this._nudBands.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            // 
            // _nudRows
            // 
            this._nudRows.Location = new System.Drawing.Point(155, 207);
            this._nudRows.Name = "_nudRows";
            this._nudRows.Size = new System.Drawing.Size(120, 20);
            this._nudRows.TabIndex = 24;
            this._nudRows.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(18, 230);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 13);
            this.label4.TabIndex = 25;
            this.label4.Text = "Start";
            // 
            // _nudStartPerm
            // 
            this._nudStartPerm.Location = new System.Drawing.Point(18, 246);
            this._nudStartPerm.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this._nudStartPerm.Name = "_nudStartPerm";
            this._nudStartPerm.Size = new System.Drawing.Size(120, 20);
            this._nudStartPerm.TabIndex = 26;
            this._nudStartPerm.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(155, 230);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(26, 13);
            this.label5.TabIndex = 27;
            this.label5.Text = "End";
            // 
            // _nudEndPerm
            // 
            this._nudEndPerm.Location = new System.Drawing.Point(155, 246);
            this._nudEndPerm.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this._nudEndPerm.Name = "_nudEndPerm";
            this._nudEndPerm.Size = new System.Drawing.Size(120, 20);
            this._nudEndPerm.TabIndex = 28;
            this._nudEndPerm.Value = new decimal(new int[] {
            255,
            0,
            0,
            0});
            // 
            // WinPermGenerator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(570, 278);
            this.Controls.Add(this._nudEndPerm);
            this.Controls.Add(this.label5);
            this.Controls.Add(this._nudStartPerm);
            this.Controls.Add(this.label4);
            this.Controls.Add(this._nudRows);
            this.Controls.Add(this._nudBands);
            this.Controls.Add(this._lRows);
            this.Controls.Add(this.label3);
            this.Controls.Add(this._btnGenerateHash);
            this.Controls.Add(this._lPermGen);
            this.Controls.Add(this._labAlgorithm);
            this.Controls.Add(this._cbmAlgorithm);
            this.Controls.Add(this._btnSv);
            this.Controls.Add(this._pbProgress);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._nudKeys);
            this.Controls.Add(this._nudLTables);
            this.Controls.Add(this._nudPermsCount);
            this.Controls.Add(this._labpermCount);
            this.Controls.Add(this._gbRange);
            this.Controls.Add(this._btnSave);
            this.Name = "WinPermGenerator";
            this.Text = "WinPermGenerator";
            ((System.ComponentModel.ISupportInitialize)(this._nudFrom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudTo)).EndInit();
            this._gbRange.ResumeLayout(false);
            this._gbRange.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._nudPermsCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudLTables)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudKeys)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudBands)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudRows)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudStartPerm)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudEndPerm)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _btnSave;
        private System.Windows.Forms.NumericUpDown _nudFrom;
        private System.Windows.Forms.NumericUpDown _nudTo;
        private System.Windows.Forms.GroupBox _gbRange;
        private System.Windows.Forms.Label _labfrom;
        private System.Windows.Forms.Label _labTo;
        private System.Windows.Forms.Label _labpermCount;
        private System.Windows.Forms.NumericUpDown _nudPermsCount;
        private System.Windows.Forms.NumericUpDown _nudLTables;
        private System.Windows.Forms.NumericUpDown _nudKeys;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ProgressBar _pbProgress;
        private System.Windows.Forms.Button _btnSv;
        private System.Windows.Forms.ComboBox _cbmAlgorithm;
        private System.Windows.Forms.Label _labAlgorithm;
        private System.Windows.Forms.Label _lPermGen;
        private System.Windows.Forms.Button _btnGenerateHash;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label _lRows;
        private System.Windows.Forms.NumericUpDown _nudBands;
        private System.Windows.Forms.NumericUpDown _nudRows;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown _nudStartPerm;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown _nudEndPerm;
    }
}