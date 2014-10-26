namespace SoundFingerprinting.SoundTools.SpectralImagesCreator
{
    partial class WinSpectralImagesCreator
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
            this.tbPathToRoot = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnInsertSpectralImages = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(163, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Path to root folder with audio files";
            // 
            // tbPathToRoot
            // 
            this.tbPathToRoot.Location = new System.Drawing.Point(12, 34);
            this.tbPathToRoot.Name = "tbPathToRoot";
            this.tbPathToRoot.Size = new System.Drawing.Size(374, 20);
            this.tbPathToRoot.TabIndex = 1;
            this.tbPathToRoot.Text = "Double click to select";
            this.tbPathToRoot.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.TbSelectRootFolder);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 86);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "To insert:";
            // 
            // btnInsertSpectralImages
            // 
            this.btnInsertSpectralImages.Location = new System.Drawing.Point(306, 114);
            this.btnInsertSpectralImages.Name = "btnInsertSpectralImages";
            this.btnInsertSpectralImages.Size = new System.Drawing.Size(75, 23);
            this.btnInsertSpectralImages.TabIndex = 3;
            this.btnInsertSpectralImages.Text = "Insert";
            this.btnInsertSpectralImages.UseVisualStyleBackColor = true;
            this.btnInsertSpectralImages.Click += new System.EventHandler(this.BtnInsertSpectralImagesClick);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 67);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(163, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Number of songs in data source: ";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 103);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(48, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Inserted:";
            // 
            // WinSpectralImagesCreator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(393, 141);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnInsertSpectralImages);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbPathToRoot);
            this.Controls.Add(this.label1);
            this.Name = "WinSpectralImagesCreator";
            this.Text = "Neural Hasher Training Set Creator";
            this.Load += new System.EventHandler(this.WinSpectralImagesCreatorLoad);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbPathToRoot;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnInsertSpectralImages;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
    }
}