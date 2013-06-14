namespace SoundFingerprinting.SoundTools.NetworkTrainer
{
    partial class WinNetworkTrainer
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
            this.components = new System.ComponentModel.Container();
            this._gbNetProperties = new System.Windows.Forms.GroupBox();
            this._cbLog = new System.Windows.Forms.CheckBox();
            this._btnCreateNetwork = new System.Windows.Forms.Button();
            this._gbActivationFunction = new System.Windows.Forms.GroupBox();
            this._cmbActivationFunctionOutput = new System.Windows.Forms.ComboBox();
            this._cmbActivationFunctionHidden = new System.Windows.Forms.ComboBox();
            this._cmbActivationFunction = new System.Windows.Forms.ComboBox();
            this._tbHiddenUnits = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this._gbLearnerProperties = new System.Windows.Forms.GroupBox();
            this._tbMomentum = new System.Windows.Forms.TextBox();
            this._tbLearningRate = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this._buttonStart = new System.Windows.Forms.Button();
            this._buttonPause = new System.Windows.Forms.Button();
            this._buttonAbort = new System.Windows.Forms.Button();
            this._gbTrainingStatus = new System.Windows.Forms.GroupBox();
            this._textBoxValidation = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this._tbCorrectOutputs = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this._textBoxError = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this._textBoxTimer = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this._textBoxIteration = new System.Windows.Forms.TextBox();
            this._tbStatus = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this._buttonSave = new System.Windows.Forms.Button();
            this._tElapsedTime = new System.Windows.Forms.Timer(this.components);
            this._gbNetProperties.SuspendLayout();
            this._gbActivationFunction.SuspendLayout();
            this._gbLearnerProperties.SuspendLayout();
            this._gbTrainingStatus.SuspendLayout();
            this.SuspendLayout();
            // 
            // _gbNetProperties
            // 
            this._gbNetProperties.Controls.Add(this._cbLog);
            this._gbNetProperties.Controls.Add(this._btnCreateNetwork);
            this._gbNetProperties.Controls.Add(this._gbActivationFunction);
            this._gbNetProperties.Controls.Add(this._tbHiddenUnits);
            this._gbNetProperties.Controls.Add(this.label1);
            this._gbNetProperties.Location = new System.Drawing.Point(13, 13);
            this._gbNetProperties.Name = "_gbNetProperties";
            this._gbNetProperties.Size = new System.Drawing.Size(247, 209);
            this._gbNetProperties.TabIndex = 0;
            this._gbNetProperties.TabStop = false;
            this._gbNetProperties.Text = "Net Properties";
            // 
            // _cbLog
            // 
            this._cbLog.AutoSize = true;
            this._cbLog.Location = new System.Drawing.Point(14, 186);
            this._cbLog.Name = "_cbLog";
            this._cbLog.Size = new System.Drawing.Size(95, 17);
            this._cbLog.TabIndex = 7;
            this._cbLog.Text = "Log the results";
            this._cbLog.UseVisualStyleBackColor = true;
            // 
            // _btnCreateNetwork
            // 
            this._btnCreateNetwork.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnCreateNetwork.Location = new System.Drawing.Point(161, 17);
            this._btnCreateNetwork.Name = "_btnCreateNetwork";
            this._btnCreateNetwork.Size = new System.Drawing.Size(75, 23);
            this._btnCreateNetwork.TabIndex = 6;
            this._btnCreateNetwork.Text = "Create";
            this._btnCreateNetwork.UseVisualStyleBackColor = true;
            this._btnCreateNetwork.Click += new System.EventHandler(this.BtnCreateClick);
            // 
            // _gbActivationFunction
            // 
            this._gbActivationFunction.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._gbActivationFunction.Controls.Add(this._cmbActivationFunctionOutput);
            this._gbActivationFunction.Controls.Add(this._cmbActivationFunctionHidden);
            this._gbActivationFunction.Controls.Add(this._cmbActivationFunction);
            this._gbActivationFunction.Location = new System.Drawing.Point(14, 45);
            this._gbActivationFunction.Name = "_gbActivationFunction";
            this._gbActivationFunction.Size = new System.Drawing.Size(222, 124);
            this._gbActivationFunction.TabIndex = 5;
            this._gbActivationFunction.TabStop = false;
            this._gbActivationFunction.Text = "Activation Functions";
            // 
            // _cmbActivationFunctionOutput
            // 
            this._cmbActivationFunctionOutput.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbActivationFunctionOutput.FormattingEnabled = true;
            this._cmbActivationFunctionOutput.Location = new System.Drawing.Point(6, 73);
            this._cmbActivationFunctionOutput.Name = "_cmbActivationFunctionOutput";
            this._cmbActivationFunctionOutput.Size = new System.Drawing.Size(208, 21);
            this._cmbActivationFunctionOutput.TabIndex = 6;
            // 
            // _cmbActivationFunctionHidden
            // 
            this._cmbActivationFunctionHidden.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbActivationFunctionHidden.FormattingEnabled = true;
            this._cmbActivationFunctionHidden.Location = new System.Drawing.Point(6, 46);
            this._cmbActivationFunctionHidden.Name = "_cmbActivationFunctionHidden";
            this._cmbActivationFunctionHidden.Size = new System.Drawing.Size(208, 21);
            this._cmbActivationFunctionHidden.TabIndex = 5;
            // 
            // _cmbActivationFunction
            // 
            this._cmbActivationFunction.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._cmbActivationFunction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbActivationFunction.FormattingEnabled = true;
            this._cmbActivationFunction.Location = new System.Drawing.Point(6, 19);
            this._cmbActivationFunction.Name = "_cmbActivationFunction";
            this._cmbActivationFunction.Size = new System.Drawing.Size(208, 21);
            this._cmbActivationFunction.TabIndex = 4;
            this._cmbActivationFunction.SelectedIndexChanged += new System.EventHandler(this.CmbActivationFunctionSelectedIndexChanged);
            // 
            // _tbHiddenUnits
            // 
            this._tbHiddenUnits.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._tbHiddenUnits.Location = new System.Drawing.Point(85, 19);
            this._tbHiddenUnits.Name = "_tbHiddenUnits";
            this._tbHiddenUnits.Size = new System.Drawing.Size(70, 20);
            this._tbHiddenUnits.TabIndex = 1;
            this._tbHiddenUnits.Text = "41";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Hidden Units";
            // 
            // _gbLearnerProperties
            // 
            this._gbLearnerProperties.Controls.Add(this._tbMomentum);
            this._gbLearnerProperties.Controls.Add(this._tbLearningRate);
            this._gbLearnerProperties.Controls.Add(this.label5);
            this._gbLearnerProperties.Controls.Add(this.label4);
            this._gbLearnerProperties.Location = new System.Drawing.Point(14, 228);
            this._gbLearnerProperties.Name = "_gbLearnerProperties";
            this._gbLearnerProperties.Size = new System.Drawing.Size(247, 72);
            this._gbLearnerProperties.TabIndex = 1;
            this._gbLearnerProperties.TabStop = false;
            this._gbLearnerProperties.Text = "Learner Properties";
            // 
            // _tbMomentum
            // 
            this._tbMomentum.Enabled = false;
            this._tbMomentum.Location = new System.Drawing.Point(96, 19);
            this._tbMomentum.Name = "_tbMomentum";
            this._tbMomentum.Size = new System.Drawing.Size(132, 20);
            this._tbMomentum.TabIndex = 3;
            this._tbMomentum.Text = "0";
            // 
            // _tbLearningRate
            // 
            this._tbLearningRate.Enabled = false;
            this._tbLearningRate.Location = new System.Drawing.Point(96, 45);
            this._tbLearningRate.Name = "_tbLearningRate";
            this._tbLearningRate.Size = new System.Drawing.Size(132, 20);
            this._tbLearningRate.TabIndex = 2;
            this._tbLearningRate.Text = "0,00025";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(16, 48);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(74, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "Learning Rate";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(16, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Momentum";
            // 
            // _buttonStart
            // 
            this._buttonStart.Enabled = false;
            this._buttonStart.Location = new System.Drawing.Point(267, 240);
            this._buttonStart.Name = "_buttonStart";
            this._buttonStart.Size = new System.Drawing.Size(90, 23);
            this._buttonStart.TabIndex = 2;
            this._buttonStart.Text = "Start";
            this._buttonStart.UseVisualStyleBackColor = true;
            this._buttonStart.Click += new System.EventHandler(this.ButtonStartClick);
            // 
            // _buttonPause
            // 
            this._buttonPause.Enabled = false;
            this._buttonPause.Location = new System.Drawing.Point(378, 240);
            this._buttonPause.Name = "_buttonPause";
            this._buttonPause.Size = new System.Drawing.Size(90, 23);
            this._buttonPause.TabIndex = 3;
            this._buttonPause.Text = "Pause";
            this._buttonPause.UseVisualStyleBackColor = true;
            this._buttonPause.Click += new System.EventHandler(this.ButtonPauseClick);
            // 
            // _buttonAbort
            // 
            this._buttonAbort.Enabled = false;
            this._buttonAbort.Location = new System.Drawing.Point(267, 276);
            this._buttonAbort.Name = "_buttonAbort";
            this._buttonAbort.Size = new System.Drawing.Size(90, 23);
            this._buttonAbort.TabIndex = 4;
            this._buttonAbort.Text = "Abort";
            this._buttonAbort.UseVisualStyleBackColor = true;
            this._buttonAbort.Click += new System.EventHandler(this.ButtonAbortClick);
            // 
            // _gbTrainingStatus
            // 
            this._gbTrainingStatus.Controls.Add(this._textBoxValidation);
            this._gbTrainingStatus.Controls.Add(this.label2);
            this._gbTrainingStatus.Controls.Add(this._tbCorrectOutputs);
            this._gbTrainingStatus.Controls.Add(this.label10);
            this._gbTrainingStatus.Controls.Add(this._textBoxError);
            this._gbTrainingStatus.Controls.Add(this.label9);
            this._gbTrainingStatus.Controls.Add(this._textBoxTimer);
            this._gbTrainingStatus.Controls.Add(this.label8);
            this._gbTrainingStatus.Controls.Add(this._textBoxIteration);
            this._gbTrainingStatus.Controls.Add(this._tbStatus);
            this._gbTrainingStatus.Controls.Add(this.label7);
            this._gbTrainingStatus.Controls.Add(this.label6);
            this._gbTrainingStatus.Location = new System.Drawing.Point(267, 13);
            this._gbTrainingStatus.Name = "_gbTrainingStatus";
            this._gbTrainingStatus.Size = new System.Drawing.Size(201, 221);
            this._gbTrainingStatus.TabIndex = 5;
            this._gbTrainingStatus.TabStop = false;
            this._gbTrainingStatus.Text = "Training Status";
            // 
            // _textBoxValidation
            // 
            this._textBoxValidation.Enabled = false;
            this._textBoxValidation.Location = new System.Drawing.Point(95, 149);
            this._textBoxValidation.Name = "_textBoxValidation";
            this._textBoxValidation.Size = new System.Drawing.Size(100, 20);
            this._textBoxValidation.TabIndex = 11;
            this._textBoxValidation.Text = "Not yet validated";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 152);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(30, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Valid";
            // 
            // _tbCorrectOutputs
            // 
            this._tbCorrectOutputs.Enabled = false;
            this._tbCorrectOutputs.Location = new System.Drawing.Point(95, 71);
            this._tbCorrectOutputs.Name = "_tbCorrectOutputs";
            this._tbCorrectOutputs.Size = new System.Drawing.Size(100, 20);
            this._tbCorrectOutputs.TabIndex = 9;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(7, 100);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(29, 13);
            this.label10.TabIndex = 8;
            this.label10.Text = "Error";
            // 
            // _textBoxError
            // 
            this._textBoxError.Enabled = false;
            this._textBoxError.Location = new System.Drawing.Point(95, 97);
            this._textBoxError.Name = "_textBoxError";
            this._textBoxError.Size = new System.Drawing.Size(100, 20);
            this._textBoxError.TabIndex = 7;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(7, 74);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(78, 13);
            this.label9.TabIndex = 6;
            this.label9.Text = "CorrectOutputs";
            // 
            // _textBoxTimer
            // 
            this._textBoxTimer.Enabled = false;
            this._textBoxTimer.Location = new System.Drawing.Point(95, 123);
            this._textBoxTimer.Name = "_textBoxTimer";
            this._textBoxTimer.Size = new System.Drawing.Size(100, 20);
            this._textBoxTimer.TabIndex = 5;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(7, 126);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(71, 13);
            this.label8.TabIndex = 4;
            this.label8.Text = "Time Elapsed";
            // 
            // _textBoxIteration
            // 
            this._textBoxIteration.Enabled = false;
            this._textBoxIteration.Location = new System.Drawing.Point(95, 45);
            this._textBoxIteration.Name = "_textBoxIteration";
            this._textBoxIteration.Size = new System.Drawing.Size(100, 20);
            this._textBoxIteration.TabIndex = 3;
            // 
            // _tbStatus
            // 
            this._tbStatus.Enabled = false;
            this._tbStatus.Location = new System.Drawing.Point(50, 19);
            this._tbStatus.Name = "_tbStatus";
            this._tbStatus.Size = new System.Drawing.Size(145, 20);
            this._tbStatus.TabIndex = 2;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(7, 48);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(82, 13);
            this.label7.TabIndex = 1;
            this.label7.Text = "Current Iteration";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 22);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(37, 13);
            this.label6.TabIndex = 0;
            this.label6.Text = "Status";
            // 
            // _buttonSave
            // 
            this._buttonSave.Enabled = false;
            this._buttonSave.Location = new System.Drawing.Point(378, 276);
            this._buttonSave.Name = "_buttonSave";
            this._buttonSave.Size = new System.Drawing.Size(90, 23);
            this._buttonSave.TabIndex = 6;
            this._buttonSave.Text = "Save";
            this._buttonSave.UseVisualStyleBackColor = true;
            this._buttonSave.Click += new System.EventHandler(this.BtnSaveClick);
            // 
            // _tElapsedTime
            // 
            this._tElapsedTime.Interval = 1000;
            this._tElapsedTime.Tick += new System.EventHandler(this.Timer1Tick);
            // 
            // WinNetworkTrainer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(474, 312);
            this.Controls.Add(this._buttonSave);
            this.Controls.Add(this._gbTrainingStatus);
            this.Controls.Add(this._buttonAbort);
            this.Controls.Add(this._buttonPause);
            this.Controls.Add(this._buttonStart);
            this.Controls.Add(this._gbLearnerProperties);
            this.Controls.Add(this._gbNetProperties);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(490, 350);
            this.MinimumSize = new System.Drawing.Size(490, 350);
            this.Name = "WinNetworkTrainer";
            this.Text = "Network Ensemble Trainer";
            this._gbNetProperties.ResumeLayout(false);
            this._gbNetProperties.PerformLayout();
            this._gbActivationFunction.ResumeLayout(false);
            this._gbLearnerProperties.ResumeLayout(false);
            this._gbLearnerProperties.PerformLayout();
            this._gbTrainingStatus.ResumeLayout(false);
            this._gbTrainingStatus.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox _gbNetProperties;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox _tbHiddenUnits;
        private System.Windows.Forms.ComboBox _cmbActivationFunction;
        private System.Windows.Forms.GroupBox _gbActivationFunction;
        private System.Windows.Forms.GroupBox _gbLearnerProperties;
        private System.Windows.Forms.Button _btnCreateNetwork;
        private System.Windows.Forms.TextBox _tbMomentum;
        private System.Windows.Forms.TextBox _tbLearningRate;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button _buttonStart;
        private System.Windows.Forms.Button _buttonPause;
        private System.Windows.Forms.Button _buttonAbort;
        private System.Windows.Forms.GroupBox _gbTrainingStatus;
        private System.Windows.Forms.TextBox _textBoxIteration;
        private System.Windows.Forms.TextBox _tbStatus;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button _buttonSave;
        private System.Windows.Forms.TextBox _textBoxTimer;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox _textBoxError;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Timer _tElapsedTime;
        private System.Windows.Forms.TextBox _tbCorrectOutputs;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox _textBoxValidation;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox _cbLog;
        private System.Windows.Forms.ComboBox _cmbActivationFunctionOutput;
        private System.Windows.Forms.ComboBox _cmbActivationFunctionHidden;
    }
}