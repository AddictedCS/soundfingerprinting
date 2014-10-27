namespace SoundFingerprinting.SoundTools.QueryDb
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Security.Permissions;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.SoundTools.Properties;
    using SoundFingerprinting.Strides;

    public partial class WinCheckHashBins : Form
    {
        private readonly IQueryCommandBuilder queryCommandBuilder;
        private readonly ITagService tagService;
        private readonly IModelService modelService;
        private readonly IAudioService audioService;
        private readonly IWaveFileUtility waveFileUtility;

        private readonly List<string> filters = new List<string>(new[] { "*.mp3", "*.wav", "*.ogg", "*.flac" });
        private List<string> fileList = new List<string>();
        private HashAlgorithm hashAlgorithm = HashAlgorithm.LSH;

        public WinCheckHashBins(IQueryCommandBuilder queryCommandBuilder, ITagService tagService, IModelService modelService, IAudioService audioService, IWaveFileUtility waveFileUtility)
        {
            this.queryCommandBuilder = queryCommandBuilder;
            this.tagService = tagService;
            this.modelService = modelService;
            this.audioService = audioService;
            this.waveFileUtility = waveFileUtility;

            InitializeComponent();

            Icon = Resources.Sound;
            AddConnectionStringsToComboBox();

            _cmbAlgorithm.SelectedIndex = (int)hashAlgorithm;

            string[] items = Enum.GetNames(typeof(StrideType)); 

            _cmbStrideType.Items.AddRange(items.ToArray<object>());
            _cmbStrideType.SelectedIndex = 3;

            switch (_cmbAlgorithm.SelectedIndex)
            {
                case (int)HashAlgorithm.LSH:
                    _gbMinHash.Enabled = true;
                    _gbNeuralHasher.Enabled = false;
                    break;
                case (int)HashAlgorithm.NeuralHasher:
                    _gbMinHash.Enabled = false;
                    _gbNeuralHasher.Enabled = true;
                    break;
                case (int)HashAlgorithm.None:
                    _gbMinHash.Enabled = false;
                    _gbNeuralHasher.Enabled = false;
                    break;
            }

            _gbQueryMicrophoneBox.Enabled = audioService.IsRecordingSupported;
        }

        private void AddConnectionStringsToComboBox()
        {
            foreach (object item in ConfigurationManager.ConnectionStrings)
            {
                _cmbConnectionString.Items.Add(item.ToString());
            }

            if (_cmbConnectionString.Items.Count > 0)
            {
                _cmbConnectionString.SelectedIndex = 0;
            }
        }

        private void CmbAlgorithmSelectedIndexChanged(object sender, EventArgs e)
        {
            hashAlgorithm = (HashAlgorithm)_cmbAlgorithm.SelectedIndex;
            _gbMinHash.Enabled = true;
            _gbNeuralHasher.Enabled = false;
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

        [FileDialogPermission(SecurityAction.Demand)]
        private void BtnBrowseSongClick(object sender, EventArgs e)
        {
            string filter = WinUtils.GetMultipleFilter("Audio files", filters);
            OpenFileDialog ofd = new OpenFileDialog { Filter = filter, Multiselect = true };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _tbSingleFile.Text = null;
                foreach (string file in ofd.FileNames)
                {
                    _tbSingleFile.Text += "\"" + Path.GetFileName(file) + "\" ";
                }

                foreach (string file in ofd.FileNames.Where(file => !fileList.Contains(file)))
                {
                    _btnStart.Enabled = true;
                    fileList.Add(file);
                }

                _nudTotalSongs.Value = fileList.Count;
            }
        }

        [FileDialogPermission(SecurityAction.Demand)]
        private void BtnSelectClick(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog { FileName = "Ensemble", Filter = Resources.FileFilterEnsemble };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _tbPathToEnsemble.Text = ofd.FileName;
            }
        }

        private void BtnStartClick(object sender, EventArgs e)
        {
            DefaultFingerprintConfiguration configuration = new DefaultFingerprintConfiguration();
            switch (hashAlgorithm)
            {
                case HashAlgorithm.LSH:
                    if (!fileList.Any())
                    {
                        MessageBox.Show(Resources.SelectFolderWithSongs, Resources.Songs, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    }

                    WinQueryResults winQueryResults = new WinQueryResults(
                        _numSecondsToAnalyze.Enabled ? (int)_numSecondsToAnalyze.Value : 0,
                        _numStartAtSecond.Enabled ? (int)_numStartAtSecond.Value : 0,
                        (int)_nudHashtables.Value,
                        (int)_nudKeys.Value,
                        Convert.ToInt32(_nudThreshold.Value),
                        WinUtils.GetStride((StrideType)_cmbStrideType.SelectedIndex, (int)_nudQueryStrideMax.Value, (int)_nudQueryStrideMin.Value, configuration.SamplesPerFingerprint),
                        tagService,
                        modelService,
                        audioService,
                        queryCommandBuilder);
                    winQueryResults.Show();
                    winQueryResults.Refresh();
                    winQueryResults.ExtractCandidatesWithMinHashAlgorithm(fileList);
                    break;
                case HashAlgorithm.NeuralHasher:
                    throw new NotImplementedException();
                case HashAlgorithm.None:
                    throw new NotImplementedException();
            }
        }

        private void BtnQueryFromMicrophoneClick(object sender, EventArgs e)
        {
            DefaultFingerprintConfiguration configuration = new DefaultFingerprintConfiguration();
            int secondsToRecord = (int)_nudSecondsToRecord.Value;
            int sampleRate = (int)_nudSampleRate.Value;
            string pathToFile = "mic_" + DateTime.Now.Ticks + ".wav";
            _gbQueryMicrophoneBox.Enabled = false;
            Task<float[]>.Factory.StartNew(() => audioService.ReadMonoSamplesFromMicrophone(sampleRate, secondsToRecord)).ContinueWith(
                task =>
                    {
                        var samples = task.Result;
                        waveFileUtility.WriteSamplesToFile(samples, sampleRate, pathToFile);

                        _gbQueryMicrophoneBox.Enabled = true;
                        WinQueryResults winQueryResults = new WinQueryResults(
                            secondsToRecord,
                            0,
                            (int)_nudHashtables.Value,
                            (int)_nudKeys.Value,
                            (int)_nudThreshold.Value,
                            WinUtils.GetStride((StrideType)_cmbStrideType.SelectedIndex, (int)_nudQueryStrideMax.Value, (int)_nudQueryStrideMin.Value, configuration.SamplesPerFingerprint),
                            tagService,
                            modelService,
                            audioService,
                            queryCommandBuilder);
                        winQueryResults.Show();
                        winQueryResults.Refresh();
                        winQueryResults.ExtractCandidatesUsingSamples(task.Result);
                    },
                TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void CbFullSongCheckedChanged(object sender, EventArgs e)
        {
            _numSecondsToAnalyze.Enabled = _numStartAtSecond.Enabled = !cbFullSong.Checked;
        }

        private void BtnQueryFromUrlClick(object sender, EventArgs e)
        {
            string url = tbUrlToResource.Text;
            int sampleRate = (int)nudSampleRateUrl.Value;
            int seconds = (int)nudSecondsUrl.Value;
            string pathToFile = "url_" + DateTime.Now.Ticks + ".wav";

            Task<float[]>.Factory.StartNew(() => audioService.ReadMonoSamplesFromStreamingUrl(url, sampleRate, seconds))
                .ContinueWith(task =>
                    { 
                        float[] samples = task.Result;
                        waveFileUtility.WriteSamplesToFile(samples, sampleRate, pathToFile);
                    });
        }
    }
}
