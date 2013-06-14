namespace SoundFingerprinting.SoundTools.DbFiller
{
    partial class WinDbFiller
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
            this._dgvFillDatabase = new System.Windows.Forms.DataGridView();
            this.Artist = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Title = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Album = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Length = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Fingerprints = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._labChooseConnectionString = new System.Windows.Forms.Label();
            this._cmbDBFillerConnectionString = new System.Windows.Forms.ComboBox();
            this._labSelectRootFolder = new System.Windows.Forms.Label();
            this._tbRootFolder = new System.Windows.Forms.TextBox();
            this._labSelectFile = new System.Windows.Forms.Label();
            this._tbSingleFile = new System.Windows.Forms.TextBox();
            this._labSelectType = new System.Windows.Forms.Label();
            this._lbAlgorithm = new System.Windows.Forms.ListBox();
            this._nudStride = new System.Windows.Forms.NumericUpDown();
            this._nudHashTables = new System.Windows.Forms.NumericUpDown();
            this._labHashTables = new System.Windows.Forms.Label();
            this._labHashKeys = new System.Windows.Forms.Label();
            this._nudHashKeys = new System.Windows.Forms.NumericUpDown();
            this._labStride = new System.Windows.Forms.Label();
            this._pbTotalSongs = new System.Windows.Forms.ProgressBar();
            this._btnStart = new System.Windows.Forms.Button();
            this._btnStop = new System.Windows.Forms.Button();
            this._labStart = new System.Windows.Forms.Label();
            this._labTotalSongs = new System.Windows.Forms.Label();
            this._nudTotalSongs = new System.Windows.Forms.NumericUpDown();
            this._labDuplicates = new System.Windows.Forms.Label();
            this._nudDetectedDuplicates = new System.Windows.Forms.NumericUpDown();
            this._gbStatistics = new System.Windows.Forms.GroupBox();
            this._btnExport = new System.Windows.Forms.Button();
            this._nudLeft = new System.Windows.Forms.NumericUpDown();
            this._labLeft = new System.Windows.Forms.Label();
            this._nudProcessed = new System.Windows.Forms.NumericUpDown();
            this._labProcessed = new System.Windows.Forms.Label();
            this._nudBadFiles = new System.Windows.Forms.NumericUpDown();
            this._labBadFiles = new System.Windows.Forms.Label();
            this._nudThreads = new System.Windows.Forms.NumericUpDown();
            this._labThreads = new System.Windows.Forms.Label();
            this._gbMinHash = new System.Windows.Forms.GroupBox();
            this._gbHasher = new System.Windows.Forms.GroupBox();
            this._btnBrowse = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this._tbPathToEnsemble = new System.Windows.Forms.TextBox();
            this._nudTopWav = new System.Windows.Forms.NumericUpDown();
            this._labTopWavelets = new System.Windows.Forms.Label();
            this._cmbStrideType = new System.Windows.Forms.ComboBox();
            this._labStrideType = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this._dgvFillDatabase)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudStride)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudHashTables)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudHashKeys)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudTotalSongs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudDetectedDuplicates)).BeginInit();
            this._gbStatistics.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._nudLeft)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudProcessed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudBadFiles)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudThreads)).BeginInit();
            this._gbMinHash.SuspendLayout();
            this._gbHasher.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._nudTopWav)).BeginInit();
            this.SuspendLayout();
            // 
            // _dgvFillDatabase
            // 
            this._dgvFillDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._dgvFillDatabase.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._dgvFillDatabase.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Artist,
            this.Title,
            this.Album,
            this.Length,
            this.Fingerprints});
            this._dgvFillDatabase.Location = new System.Drawing.Point(12, 389);
            this._dgvFillDatabase.Name = "_dgvFillDatabase";
            this._dgvFillDatabase.ReadOnly = true;
            this._dgvFillDatabase.Size = new System.Drawing.Size(680, 201);
            this._dgvFillDatabase.TabIndex = 0;
            // 
            // Artist
            // 
            this.Artist.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Artist.HeaderText = "Artist";
            this.Artist.Name = "Artist";
            this.Artist.ReadOnly = true;
            // 
            // Title
            // 
            this.Title.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Title.FillWeight = 200F;
            this.Title.HeaderText = "Title";
            this.Title.Name = "Title";
            this.Title.ReadOnly = true;
            // 
            // Album
            // 
            this.Album.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Album.HeaderText = "Album";
            this.Album.Name = "Album";
            this.Album.ReadOnly = true;
            // 
            // Length
            // 
            this.Length.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Length.FillWeight = 50F;
            this.Length.HeaderText = "Length";
            this.Length.Name = "Length";
            this.Length.ReadOnly = true;
            // 
            // Fingerprints
            // 
            this.Fingerprints.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Fingerprints.FillWeight = 50F;
            this.Fingerprints.HeaderText = "Fingerprints";
            this.Fingerprints.Name = "Fingerprints";
            this.Fingerprints.ReadOnly = true;
            // 
            // _labChooseConnectionString
            // 
            this._labChooseConnectionString.AutoSize = true;
            this._labChooseConnectionString.Location = new System.Drawing.Point(19, 9);
            this._labChooseConnectionString.Name = "_labChooseConnectionString";
            this._labChooseConnectionString.Size = new System.Drawing.Size(256, 13);
            this._labChooseConnectionString.TabIndex = 1;
            this._labChooseConnectionString.Text = "1. Choose connection string (specified in App.Config)";
            // 
            // _cmbDBFillerConnectionString
            // 
            this._cmbDBFillerConnectionString.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._cmbDBFillerConnectionString.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbDBFillerConnectionString.FormattingEnabled = true;
            this._cmbDBFillerConnectionString.Location = new System.Drawing.Point(18, 25);
            this._cmbDBFillerConnectionString.Name = "_cmbDBFillerConnectionString";
            this._cmbDBFillerConnectionString.Size = new System.Drawing.Size(677, 21);
            this._cmbDBFillerConnectionString.TabIndex = 2;
            // 
            // _labSelectRootFolder
            // 
            this._labSelectRootFolder.AutoSize = true;
            this._labSelectRootFolder.Location = new System.Drawing.Point(18, 54);
            this._labSelectRootFolder.Name = "_labSelectRootFolder";
            this._labSelectRootFolder.Size = new System.Drawing.Size(279, 13);
            this._labSelectRootFolder.TabIndex = 3;
            this._labSelectRootFolder.Text = "2. Select root folder for audio files (double click to browse)";
            // 
            // _tbRootFolder
            // 
            this._tbRootFolder.Location = new System.Drawing.Point(18, 70);
            this._tbRootFolder.Name = "_tbRootFolder";
            this._tbRootFolder.Size = new System.Drawing.Size(330, 20);
            this._tbRootFolder.TabIndex = 4;
            this._tbRootFolder.TextChanged += new System.EventHandler(this.RootFolderIsSelected);
            this._tbRootFolder.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.TbRootFolderMouseDoubleClick);
            // 
            // _labSelectFile
            // 
            this._labSelectFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._labSelectFile.AutoSize = true;
            this._labSelectFile.Location = new System.Drawing.Point(359, 54);
            this._labSelectFile.Name = "_labSelectFile";
            this._labSelectFile.Size = new System.Drawing.Size(203, 13);
            this._labSelectFile.TabIndex = 5;
            this._labSelectFile.Text = "Select Single File (double click to browse)";
            // 
            // _tbSingleFile
            // 
            this._tbSingleFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._tbSingleFile.Location = new System.Drawing.Point(362, 70);
            this._tbSingleFile.Name = "_tbSingleFile";
            this._tbSingleFile.Size = new System.Drawing.Size(330, 20);
            this._tbSingleFile.TabIndex = 6;
            this._tbSingleFile.TextChanged += new System.EventHandler(this.TbSingleFileTextChanged);
            this._tbSingleFile.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.TbSingleFileMouseDoubleClick);
            // 
            // _labSelectType
            // 
            this._labSelectType.AutoSize = true;
            this._labSelectType.Location = new System.Drawing.Point(18, 97);
            this._labSelectType.Name = "_labSelectType";
            this._labSelectType.Size = new System.Drawing.Size(199, 13);
            this._labSelectType.TabIndex = 7;
            this._labSelectType.Text = "3. Select Algorithm for fingerprint hashing";
            // 
            // _lbAlgorithm
            // 
            this._lbAlgorithm.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lbAlgorithm.FormattingEnabled = true;
            this._lbAlgorithm.Items.AddRange(new object[] {
            "LSH+MinHash",
            "Neural Hasher",
            "Do not hash!"});
            this._lbAlgorithm.Location = new System.Drawing.Point(18, 113);
            this._lbAlgorithm.Name = "_lbAlgorithm";
            this._lbAlgorithm.Size = new System.Drawing.Size(148, 43);
            this._lbAlgorithm.TabIndex = 8;
            this._lbAlgorithm.SelectedIndexChanged += new System.EventHandler(this.LbAlgorithmSelectedIndexChanged);
            // 
            // _nudStride
            // 
            this._nudStride.Location = new System.Drawing.Point(18, 220);
            this._nudStride.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this._nudStride.Name = "_nudStride";
            this._nudStride.Size = new System.Drawing.Size(145, 20);
            this._nudStride.TabIndex = 10;
            // 
            // _nudHashTables
            // 
            this._nudHashTables.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._nudHashTables.Location = new System.Drawing.Point(9, 34);
            this._nudHashTables.Name = "_nudHashTables";
            this._nudHashTables.Size = new System.Drawing.Size(120, 20);
            this._nudHashTables.TabIndex = 11;
            this._nudHashTables.Value = new decimal(new int[] {
            25,
            0,
            0,
            0});
            // 
            // _labHashTables
            // 
            this._labHashTables.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._labHashTables.AutoSize = true;
            this._labHashTables.Location = new System.Drawing.Point(6, 17);
            this._labHashTables.Name = "_labHashTables";
            this._labHashTables.Size = new System.Drawing.Size(64, 13);
            this._labHashTables.TabIndex = 12;
            this._labHashTables.Text = "HashTables";
            // 
            // _labHashKeys
            // 
            this._labHashKeys.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._labHashKeys.AutoSize = true;
            this._labHashKeys.Location = new System.Drawing.Point(6, 58);
            this._labHashKeys.Name = "_labHashKeys";
            this._labHashKeys.Size = new System.Drawing.Size(58, 13);
            this._labHashKeys.TabIndex = 13;
            this._labHashKeys.Text = "Hash Keys";
            // 
            // _nudHashKeys
            // 
            this._nudHashKeys.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._nudHashKeys.Location = new System.Drawing.Point(9, 75);
            this._nudHashKeys.Name = "_nudHashKeys";
            this._nudHashKeys.Size = new System.Drawing.Size(120, 20);
            this._nudHashKeys.TabIndex = 14;
            this._nudHashKeys.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            // 
            // _labStride
            // 
            this._labStride.AutoSize = true;
            this._labStride.Location = new System.Drawing.Point(19, 204);
            this._labStride.Name = "_labStride";
            this._labStride.Size = new System.Drawing.Size(147, 13);
            this._labStride.TabIndex = 15;
            this._labStride.Text = "4. Select DB stride in samples";
            // 
            // _pbTotalSongs
            // 
            this._pbTotalSongs.Location = new System.Drawing.Point(9, 107);
            this._pbTotalSongs.Name = "_pbTotalSongs";
            this._pbTotalSongs.Size = new System.Drawing.Size(374, 23);
            this._pbTotalSongs.TabIndex = 16;
            // 
            // _btnStart
            // 
            this._btnStart.Location = new System.Drawing.Point(18, 360);
            this._btnStart.Name = "_btnStart";
            this._btnStart.Size = new System.Drawing.Size(75, 23);
            this._btnStart.TabIndex = 17;
            this._btnStart.Text = "Start";
            this._btnStart.UseVisualStyleBackColor = true;
            this._btnStart.Click += new System.EventHandler(this.BtnStartClick);
            // 
            // _btnStop
            // 
            this._btnStop.Location = new System.Drawing.Point(99, 360);
            this._btnStop.Name = "_btnStop";
            this._btnStop.Size = new System.Drawing.Size(75, 23);
            this._btnStop.TabIndex = 18;
            this._btnStop.Text = "Stop";
            this._btnStop.UseVisualStyleBackColor = true;
            this._btnStop.Click += new System.EventHandler(this.BtnStopClick);
            // 
            // _labStart
            // 
            this._labStart.AutoSize = true;
            this._labStart.Location = new System.Drawing.Point(19, 344);
            this._labStart.Name = "_labStart";
            this._labStart.Size = new System.Drawing.Size(131, 13);
            this._labStart.TabIndex = 20;
            this._labStart.Text = "5. Start fingerprint creation";
            // 
            // _labTotalSongs
            // 
            this._labTotalSongs.AutoSize = true;
            this._labTotalSongs.Location = new System.Drawing.Point(44, 15);
            this._labTotalSongs.Name = "_labTotalSongs";
            this._labTotalSongs.Size = new System.Drawing.Size(64, 13);
            this._labTotalSongs.TabIndex = 22;
            this._labTotalSongs.Text = "Total Songs";
            // 
            // _nudTotalSongs
            // 
            this._nudTotalSongs.Enabled = false;
            this._nudTotalSongs.Location = new System.Drawing.Point(140, 13);
            this._nudTotalSongs.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this._nudTotalSongs.Name = "_nudTotalSongs";
            this._nudTotalSongs.ReadOnly = true;
            this._nudTotalSongs.Size = new System.Drawing.Size(120, 20);
            this._nudTotalSongs.TabIndex = 23;
            // 
            // _labDuplicates
            // 
            this._labDuplicates.AutoSize = true;
            this._labDuplicates.Location = new System.Drawing.Point(6, 39);
            this._labDuplicates.Name = "_labDuplicates";
            this._labDuplicates.Size = new System.Drawing.Size(102, 13);
            this._labDuplicates.TabIndex = 24;
            this._labDuplicates.Text = "Detected duplicates";
            // 
            // _nudDetectedDuplicates
            // 
            this._nudDetectedDuplicates.Enabled = false;
            this._nudDetectedDuplicates.Location = new System.Drawing.Point(140, 37);
            this._nudDetectedDuplicates.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this._nudDetectedDuplicates.Name = "_nudDetectedDuplicates";
            this._nudDetectedDuplicates.ReadOnly = true;
            this._nudDetectedDuplicates.Size = new System.Drawing.Size(120, 20);
            this._nudDetectedDuplicates.TabIndex = 25;
            // 
            // _gbStatistics
            // 
            this._gbStatistics.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._gbStatistics.Controls.Add(this._btnExport);
            this._gbStatistics.Controls.Add(this._nudLeft);
            this._gbStatistics.Controls.Add(this._labLeft);
            this._gbStatistics.Controls.Add(this._nudProcessed);
            this._gbStatistics.Controls.Add(this._labProcessed);
            this._gbStatistics.Controls.Add(this._nudBadFiles);
            this._gbStatistics.Controls.Add(this._labBadFiles);
            this._gbStatistics.Controls.Add(this._nudThreads);
            this._gbStatistics.Controls.Add(this._labThreads);
            this._gbStatistics.Controls.Add(this._labTotalSongs);
            this._gbStatistics.Controls.Add(this._nudDetectedDuplicates);
            this._gbStatistics.Controls.Add(this._nudTotalSongs);
            this._gbStatistics.Controls.Add(this._labDuplicates);
            this._gbStatistics.Controls.Add(this._pbTotalSongs);
            this._gbStatistics.Location = new System.Drawing.Point(222, 204);
            this._gbStatistics.Name = "_gbStatistics";
            this._gbStatistics.Size = new System.Drawing.Size(470, 136);
            this._gbStatistics.TabIndex = 26;
            this._gbStatistics.TabStop = false;
            this._gbStatistics.Text = "Stats";
            // 
            // _btnExport
            // 
            this._btnExport.Location = new System.Drawing.Point(389, 107);
            this._btnExport.Name = "_btnExport";
            this._btnExport.Size = new System.Drawing.Size(75, 23);
            this._btnExport.TabIndex = 34;
            this._btnExport.Text = "Export Stats";
            this._btnExport.UseVisualStyleBackColor = true;
            this._btnExport.Click += new System.EventHandler(this.BtnExportClick);
            // 
            // _nudLeft
            // 
            this._nudLeft.Enabled = false;
            this._nudLeft.Location = new System.Drawing.Point(344, 61);
            this._nudLeft.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this._nudLeft.Name = "_nudLeft";
            this._nudLeft.ReadOnly = true;
            this._nudLeft.Size = new System.Drawing.Size(120, 20);
            this._nudLeft.TabIndex = 33;
            // 
            // _labLeft
            // 
            this._labLeft.AutoSize = true;
            this._labLeft.Location = new System.Drawing.Point(313, 62);
            this._labLeft.Name = "_labLeft";
            this._labLeft.Size = new System.Drawing.Size(25, 13);
            this._labLeft.TabIndex = 32;
            this._labLeft.Text = "Left";
            // 
            // _nudProcessed
            // 
            this._nudProcessed.Enabled = false;
            this._nudProcessed.Location = new System.Drawing.Point(344, 37);
            this._nudProcessed.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this._nudProcessed.Name = "_nudProcessed";
            this._nudProcessed.ReadOnly = true;
            this._nudProcessed.Size = new System.Drawing.Size(120, 20);
            this._nudProcessed.TabIndex = 31;
            // 
            // _labProcessed
            // 
            this._labProcessed.AutoSize = true;
            this._labProcessed.Location = new System.Drawing.Point(281, 39);
            this._labProcessed.Name = "_labProcessed";
            this._labProcessed.Size = new System.Drawing.Size(57, 13);
            this._labProcessed.TabIndex = 30;
            this._labProcessed.Text = "Processed";
            // 
            // _nudBadFiles
            // 
            this._nudBadFiles.Enabled = false;
            this._nudBadFiles.Location = new System.Drawing.Point(140, 61);
            this._nudBadFiles.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this._nudBadFiles.Name = "_nudBadFiles";
            this._nudBadFiles.ReadOnly = true;
            this._nudBadFiles.Size = new System.Drawing.Size(120, 20);
            this._nudBadFiles.TabIndex = 29;
            // 
            // _labBadFiles
            // 
            this._labBadFiles.AutoSize = true;
            this._labBadFiles.Location = new System.Drawing.Point(58, 63);
            this._labBadFiles.Name = "_labBadFiles";
            this._labBadFiles.Size = new System.Drawing.Size(50, 13);
            this._labBadFiles.TabIndex = 28;
            this._labBadFiles.Text = "Bad Files";
            // 
            // _nudThreads
            // 
            this._nudThreads.Enabled = false;
            this._nudThreads.Location = new System.Drawing.Point(344, 13);
            this._nudThreads.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this._nudThreads.Name = "_nudThreads";
            this._nudThreads.ReadOnly = true;
            this._nudThreads.Size = new System.Drawing.Size(120, 20);
            this._nudThreads.TabIndex = 27;
            // 
            // _labThreads
            // 
            this._labThreads.AutoSize = true;
            this._labThreads.Location = new System.Drawing.Point(292, 16);
            this._labThreads.Name = "_labThreads";
            this._labThreads.Size = new System.Drawing.Size(46, 13);
            this._labThreads.TabIndex = 26;
            this._labThreads.Text = "MaxThreadToProcessFiles";
            // 
            // _gbMinHash
            // 
            this._gbMinHash.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._gbMinHash.Controls.Add(this._nudHashTables);
            this._gbMinHash.Controls.Add(this._labHashTables);
            this._gbMinHash.Controls.Add(this._labHashKeys);
            this._gbMinHash.Controls.Add(this._nudHashKeys);
            this._gbMinHash.Location = new System.Drawing.Point(222, 97);
            this._gbMinHash.Name = "_gbMinHash";
            this._gbMinHash.Size = new System.Drawing.Size(217, 101);
            this._gbMinHash.TabIndex = 27;
            this._gbMinHash.TabStop = false;
            this._gbMinHash.Text = "Min Hash settings";
            // 
            // _gbHasher
            // 
            this._gbHasher.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._gbHasher.Controls.Add(this._btnBrowse);
            this._gbHasher.Controls.Add(this.label1);
            this._gbHasher.Controls.Add(this._tbPathToEnsemble);
            this._gbHasher.Enabled = false;
            this._gbHasher.Location = new System.Drawing.Point(445, 97);
            this._gbHasher.Name = "_gbHasher";
            this._gbHasher.Size = new System.Drawing.Size(241, 100);
            this._gbHasher.TabIndex = 28;
            this._gbHasher.TabStop = false;
            this._gbHasher.Text = "Neural Hasher settings";
            // 
            // _btnBrowse
            // 
            this._btnBrowse.Location = new System.Drawing.Point(160, 58);
            this._btnBrowse.Name = "_btnBrowse";
            this._btnBrowse.Size = new System.Drawing.Size(75, 23);
            this._btnBrowse.TabIndex = 2;
            this._btnBrowse.Text = "Browse";
            this._btnBrowse.UseVisualStyleBackColor = true;
            this._btnBrowse.Click += new System.EventHandler(this.BtnBrowseClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Path to ensemble";
            // 
            // _tbPathToEnsemble
            // 
            this._tbPathToEnsemble.Location = new System.Drawing.Point(6, 34);
            this._tbPathToEnsemble.Name = "_tbPathToEnsemble";
            this._tbPathToEnsemble.ReadOnly = true;
            this._tbPathToEnsemble.Size = new System.Drawing.Size(229, 20);
            this._tbPathToEnsemble.TabIndex = 0;
            // 
            // _nudTopWav
            // 
            this._nudTopWav.Location = new System.Drawing.Point(18, 311);
            this._nudTopWav.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this._nudTopWav.Name = "_nudTopWav";
            this._nudTopWav.Size = new System.Drawing.Size(145, 20);
            this._nudTopWav.TabIndex = 35;
            this._nudTopWav.Value = new decimal(new int[] {
            200,
            0,
            0,
            0});
            // 
            // _labTopWavelets
            // 
            this._labTopWavelets.AutoSize = true;
            this._labTopWavelets.Location = new System.Drawing.Point(19, 295);
            this._labTopWavelets.Name = "_labTopWavelets";
            this._labTopWavelets.Size = new System.Drawing.Size(74, 13);
            this._labTopWavelets.TabIndex = 35;
            this._labTopWavelets.Text = "Top Wavelets";
            // 
            // _cmbStrideType
            // 
            this._cmbStrideType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbStrideType.FormattingEnabled = true;
            this._cmbStrideType.Location = new System.Drawing.Point(18, 265);
            this._cmbStrideType.Name = "_cmbStrideType";
            this._cmbStrideType.Size = new System.Drawing.Size(145, 21);
            this._cmbStrideType.TabIndex = 36;
            // 
            // _labStrideType
            // 
            this._labStrideType.AutoSize = true;
            this._labStrideType.Location = new System.Drawing.Point(18, 248);
            this._labStrideType.Name = "_labStrideType";
            this._labStrideType.Size = new System.Drawing.Size(57, 13);
            this._labStrideType.TabIndex = 37;
            this._labStrideType.Text = "Stride type";
            // 
            // WinDBFiller
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(704, 602);
            this.Controls.Add(this._labStrideType);
            this.Controls.Add(this._cmbStrideType);
            this.Controls.Add(this._labTopWavelets);
            this.Controls.Add(this._nudTopWav);
            this.Controls.Add(this._gbHasher);
            this.Controls.Add(this._gbMinHash);
            this.Controls.Add(this._gbStatistics);
            this.Controls.Add(this._labStart);
            this.Controls.Add(this._btnStop);
            this.Controls.Add(this._btnStart);
            this.Controls.Add(this._labStride);
            this.Controls.Add(this._nudStride);
            this.Controls.Add(this._lbAlgorithm);
            this.Controls.Add(this._labSelectType);
            this.Controls.Add(this._tbSingleFile);
            this.Controls.Add(this._labSelectFile);
            this.Controls.Add(this._tbRootFolder);
            this.Controls.Add(this._labSelectRootFolder);
            this.Controls.Add(this._cmbDBFillerConnectionString);
            this.Controls.Add(this._labChooseConnectionString);
            this.Controls.Add(this._dgvFillDatabase);
            this.Name = "WinDbFiller";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Fill database";
            ((System.ComponentModel.ISupportInitialize)(this._dgvFillDatabase)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudStride)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudHashTables)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudHashKeys)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudTotalSongs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudDetectedDuplicates)).EndInit();
            this._gbStatistics.ResumeLayout(false);
            this._gbStatistics.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._nudLeft)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudProcessed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudBadFiles)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudThreads)).EndInit();
            this._gbMinHash.ResumeLayout(false);
            this._gbMinHash.PerformLayout();
            this._gbHasher.ResumeLayout(false);
            this._gbHasher.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._nudTopWav)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView _dgvFillDatabase;
        private System.Windows.Forms.Label _labChooseConnectionString;
        private System.Windows.Forms.ComboBox _cmbDBFillerConnectionString;
        private System.Windows.Forms.Label _labSelectRootFolder;
        private System.Windows.Forms.TextBox _tbRootFolder;
        private System.Windows.Forms.Label _labSelectFile;
        private System.Windows.Forms.TextBox _tbSingleFile;
        private System.Windows.Forms.Label _labSelectType;
        private System.Windows.Forms.ListBox _lbAlgorithm;
        private System.Windows.Forms.NumericUpDown _nudStride;
        private System.Windows.Forms.NumericUpDown _nudHashTables;
        private System.Windows.Forms.Label _labHashTables;
        private System.Windows.Forms.Label _labHashKeys;
        private System.Windows.Forms.NumericUpDown _nudHashKeys;
        private System.Windows.Forms.Label _labStride;
        private System.Windows.Forms.ProgressBar _pbTotalSongs;
        private System.Windows.Forms.Button _btnStart;
        private System.Windows.Forms.Button _btnStop;
        private System.Windows.Forms.Label _labStart;
        private System.Windows.Forms.Label _labTotalSongs;
        private System.Windows.Forms.NumericUpDown _nudTotalSongs;
        private System.Windows.Forms.Label _labDuplicates;
        private System.Windows.Forms.NumericUpDown _nudDetectedDuplicates;
        private System.Windows.Forms.GroupBox _gbStatistics;
        private System.Windows.Forms.NumericUpDown _nudThreads;
        private System.Windows.Forms.Label _labThreads;
        private System.Windows.Forms.DataGridViewTextBoxColumn Artist;
        private System.Windows.Forms.DataGridViewTextBoxColumn Title;
        private System.Windows.Forms.DataGridViewTextBoxColumn Album;
        private System.Windows.Forms.DataGridViewTextBoxColumn Length;
        private System.Windows.Forms.DataGridViewTextBoxColumn Fingerprints;
        private System.Windows.Forms.NumericUpDown _nudBadFiles;
        private System.Windows.Forms.Label _labBadFiles;
        private System.Windows.Forms.NumericUpDown _nudLeft;
        private System.Windows.Forms.Label _labLeft;
        private System.Windows.Forms.NumericUpDown _nudProcessed;
        private System.Windows.Forms.Label _labProcessed;
        private System.Windows.Forms.Button _btnExport;
        private System.Windows.Forms.GroupBox _gbMinHash;
        private System.Windows.Forms.GroupBox _gbHasher;
        private System.Windows.Forms.Button _btnBrowse;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox _tbPathToEnsemble;
        private System.Windows.Forms.NumericUpDown _nudTopWav;
        private System.Windows.Forms.Label _labTopWavelets;
        private System.Windows.Forms.ComboBox _cmbStrideType;
        private System.Windows.Forms.Label _labStrideType;
    }
}