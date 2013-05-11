namespace Soundfingerprinting.SoundTools.QueryDb
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Security.Permissions;
    using System.Windows.Forms;

    using Soundfingerprinting.Audio.Services;
    using Soundfingerprinting.Audio.Strides;
    using Soundfingerprinting.Dao;
    using Soundfingerprinting.Fingerprinting.Configuration;
    using Soundfingerprinting.Query;
    using Soundfingerprinting.SoundTools.Properties;

    public partial class WinCheckHashBins : Form
    {
        private readonly IFingerprintQueryBuilder queryBuilder;

        private readonly ITagService tagService;

        private readonly IModelService modelService;

        private readonly IExtendedAudioService audioService;

        private readonly List<string> filters = new List<string>(new[] { "*.mp3", "*.wav", "*.ogg", "*.flac" }); // File filters

        private List<string> fileList = new List<string>();

        private HashAlgorithm hashAlgorithm = HashAlgorithm.LSH;

        public WinCheckHashBins(IFingerprintQueryBuilder queryBuilder, ITagService tagService, IModelService modelService, IExtendedAudioService audioService)
        {
            this.queryBuilder = queryBuilder;
            this.tagService = tagService;
            this.modelService = modelService;
            this.audioService = audioService;

            InitializeComponent();

            Icon = Resources.Sound;
            /*Detect all the connection strings*/
            foreach (object item in ConfigurationManager.ConnectionStrings) 
            {
                _cmbConnectionString.Items.Add(item.ToString());
            }

            /*Set connection string*/
            if (_cmbConnectionString.Items.Count > 0)
            {
                _cmbConnectionString.SelectedIndex = 0;
            }

            /*Setting default values*/
            _cmbAlgorithm.SelectedIndex = (int)hashAlgorithm;

            /*Add enumeration types in the combo box*/
            string[] items = Enum.GetNames(typeof(StrideType)); 

            _cmbStrideType.Items.AddRange(items);
            _cmbStrideType.SelectedIndex = 3;

            switch (_cmbAlgorithm.SelectedIndex)
            {
                case (int) HashAlgorithm.LSH:
                    _gbMinHash.Enabled = true;
                    _gbNeuralHasher.Enabled = false;
                    break;
                case (int) HashAlgorithm.NeuralHasher:
                    _gbMinHash.Enabled = false;
                    _gbNeuralHasher.Enabled = true;
                    break;
                case (int) HashAlgorithm.None:
                    _gbMinHash.Enabled = false;
                    _gbNeuralHasher.Enabled = false;
                    break;
            }

            _gbQueryMicrophoneBox.Enabled = audioService.IsRecordingSupported;
        }

        private void CmbAlgorithmSelectedIndexChanged(object sender, EventArgs e)
        {
            hashAlgorithm = (HashAlgorithm) _cmbAlgorithm.SelectedIndex;

            switch (_cmbAlgorithm.SelectedIndex)
            {
                case (int) HashAlgorithm.LSH: /*Locality sensitive hashing + min hash*/
                    _gbMinHash.Enabled = true;
                    _gbNeuralHasher.Enabled = false;
                    _nudQueryStrideMax.Value = 253;
                    break;
                case (int) HashAlgorithm.NeuralHasher: /*Neural hasher*/
                    _gbMinHash.Enabled = false;
                    _gbNeuralHasher.Enabled = true;
                    _nudQueryStrideMax.Value = 640;
                    break;
                case (int) HashAlgorithm.None: /*None*/
                    _gbMinHash.Enabled = false;
                    _gbNeuralHasher.Enabled = false;
                    break;
            }
        }

        private void BtnBrowseFolderClick(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                Cursor = Cursors.WaitCursor;
                _tbRootFolder.Enabled = false;
                _tbRootFolder.Text = fbd.SelectedPath;
                fileList = WinUtils.GetFiles(filters, _tbRootFolder.Text);

                Invoke(
                    new Action(
                        () =>
                            {
                                Cursor = Cursors.Default;
                                _tbRootFolder.Enabled = true;
                                _nudTotalSongs.Value = fileList.Count;
                                _btnStart.Enabled = true;
                                _tbSingleFile.Text = null;
                            }));
            }
        }

        /// <summary>
        ///   Browse single files to be recognized
        /// </summary>
        [FileDialogPermission(SecurityAction.Demand)]
        private void BtnBrowseSongClick(object sender, EventArgs e)
        {
            string filter = WinUtils.GetMultipleFilter("Audio files", filters);
            OpenFileDialog ofd = new OpenFileDialog {Filter = filter, Multiselect = true};

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _tbSingleFile.Text = null;
                foreach (string file in ofd.FileNames)
                    _tbSingleFile.Text += "\"" + Path.GetFileName(file) + "\" ";

                if (fileList == null)
                    fileList = new List<string>();
                foreach (string file in ofd.FileNames.Where(file => !fileList.Contains(file)))
                {
                    _btnStart.Enabled = true;
                    fileList.Add(file);
                }
                _nudTotalSongs.Value = fileList.Count;
            }
        }

        /// <summary>
        ///   Select a path to network ensemble
        /// </summary>
        [FileDialogPermission(SecurityAction.Demand)]
        private void BtnSelectClick(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog { FileName = "Ensemble", Filter = Resources.FileFilterEnsemble };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _tbPathToEnsemble.Text = ofd.FileName;
            }
        }

        /// <summary>
        ///   Start recognition process
        /// </summary>
        private void BtnStartClick(object sender, EventArgs e)
        {
            DefaultFingerprintingConfiguration configuration = new DefaultFingerprintingConfiguration();
            WinQueryResults winform;
            switch (hashAlgorithm)
            {
                case HashAlgorithm.LSH:
                    if (fileList == null || fileList.Count == 0)
                    {
                        MessageBox.Show(Resources.SelectFolderWithSongs, Resources.Songs, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    }

                    winform = new WinQueryResults(
                        (int)_nudNumberOfFingerprints.Value,
                        (int)_numStaratSeconds.Value,
                        WinUtils.GetStride(
                            (StrideType)_cmbStrideType.SelectedIndex, (int)_nudQueryStrideMax.Value, (int)_nudQueryStrideMin.Value, configuration.SamplesPerFingerprint),
                        fileList,
                        (int)_nudHashtables.Value,
                        (int)_nudKeys.Value,
                        Convert.ToInt32(_nudThreshold.Value),
                        tagService,
                        modelService,
                        queryBuilder);
                    winform.Show();
                    break;
                case HashAlgorithm.NeuralHasher:
                    if (fileList == null || fileList.Count == 0)
                    {
                        MessageBox.Show(Resources.SelectFolderWithSongs, Resources.Songs, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    }

                    winform = new WinQueryResults(
                        (int)_nudNumberOfFingerprints.Value,
                        (int)_numStaratSeconds.Value,
                        WinUtils.GetStride(
                            (StrideType)_cmbStrideType.SelectedIndex, (int)_nudQueryStrideMax.Value, (int)_nudQueryStrideMin.Value, configuration.SamplesPerFingerprint),
                        fileList,
                        (int)_nudHashtables.Value,
                        (int)_nudKeys.Value,
                        (int)_nudThreshold.Value,
                        tagService,
                        modelService,
                        queryBuilder);
                    winform.Show();
                    break;
                case HashAlgorithm.None:
                    break;
            }
        }

        private void _btnQueryFromMicrophone_Click(object sender, EventArgs e)
        {
            DefaultFingerprintingConfiguration configuration = new DefaultFingerprintingConfiguration();
            int secondsToRecord = (int)_nudSecondsToRecord.Value;
            int sampleRate = (int)_nudSampleRate.Value;
            string pathToFile = "mic_" + DateTime.Now.Ticks + ".wav";
            fileList.Add(pathToFile);
            _gbQueryMicrophoneBox.Enabled = false;
            float[] samples = audioService.RecordFromMicrophoneToFile(pathToFile, sampleRate, secondsToRecord);
            _gbQueryMicrophoneBox.Enabled = true;
            WinQueryResults winform = new WinQueryResults(
                secondsToRecord,
                0,
                WinUtils.GetStride(
                    (StrideType)_cmbStrideType.SelectedIndex, (int)_nudQueryStrideMax.Value, (int)_nudQueryStrideMin.Value, configuration.SamplesPerFingerprint),
                samples,
                (int)_nudHashtables.Value,
                (int)_nudKeys.Value,
                (int)_nudThreshold.Value,
                tagService,
                modelService,
                queryBuilder);
            winform.Show();
        }
    }
}