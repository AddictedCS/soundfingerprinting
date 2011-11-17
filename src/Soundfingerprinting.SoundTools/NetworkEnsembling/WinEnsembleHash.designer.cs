namespace Soundfingerprinting.SoundTools.NetworkEnsembling
{
    partial class WinEnsembleHash
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
            this._tbActionControl = new System.Windows.Forms.TabControl();
            this._tpEnsemble = new System.Windows.Forms.TabPage();
            this._tbSelectedNetworks = new System.Windows.Forms.TextBox();
            this._lbSelected = new System.Windows.Forms.Label();
            this._cmbConnectionStringEnsemble = new System.Windows.Forms.ComboBox();
            this._lbConnectionString = new System.Windows.Forms.Label();
            this._btnInsert = new System.Windows.Forms.Button();
            this._btnSaveEnsamble = new System.Windows.Forms.Button();
            this._btnStart = new System.Windows.Forms.Button();
            this._lbNetworks = new System.Windows.Forms.Label();
            this._dgvNetworks = new System.Windows.Forms.DataGridView();
            this.PathToNetwork = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SelectNework = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this._nudNumberOfHashesPerKeyNeuralHasher = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this._nudNumberOfGroupsNeuralHasher = new System.Windows.Forms.NumericUpDown();
            this._lbGroupNumbers = new System.Windows.Forms.Label();
            this._tbSaveToEnsembleFilename = new System.Windows.Forms.TextBox();
            this._lbFilename = new System.Windows.Forms.Label();
            this._tpHashBins = new System.Windows.Forms.TabPage();
            this._lbInsert = new System.Windows.Forms.Label();
            this._pbProgress = new System.Windows.Forms.ProgressBar();
            this._btnSelectStoredEnsemble = new System.Windows.Forms.Button();
            this._tbStoredEnsembleFilename = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this._bntComputeHashesNeuralHasher = new System.Windows.Forms.Button();
            this._lbConnectionStringValidation = new System.Windows.Forms.Label();
            this._cmbValidationConnectionStringNeuralHasher = new System.Windows.Forms.ComboBox();
            this._tbMinHash = new System.Windows.Forms.TabPage();
            this._gbSpread = new System.Windows.Forms.GroupBox();
            this._pbSpread = new System.Windows.Forms.ProgressBar();
            this._btnSpread = new System.Windows.Forms.Button();
            this._gbMain = new System.Windows.Forms.GroupBox();
            this._nudNumberOfHashesPerKeyMinHash = new System.Windows.Forms.NumericUpDown();
            this._cmbConnectionStringMinHash = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this._pbMinHash = new System.Windows.Forms.ProgressBar();
            this._nudNumberOfGroupsMinHash = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this._btnStartMinHash = new System.Windows.Forms.Button();
            this._sfdEnsembleSave = new System.Windows.Forms.SaveFileDialog();
            this._fbdSelectNetworks = new System.Windows.Forms.FolderBrowserDialog();
            this._ofdSelectEnsemble = new System.Windows.Forms.OpenFileDialog();
            this._tbActionControl.SuspendLayout();
            this._tpEnsemble.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._dgvNetworks)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudNumberOfHashesPerKeyNeuralHasher)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudNumberOfGroupsNeuralHasher)).BeginInit();
            this._tpHashBins.SuspendLayout();
            this._tbMinHash.SuspendLayout();
            this._gbSpread.SuspendLayout();
            this._gbMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._nudNumberOfHashesPerKeyMinHash)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudNumberOfGroupsMinHash)).BeginInit();
            this.SuspendLayout();
            // 
            // _tbActionControl
            // 
            this._tbActionControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._tbActionControl.Controls.Add(this._tpEnsemble);
            this._tbActionControl.Controls.Add(this._tpHashBins);
            this._tbActionControl.Controls.Add(this._tbMinHash);
            this._tbActionControl.Location = new System.Drawing.Point(12, 12);
            this._tbActionControl.Name = "_tbActionControl";
            this._tbActionControl.SelectedIndex = 0;
            this._tbActionControl.Size = new System.Drawing.Size(567, 477);
            this._tbActionControl.TabIndex = 0;
            // 
            // _tpEnsemble
            // 
            this._tpEnsemble.BackColor = System.Drawing.SystemColors.Control;
            this._tpEnsemble.Controls.Add(this._tbSelectedNetworks);
            this._tpEnsemble.Controls.Add(this._lbSelected);
            this._tpEnsemble.Controls.Add(this._cmbConnectionStringEnsemble);
            this._tpEnsemble.Controls.Add(this._lbConnectionString);
            this._tpEnsemble.Controls.Add(this._btnInsert);
            this._tpEnsemble.Controls.Add(this._btnSaveEnsamble);
            this._tpEnsemble.Controls.Add(this._btnStart);
            this._tpEnsemble.Controls.Add(this._lbNetworks);
            this._tpEnsemble.Controls.Add(this._dgvNetworks);
            this._tpEnsemble.Controls.Add(this._nudNumberOfHashesPerKeyNeuralHasher);
            this._tpEnsemble.Controls.Add(this.label1);
            this._tpEnsemble.Controls.Add(this._nudNumberOfGroupsNeuralHasher);
            this._tpEnsemble.Controls.Add(this._lbGroupNumbers);
            this._tpEnsemble.Controls.Add(this._tbSaveToEnsembleFilename);
            this._tpEnsemble.Controls.Add(this._lbFilename);
            this._tpEnsemble.Location = new System.Drawing.Point(4, 22);
            this._tpEnsemble.Name = "_tpEnsemble";
            this._tpEnsemble.Padding = new System.Windows.Forms.Padding(3);
            this._tpEnsemble.Size = new System.Drawing.Size(559, 451);
            this._tpEnsemble.TabIndex = 0;
            this._tpEnsemble.Text = "Ensemble";
            // 
            // _tbSelectedNetworks
            // 
            this._tbSelectedNetworks.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._tbSelectedNetworks.Location = new System.Drawing.Point(10, 411);
            this._tbSelectedNetworks.Name = "_tbSelectedNetworks";
            this._tbSelectedNetworks.ReadOnly = true;
            this._tbSelectedNetworks.Size = new System.Drawing.Size(100, 20);
            this._tbSelectedNetworks.TabIndex = 14;
            // 
            // _lbSelected
            // 
            this._lbSelected.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._lbSelected.AutoSize = true;
            this._lbSelected.Location = new System.Drawing.Point(7, 394);
            this._lbSelected.Name = "_lbSelected";
            this._lbSelected.Size = new System.Drawing.Size(137, 13);
            this._lbSelected.TabIndex = 13;
            this._lbSelected.Text = "Number Networks Selected";
            // 
            // _cmbConnectionStringEnsemble
            // 
            this._cmbConnectionStringEnsemble.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._cmbConnectionStringEnsemble.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbConnectionStringEnsemble.FormattingEnabled = true;
            this._cmbConnectionStringEnsemble.Location = new System.Drawing.Point(7, 148);
            this._cmbConnectionStringEnsemble.Name = "_cmbConnectionStringEnsemble";
            this._cmbConnectionStringEnsemble.Size = new System.Drawing.Size(542, 21);
            this._cmbConnectionStringEnsemble.TabIndex = 12;
            this._cmbConnectionStringEnsemble.SelectedIndexChanged += new System.EventHandler(this.ComboBox1SelectedIndexChanged);
            // 
            // _lbConnectionString
            // 
            this._lbConnectionString.AutoSize = true;
            this._lbConnectionString.Location = new System.Drawing.Point(7, 132);
            this._lbConnectionString.Name = "_lbConnectionString";
            this._lbConnectionString.Size = new System.Drawing.Size(309, 13);
            this._lbConnectionString.TabIndex = 11;
            this._lbConnectionString.Text = "Connection String [Training Database Should Be Supplied Here]";
            // 
            // _btnInsert
            // 
            this._btnInsert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._btnInsert.Location = new System.Drawing.Point(478, 393);
            this._btnInsert.Name = "_btnInsert";
            this._btnInsert.Size = new System.Drawing.Size(75, 23);
            this._btnInsert.TabIndex = 10;
            this._btnInsert.Text = "Insert";
            this._btnInsert.UseVisualStyleBackColor = true;
            this._btnInsert.Click += new System.EventHandler(this.BtnInsertClick);
            // 
            // _btnSaveEnsamble
            // 
            this._btnSaveEnsamble.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnSaveEnsamble.Location = new System.Drawing.Point(477, 54);
            this._btnSaveEnsamble.Name = "_btnSaveEnsamble";
            this._btnSaveEnsamble.Size = new System.Drawing.Size(75, 23);
            this._btnSaveEnsamble.TabIndex = 9;
            this._btnSaveEnsamble.Text = "Save";
            this._btnSaveEnsamble.UseVisualStyleBackColor = true;
            this._btnSaveEnsamble.Click += new System.EventHandler(this.BtnSaveClick);
            // 
            // _btnStart
            // 
            this._btnStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._btnStart.Location = new System.Drawing.Point(478, 421);
            this._btnStart.Name = "_btnStart";
            this._btnStart.Size = new System.Drawing.Size(75, 23);
            this._btnStart.TabIndex = 8;
            this._btnStart.Text = "Start";
            this._btnStart.UseVisualStyleBackColor = true;
            this._btnStart.Click += new System.EventHandler(this.BtnStartClick);
            // 
            // _lbNetworks
            // 
            this._lbNetworks.AutoSize = true;
            this._lbNetworks.Location = new System.Drawing.Point(4, 188);
            this._lbNetworks.Name = "_lbNetworks";
            this._lbNetworks.Size = new System.Drawing.Size(52, 13);
            this._lbNetworks.TabIndex = 7;
            this._lbNetworks.Text = "Networks";
            // 
            // _dgvNetworks
            // 
            this._dgvNetworks.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._dgvNetworks.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._dgvNetworks.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.PathToNetwork,
            this.SelectNework});
            this._dgvNetworks.Location = new System.Drawing.Point(7, 204);
            this._dgvNetworks.Name = "_dgvNetworks";
            this._dgvNetworks.ReadOnly = true;
            this._dgvNetworks.Size = new System.Drawing.Size(546, 183);
            this._dgvNetworks.TabIndex = 6;
            this._dgvNetworks.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DgvNetworksCellContentClick);
            // 
            // PathToNetwork
            // 
            this.PathToNetwork.HeaderText = "Path To Network";
            this.PathToNetwork.Name = "PathToNetwork";
            this.PathToNetwork.ReadOnly = true;
            this.PathToNetwork.Width = 400;
            // 
            // SelectNework
            // 
            this.SelectNework.HeaderText = "CheckedNetwork";
            this.SelectNework.Name = "SelectNework";
            this.SelectNework.ReadOnly = true;
            // 
            // _nudNumberOfHashesPerKeyNeuralHasher
            // 
            this._nudNumberOfHashesPerKeyNeuralHasher.Location = new System.Drawing.Point(7, 109);
            this._nudNumberOfHashesPerKeyNeuralHasher.Name = "_nudNumberOfHashesPerKeyNeuralHasher";
            this._nudNumberOfHashesPerKeyNeuralHasher.Size = new System.Drawing.Size(120, 20);
            this._nudNumberOfHashesPerKeyNeuralHasher.TabIndex = 5;
            this._nudNumberOfHashesPerKeyNeuralHasher.ValueChanged += new System.EventHandler(this.NudNumberOfHashesPerKeyValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 93);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(137, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Number Of Hashes Per Key";
            // 
            // _nudNumberOfGroupsNeuralHasher
            // 
            this._nudNumberOfGroupsNeuralHasher.Location = new System.Drawing.Point(7, 70);
            this._nudNumberOfGroupsNeuralHasher.Name = "_nudNumberOfGroupsNeuralHasher";
            this._nudNumberOfGroupsNeuralHasher.Size = new System.Drawing.Size(120, 20);
            this._nudNumberOfGroupsNeuralHasher.TabIndex = 3;
            this._nudNumberOfGroupsNeuralHasher.ValueChanged += new System.EventHandler(this.NudNumberOfGroupsValueChanged);
            // 
            // _lbGroupNumbers
            // 
            this._lbGroupNumbers.AutoSize = true;
            this._lbGroupNumbers.Location = new System.Drawing.Point(7, 54);
            this._lbGroupNumbers.Name = "_lbGroupNumbers";
            this._lbGroupNumbers.Size = new System.Drawing.Size(95, 13);
            this._lbGroupNumbers.TabIndex = 2;
            this._lbGroupNumbers.Text = "Number Of Groups";
            // 
            // _tbSaveToEnsembleFilename
            // 
            this._tbSaveToEnsembleFilename.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._tbSaveToEnsembleFilename.Location = new System.Drawing.Point(6, 28);
            this._tbSaveToEnsembleFilename.Name = "_tbSaveToEnsembleFilename";
            this._tbSaveToEnsembleFilename.Size = new System.Drawing.Size(546, 20);
            this._tbSaveToEnsembleFilename.TabIndex = 1;
            // 
            // _lbFilename
            // 
            this._lbFilename.AutoSize = true;
            this._lbFilename.Location = new System.Drawing.Point(4, 12);
            this._lbFilename.Name = "_lbFilename";
            this._lbFilename.Size = new System.Drawing.Size(98, 13);
            this._lbFilename.TabIndex = 0;
            this._lbFilename.Text = "Ensemble Filename";
            // 
            // _tpHashBins
            // 
            this._tpHashBins.BackColor = System.Drawing.SystemColors.Control;
            this._tpHashBins.Controls.Add(this._lbInsert);
            this._tpHashBins.Controls.Add(this._pbProgress);
            this._tpHashBins.Controls.Add(this._btnSelectStoredEnsemble);
            this._tpHashBins.Controls.Add(this._tbStoredEnsembleFilename);
            this._tpHashBins.Controls.Add(this.label2);
            this._tpHashBins.Controls.Add(this._bntComputeHashesNeuralHasher);
            this._tpHashBins.Controls.Add(this._lbConnectionStringValidation);
            this._tpHashBins.Controls.Add(this._cmbValidationConnectionStringNeuralHasher);
            this._tpHashBins.Location = new System.Drawing.Point(4, 22);
            this._tpHashBins.Name = "_tpHashBins";
            this._tpHashBins.Padding = new System.Windows.Forms.Padding(3);
            this._tpHashBins.Size = new System.Drawing.Size(556, 454);
            this._tpHashBins.TabIndex = 1;
            this._tpHashBins.Text = "Neural hasher";
            // 
            // _lbInsert
            // 
            this._lbInsert.AutoSize = true;
            this._lbInsert.Location = new System.Drawing.Point(9, 7);
            this._lbInsert.Name = "_lbInsert";
            this._lbInsert.Size = new System.Drawing.Size(233, 13);
            this._lbInsert.TabIndex = 18;
            this._lbInsert.Text = "Insert HashBins from the song into the database";
            // 
            // _pbProgress
            // 
            this._pbProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._pbProgress.Location = new System.Drawing.Point(12, 167);
            this._pbProgress.Name = "_pbProgress";
            this._pbProgress.Size = new System.Drawing.Size(529, 23);
            this._pbProgress.TabIndex = 17;
            // 
            // _btnSelectStoredEnsemble
            // 
            this._btnSelectStoredEnsemble.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._btnSelectStoredEnsemble.Location = new System.Drawing.Point(466, 69);
            this._btnSelectStoredEnsemble.Name = "_btnSelectStoredEnsemble";
            this._btnSelectStoredEnsemble.Size = new System.Drawing.Size(75, 23);
            this._btnSelectStoredEnsemble.TabIndex = 16;
            this._btnSelectStoredEnsemble.Text = "Select";
            this._btnSelectStoredEnsemble.UseVisualStyleBackColor = true;
            this._btnSelectStoredEnsemble.Click += new System.EventHandler(this.BtnSelectClick);
            // 
            // _tbStoredEnsembleFilename
            // 
            this._tbStoredEnsembleFilename.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._tbStoredEnsembleFilename.Location = new System.Drawing.Point(12, 43);
            this._tbStoredEnsembleFilename.Name = "_tbStoredEnsembleFilename";
            this._tbStoredEnsembleFilename.Size = new System.Drawing.Size(529, 20);
            this._tbStoredEnsembleFilename.TabIndex = 15;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(135, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "Path to serialized ensemble";
            // 
            // _bntComputeHashesNeuralHasher
            // 
            this._bntComputeHashesNeuralHasher.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._bntComputeHashesNeuralHasher.Location = new System.Drawing.Point(466, 138);
            this._bntComputeHashesNeuralHasher.Name = "_bntComputeHashesNeuralHasher";
            this._bntComputeHashesNeuralHasher.Size = new System.Drawing.Size(75, 23);
            this._bntComputeHashesNeuralHasher.TabIndex = 13;
            this._bntComputeHashesNeuralHasher.Text = "Start";
            this._bntComputeHashesNeuralHasher.UseVisualStyleBackColor = true;
            this._bntComputeHashesNeuralHasher.Click += new System.EventHandler(this.BntComputeHashesClick);
            // 
            // _lbConnectionStringValidation
            // 
            this._lbConnectionStringValidation.AutoSize = true;
            this._lbConnectionStringValidation.Location = new System.Drawing.Point(6, 95);
            this._lbConnectionStringValidation.Name = "_lbConnectionStringValidation";
            this._lbConnectionStringValidation.Size = new System.Drawing.Size(317, 13);
            this._lbConnectionStringValidation.TabIndex = 12;
            this._lbConnectionStringValidation.Text = "Connection String [Validation Database Should Be Supplied Here]";
            // 
            // _cmbValidationConnectionStringNeuralHasher
            // 
            this._cmbValidationConnectionStringNeuralHasher.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._cmbValidationConnectionStringNeuralHasher.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbValidationConnectionStringNeuralHasher.FormattingEnabled = true;
            this._cmbValidationConnectionStringNeuralHasher.Location = new System.Drawing.Point(9, 111);
            this._cmbValidationConnectionStringNeuralHasher.Name = "_cmbValidationConnectionStringNeuralHasher";
            this._cmbValidationConnectionStringNeuralHasher.Size = new System.Drawing.Size(532, 21);
            this._cmbValidationConnectionStringNeuralHasher.TabIndex = 0;
            // 
            // _tbMinHash
            // 
            this._tbMinHash.BackColor = System.Drawing.SystemColors.Control;
            this._tbMinHash.Controls.Add(this._gbSpread);
            this._tbMinHash.Controls.Add(this._gbMain);
            this._tbMinHash.Location = new System.Drawing.Point(4, 22);
            this._tbMinHash.Name = "_tbMinHash";
            this._tbMinHash.Size = new System.Drawing.Size(556, 454);
            this._tbMinHash.TabIndex = 2;
            this._tbMinHash.Text = "Min Hash Algorithm";
            // 
            // _gbSpread
            // 
            this._gbSpread.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._gbSpread.Controls.Add(this._pbSpread);
            this._gbSpread.Controls.Add(this._btnSpread);
            this._gbSpread.Location = new System.Drawing.Point(3, 220);
            this._gbSpread.Name = "_gbSpread";
            this._gbSpread.Size = new System.Drawing.Size(544, 229);
            this._gbSpread.TabIndex = 23;
            this._gbSpread.TabStop = false;
            this._gbSpread.Text = "Update hashbuckets count";
            // 
            // _pbSpread
            // 
            this._pbSpread.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._pbSpread.Location = new System.Drawing.Point(12, 19);
            this._pbSpread.Name = "_pbSpread";
            this._pbSpread.Size = new System.Drawing.Size(532, 23);
            this._pbSpread.TabIndex = 2;
            this._pbSpread.Visible = false;
            // 
            // _btnSpread
            // 
            this._btnSpread.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._btnSpread.Location = new System.Drawing.Point(406, 48);
            this._btnSpread.Name = "_btnSpread";
            this._btnSpread.Size = new System.Drawing.Size(138, 23);
            this._btnSpread.TabIndex = 1;
            this._btnSpread.Text = "Update count";
            this._btnSpread.UseVisualStyleBackColor = true;
            
            // 
            // _gbMain
            // 
            this._gbMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._gbMain.Controls.Add(this._nudNumberOfHashesPerKeyMinHash);
            this._gbMain.Controls.Add(this._cmbConnectionStringMinHash);
            this._gbMain.Controls.Add(this.label4);
            this._gbMain.Controls.Add(this._pbMinHash);
            this._gbMain.Controls.Add(this._nudNumberOfGroupsMinHash);
            this._gbMain.Controls.Add(this.label5);
            this._gbMain.Controls.Add(this.label3);
            this._gbMain.Controls.Add(this._btnStartMinHash);
            this._gbMain.Location = new System.Drawing.Point(3, 3);
            this._gbMain.Name = "_gbMain";
            this._gbMain.Size = new System.Drawing.Size(550, 211);
            this._gbMain.TabIndex = 22;
            this._gbMain.TabStop = false;
            this._gbMain.Text = "Insert Hashes into the database";
            // 
            // _nudNumberOfHashesPerKeyMinHash
            // 
            this._nudNumberOfHashesPerKeyMinHash.Location = new System.Drawing.Point(15, 127);
            this._nudNumberOfHashesPerKeyMinHash.Name = "_nudNumberOfHashesPerKeyMinHash";
            this._nudNumberOfHashesPerKeyMinHash.Size = new System.Drawing.Size(120, 20);
            this._nudNumberOfHashesPerKeyMinHash.TabIndex = 26;
            // 
            // _cmbConnectionStringMinHash
            // 
            this._cmbConnectionStringMinHash.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._cmbConnectionStringMinHash.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbConnectionStringMinHash.FormattingEnabled = true;
            this._cmbConnectionStringMinHash.Location = new System.Drawing.Point(12, 45);
            this._cmbConnectionStringMinHash.Name = "_cmbConnectionStringMinHash";
            this._cmbConnectionStringMinHash.Size = new System.Drawing.Size(532, 21);
            this._cmbConnectionStringMinHash.TabIndex = 18;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 111);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(137, 13);
            this.label4.TabIndex = 25;
            this.label4.Text = "Number Of Hashes Per Key";
            // 
            // _pbMinHash
            // 
            this._pbMinHash.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._pbMinHash.Location = new System.Drawing.Point(12, 153);
            this._pbMinHash.Name = "_pbMinHash";
            this._pbMinHash.Size = new System.Drawing.Size(532, 23);
            this._pbMinHash.TabIndex = 21;
            // 
            // _nudNumberOfGroupsMinHash
            // 
            this._nudNumberOfGroupsMinHash.Location = new System.Drawing.Point(15, 88);
            this._nudNumberOfGroupsMinHash.Name = "_nudNumberOfGroupsMinHash";
            this._nudNumberOfGroupsMinHash.Size = new System.Drawing.Size(120, 20);
            this._nudNumberOfGroupsMinHash.TabIndex = 24;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(15, 72);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(95, 13);
            this.label5.TabIndex = 23;
            this.label5.Text = "Number Of Groups";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 29);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(363, 13);
            this.label3.TabIndex = 19;
            this.label3.Text = "Connection String [Validation data: from this database songs will be hashed]";
            // 
            // _btnStartMinHash
            // 
            this._btnStartMinHash.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._btnStartMinHash.Location = new System.Drawing.Point(469, 182);
            this._btnStartMinHash.Name = "_btnStartMinHash";
            this._btnStartMinHash.Size = new System.Drawing.Size(75, 23);
            this._btnStartMinHash.TabIndex = 20;
            this._btnStartMinHash.Text = "Start";
            this._btnStartMinHash.UseVisualStyleBackColor = true;
            this._btnStartMinHash.Click += new System.EventHandler(this.BtnStartMinHashClick);
            // 
            // _ofdSelectEnsemble
            // 
            this._ofdSelectEnsemble.FileName = "openFileDialog1";
            // 
            // WinEnsembleHash
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 492);
            this.Controls.Add(this._tbActionControl);
            this.Name = "WinEnsembleHash";
            this.Text = "Utility For Ensembling";
            this.Load += new System.EventHandler(this.WinMinMutualInfoLoad);
            this._tbActionControl.ResumeLayout(false);
            this._tpEnsemble.ResumeLayout(false);
            this._tpEnsemble.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._dgvNetworks)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudNumberOfHashesPerKeyNeuralHasher)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudNumberOfGroupsNeuralHasher)).EndInit();
            this._tpHashBins.ResumeLayout(false);
            this._tpHashBins.PerformLayout();
            this._tbMinHash.ResumeLayout(false);
            this._gbSpread.ResumeLayout(false);
            this._gbMain.ResumeLayout(false);
            this._gbMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._nudNumberOfHashesPerKeyMinHash)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudNumberOfGroupsMinHash)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl _tbActionControl;
        private System.Windows.Forms.TabPage _tpEnsemble;
        private System.Windows.Forms.TabPage _tpHashBins;
        private System.Windows.Forms.NumericUpDown _nudNumberOfHashesPerKeyNeuralHasher;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown _nudNumberOfGroupsNeuralHasher;
        private System.Windows.Forms.Label _lbGroupNumbers;
        private System.Windows.Forms.TextBox _tbSaveToEnsembleFilename;
        private System.Windows.Forms.Label _lbFilename;
        private System.Windows.Forms.Button _btnSaveEnsamble;
        private System.Windows.Forms.Button _btnStart;
        private System.Windows.Forms.Label _lbNetworks;
        private System.Windows.Forms.DataGridView _dgvNetworks;
        private System.Windows.Forms.Button _btnInsert;
        private System.Windows.Forms.SaveFileDialog _sfdEnsembleSave;
        private System.Windows.Forms.FolderBrowserDialog _fbdSelectNetworks;
        private System.Windows.Forms.ComboBox _cmbConnectionStringEnsemble;
        private System.Windows.Forms.Label _lbConnectionString;
        private System.Windows.Forms.Label _lbSelected;
        private System.Windows.Forms.TextBox _tbSelectedNetworks;
        private System.Windows.Forms.DataGridViewTextBoxColumn PathToNetwork;
        private System.Windows.Forms.DataGridViewCheckBoxColumn SelectNework;
        private System.Windows.Forms.Label _lbConnectionStringValidation;
        private System.Windows.Forms.ComboBox _cmbValidationConnectionStringNeuralHasher;
        private System.Windows.Forms.Button _bntComputeHashesNeuralHasher;
        private System.Windows.Forms.Button _btnSelectStoredEnsemble;
        private System.Windows.Forms.TextBox _tbStoredEnsembleFilename;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ProgressBar _pbProgress;
        private System.Windows.Forms.OpenFileDialog _ofdSelectEnsemble;
        private System.Windows.Forms.TabPage _tbMinHash;
        private System.Windows.Forms.Label _lbInsert;
        private System.Windows.Forms.GroupBox _gbMain;
        private System.Windows.Forms.ComboBox _cmbConnectionStringMinHash;
        private System.Windows.Forms.ProgressBar _pbMinHash;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button _btnStartMinHash;
        private System.Windows.Forms.NumericUpDown _nudNumberOfHashesPerKeyMinHash;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown _nudNumberOfGroupsMinHash;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox _gbSpread;
        private System.Windows.Forms.ProgressBar _pbSpread;
        private System.Windows.Forms.Button _btnSpread;
    }
}