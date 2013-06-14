namespace SoundFingerprinting.SoundTools.WaveletDecomposition
{
    partial class WinHaarWavelet
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
            this._tbImageToDecompose = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this._btnDecompose = new System.Windows.Forms.Button();
            this._tbSaveImage = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // _tbImageToDecompose
            // 
            this._tbImageToDecompose.Location = new System.Drawing.Point(15, 25);
            this._tbImageToDecompose.Name = "_tbImageToDecompose";
            this._tbImageToDecompose.Size = new System.Drawing.Size(292, 20);
            this._tbImageToDecompose.TabIndex = 0;
            this._tbImageToDecompose.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.TbImageToDecomposeMouseDoubleClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(204, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Select image to decompose (double click)";
            // 
            // _btnDecompose
            // 
            this._btnDecompose.Location = new System.Drawing.Point(232, 77);
            this._btnDecompose.Name = "_btnDecompose";
            this._btnDecompose.Size = new System.Drawing.Size(75, 23);
            this._btnDecompose.TabIndex = 2;
            this._btnDecompose.Text = "Decompose";
            this._btnDecompose.UseVisualStyleBackColor = true;
            this._btnDecompose.Click += new System.EventHandler(this.BtnDecomposeClick);
            // 
            // _tbSaveImage
            // 
            this._tbSaveImage.Location = new System.Drawing.Point(15, 51);
            this._tbSaveImage.Name = "_tbSaveImage";
            this._tbSaveImage.Size = new System.Drawing.Size(292, 20);
            this._tbSaveImage.TabIndex = 3;
            this._tbSaveImage.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.TbSaveImageMouseDoubleClick);
            // 
            // WinHaarWavelet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(319, 112);
            this.Controls.Add(this._tbSaveImage);
            this.Controls.Add(this._btnDecompose);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._tbImageToDecompose);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(335, 150);
            this.MinimumSize = new System.Drawing.Size(335, 150);
            this.Name = "WinHaarWavelet";
            this.Text = "WinHaarWavelet";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _tbImageToDecompose;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button _btnDecompose;
        private System.Windows.Forms.TextBox _tbSaveImage;
    }
}