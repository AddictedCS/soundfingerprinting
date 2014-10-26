namespace SoundFingerprinting.SoundTools.NeuralHasher
{
    partial class WinNeuralHasher
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
            this.nudHiddenLayers = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.btnTrain = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.nudOutputCount = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.nudHiddenLayers)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOutputCount)).BeginInit();
            this.SuspendLayout();
            // 
            // nudHiddenLayers
            // 
            this.nudHiddenLayers.Location = new System.Drawing.Point(144, 7);
            this.nudHiddenLayers.Name = "nudHiddenLayers";
            this.nudHiddenLayers.Size = new System.Drawing.Size(120, 20);
            this.nudHiddenLayers.TabIndex = 0;
            this.nudHiddenLayers.Value = new decimal(new int[] {
            23,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Hidden Layers";
            // 
            // btnTrain
            // 
            this.btnTrain.Location = new System.Drawing.Point(189, 106);
            this.btnTrain.Name = "btnTrain";
            this.btnTrain.Size = new System.Drawing.Size(75, 23);
            this.btnTrain.TabIndex = 2;
            this.btnTrain.Text = "Train";
            this.btnTrain.UseVisualStyleBackColor = true;
            this.btnTrain.Click += new System.EventHandler(this.BtnTrainClick);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Output Count";
            // 
            // nudOutputCount
            // 
            this.nudOutputCount.Location = new System.Drawing.Point(144, 34);
            this.nudOutputCount.Name = "nudOutputCount";
            this.nudOutputCount.Size = new System.Drawing.Size(120, 20);
            this.nudOutputCount.TabIndex = 4;
            this.nudOutputCount.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // WinNeuralHasher
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(264, 130);
            this.Controls.Add(this.nudOutputCount);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnTrain);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.nudHiddenLayers);
            this.Name = "WinNeuralHasher";
            this.Text = "Train neural network";
            ((System.ComponentModel.ISupportInitialize)(this.nudHiddenLayers)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOutputCount)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown nudHiddenLayers;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnTrain;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown nudOutputCount;
    }
}