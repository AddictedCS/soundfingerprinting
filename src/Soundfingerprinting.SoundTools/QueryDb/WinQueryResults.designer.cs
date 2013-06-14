namespace SoundFingerprinting.SoundTools.QueryDb
{
    partial class WinQueryResults
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
            this._dgvResults = new System.Windows.Forms.DataGridView();
            this._btnStop = new System.Windows.Forms.Button();
            this._btnExport = new System.Windows.Forms.Button();
            this._labTotal = new System.Windows.Forms.Label();
            this._tbResults = new System.Windows.Forms.TextBox();
            this._nudTotal = new System.Windows.Forms.NumericUpDown();
            this._labTotalItems = new System.Windows.Forms.Label();
            this._nudChecked = new System.Windows.Forms.NumericUpDown();
            this._labCheked = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this._dgvResults)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudTotal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudChecked)).BeginInit();
            this.SuspendLayout();
            // 
            // _dgvResults
            // 
            this._dgvResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._dgvResults.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._dgvResults.Location = new System.Drawing.Point(12, 12);
            this._dgvResults.Name = "_dgvResults";
            this._dgvResults.ReadOnly = true;
            this._dgvResults.Size = new System.Drawing.Size(960, 384);
            this._dgvResults.TabIndex = 0;
            // 
            // _btnStop
            // 
            this._btnStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._btnStop.Location = new System.Drawing.Point(897, 433);
            this._btnStop.Name = "_btnStop";
            this._btnStop.Size = new System.Drawing.Size(75, 23);
            this._btnStop.TabIndex = 1;
            this._btnStop.Text = "Stop";
            this._btnStop.UseVisualStyleBackColor = true;
            this._btnStop.Click += new System.EventHandler(this.BtnStopClick);
            // 
            // _btnExport
            // 
            this._btnExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._btnExport.Location = new System.Drawing.Point(897, 404);
            this._btnExport.Name = "_btnExport";
            this._btnExport.Size = new System.Drawing.Size(75, 23);
            this._btnExport.TabIndex = 2;
            this._btnExport.Text = "Export";
            this._btnExport.UseVisualStyleBackColor = true;
            this._btnExport.Click += new System.EventHandler(this.BtnExportClick);
            // 
            // _labTotal
            // 
            this._labTotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._labTotal.AutoSize = true;
            this._labTotal.Location = new System.Drawing.Point(12, 452);
            this._labTotal.Name = "_labTotal";
            this._labTotal.Size = new System.Drawing.Size(64, 13);
            this._labTotal.TabIndex = 3;
            this._labTotal.Text = "Recognition";
            // 
            // _tbResults
            // 
            this._tbResults.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._tbResults.Location = new System.Drawing.Point(82, 448);
            this._tbResults.Name = "_tbResults";
            this._tbResults.ReadOnly = true;
            this._tbResults.Size = new System.Drawing.Size(120, 20);
            this._tbResults.TabIndex = 4;
            // 
            // _nudTotal
            // 
            this._nudTotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._nudTotal.Enabled = false;
            this._nudTotal.Location = new System.Drawing.Point(82, 400);
            this._nudTotal.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this._nudTotal.Name = "_nudTotal";
            this._nudTotal.ReadOnly = true;
            this._nudTotal.Size = new System.Drawing.Size(120, 20);
            this._nudTotal.TabIndex = 5;
            // 
            // _labTotalItems
            // 
            this._labTotalItems.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._labTotalItems.AutoSize = true;
            this._labTotalItems.Location = new System.Drawing.Point(12, 404);
            this._labTotalItems.Name = "_labTotalItems";
            this._labTotalItems.Size = new System.Drawing.Size(59, 13);
            this._labTotalItems.TabIndex = 6;
            this._labTotalItems.Text = "Total Items";
            // 
            // _nudChecked
            // 
            this._nudChecked.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._nudChecked.Enabled = false;
            this._nudChecked.Location = new System.Drawing.Point(82, 424);
            this._nudChecked.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this._nudChecked.Name = "_nudChecked";
            this._nudChecked.ReadOnly = true;
            this._nudChecked.Size = new System.Drawing.Size(120, 20);
            this._nudChecked.TabIndex = 7;
            // 
            // _labCheked
            // 
            this._labCheked.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._labCheked.AutoSize = true;
            this._labCheked.Location = new System.Drawing.Point(12, 426);
            this._labCheked.Name = "_labCheked";
            this._labCheked.Size = new System.Drawing.Size(50, 13);
            this._labCheked.TabIndex = 8;
            this._labCheked.Text = "Checked";
            // 
            // WinQueryResults
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 477);
            this.Controls.Add(this._labCheked);
            this.Controls.Add(this._nudChecked);
            this.Controls.Add(this._labTotalItems);
            this.Controls.Add(this._nudTotal);
            this.Controls.Add(this._tbResults);
            this.Controls.Add(this._labTotal);
            this.Controls.Add(this._btnExport);
            this.Controls.Add(this._btnStop);
            this.Controls.Add(this._dgvResults);
            this.Name = "WinQueryResults";
            this.Text = "Results";
            ((System.ComponentModel.ISupportInitialize)(this._dgvResults)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudTotal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudChecked)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView _dgvResults;
        private System.Windows.Forms.Button _btnStop;
        private System.Windows.Forms.Button _btnExport;
        private System.Windows.Forms.Label _labTotal;
        private System.Windows.Forms.TextBox _tbResults;
        private System.Windows.Forms.NumericUpDown _nudTotal;
        private System.Windows.Forms.Label _labTotalItems;
        private System.Windows.Forms.NumericUpDown _nudChecked;
        private System.Windows.Forms.Label _labCheked;
    }
}