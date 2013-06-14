namespace SoundFingerprinting.SoundTools.FilePermutations
{
    partial class WinFilePermutation
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
            this._tbStartFolder = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this._tbEndFolder = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this._nudItems = new System.Windows.Forms.NumericUpDown();
            this._chbDeletePrevious = new System.Windows.Forms.CheckBox();
            this._btnBrowseStart = new System.Windows.Forms.Button();
            this._btnBrowseEnd = new System.Windows.Forms.Button();
            this._btnPermute = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this._nudItems)).BeginInit();
            this.SuspendLayout();
            // 
            // _tbStartFolder
            // 
            this._tbStartFolder.Location = new System.Drawing.Point(12, 25);
            this._tbStartFolder.Name = "_tbStartFolder";
            this._tbStartFolder.Size = new System.Drawing.Size(350, 20);
            this._tbStartFolder.TabIndex = 0;
            this._tbStartFolder.TextChanged += new System.EventHandler(this.TextBox1TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Start folder";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 61);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "End folder";
            // 
            // _tbEndFolder
            // 
            this._tbEndFolder.Location = new System.Drawing.Point(12, 80);
            this._tbEndFolder.Name = "_tbEndFolder";
            this._tbEndFolder.Size = new System.Drawing.Size(350, 20);
            this._tbEndFolder.TabIndex = 3;
            this._tbEndFolder.TextChanged += new System.EventHandler(this.TbEndFolderTextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(15, 128);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Number of items";
            // 
            // _nudItems
            // 
            this._nudItems.Location = new System.Drawing.Point(12, 144);
            this._nudItems.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this._nudItems.Name = "_nudItems";
            this._nudItems.Size = new System.Drawing.Size(120, 20);
            this._nudItems.TabIndex = 5;
            // 
            // _chbDeletePrevious
            // 
            this._chbDeletePrevious.AutoSize = true;
            this._chbDeletePrevious.Checked = true;
            this._chbDeletePrevious.CheckState = System.Windows.Forms.CheckState.Checked;
            this._chbDeletePrevious.Location = new System.Drawing.Point(12, 106);
            this._chbDeletePrevious.Name = "_chbDeletePrevious";
            this._chbDeletePrevious.Size = new System.Drawing.Size(101, 17);
            this._chbDeletePrevious.TabIndex = 6;
            this._chbDeletePrevious.Text = "Delete Previous";
            this._chbDeletePrevious.UseVisualStyleBackColor = true;
            // 
            // _btnBrowseStart
            // 
            this._btnBrowseStart.Location = new System.Drawing.Point(287, 51);
            this._btnBrowseStart.Name = "_btnBrowseStart";
            this._btnBrowseStart.Size = new System.Drawing.Size(75, 23);
            this._btnBrowseStart.TabIndex = 7;
            this._btnBrowseStart.Text = "Browse";
            this._btnBrowseStart.UseVisualStyleBackColor = true;
            this._btnBrowseStart.Click += new System.EventHandler(this.BtnBrowseStartClick);
            // 
            // _btnBrowseEnd
            // 
            this._btnBrowseEnd.Location = new System.Drawing.Point(287, 102);
            this._btnBrowseEnd.Name = "_btnBrowseEnd";
            this._btnBrowseEnd.Size = new System.Drawing.Size(75, 23);
            this._btnBrowseEnd.TabIndex = 8;
            this._btnBrowseEnd.Text = "Browse";
            this._btnBrowseEnd.UseVisualStyleBackColor = true;
            this._btnBrowseEnd.Click += new System.EventHandler(this.BtnBrowseEndClick);
            // 
            // _btnPermute
            // 
            this._btnPermute.Location = new System.Drawing.Point(287, 141);
            this._btnPermute.Name = "_btnPermute";
            this._btnPermute.Size = new System.Drawing.Size(75, 23);
            this._btnPermute.TabIndex = 9;
            this._btnPermute.Text = "Permute!";
            this._btnPermute.UseVisualStyleBackColor = true;
            this._btnPermute.Click += new System.EventHandler(this.BtnPermuteClick);
            // 
            // WinFilePermutation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(374, 177);
            this.Controls.Add(this._btnPermute);
            this.Controls.Add(this._btnBrowseEnd);
            this.Controls.Add(this._btnBrowseStart);
            this.Controls.Add(this._chbDeletePrevious);
            this.Controls.Add(this._nudItems);
            this.Controls.Add(this.label3);
            this.Controls.Add(this._tbEndFolder);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._tbStartFolder);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(390, 215);
            this.MinimumSize = new System.Drawing.Size(390, 215);
            this.Name = "WinFilePermutation";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Permute files";
            ((System.ComponentModel.ISupportInitialize)(this._nudItems)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _tbStartFolder;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox _tbEndFolder;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown _nudItems;
        private System.Windows.Forms.CheckBox _chbDeletePrevious;
        private System.Windows.Forms.Button _btnBrowseStart;
        private System.Windows.Forms.Button _btnBrowseEnd;
        private System.Windows.Forms.Button _btnPermute;
    }
}