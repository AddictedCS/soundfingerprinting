namespace Soundfingerprinting.SoundTools.Misc
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using System.Xml.Serialization;

    using Soundfingerprinting.Audio.Strides;
    using Soundfingerprinting.Fingerprinting;
    using Soundfingerprinting.Fingerprinting.WorkUnitBuilder;
    using Soundfingerprinting.Hashing;
    using Soundfingerprinting.SoundTools.Properties;

    public partial class WinMisc : Form
    {
        private readonly IFingerprintService fingerprintService;
        private readonly IWorkUnitBuilder workUnitBuilder;

        public WinMisc(IFingerprintService fingerprintService, IWorkUnitBuilder workUnitBuilder)
        {
            this.fingerprintService = fingerprintService;
            this.workUnitBuilder = workUnitBuilder;

            InitializeComponent();
            Icon = Resources.Sound;
        }

        private void TbPathToFileMouseDoubleClick(object sender, MouseEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog { Filter = Resources.MusicFilter, FileName = "music.mp3" };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _tbPathToFile.Text = ofd.FileName;
            }
        }

        private void TbOutputPathMouseDoubleClick(object sender, MouseEventArgs e)
        {
            SaveFileDialog ofd = new SaveFileDialog { Filter = Resources.ExportFilter, FileName = "results.txt" };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _tbOutputPath.Text = ofd.FileName;
            }
        }

        private void FadeControls(bool isVisible)
        {
            Invoke(new Action(
                () =>
                {
                    _tbOutputPath.Enabled = isVisible;
                    _tbPathToFile.Enabled = isVisible;
                    _nudMinFrequency.Enabled = isVisible;
                    _nudTopWavelets.Enabled = isVisible;
                    _btnDumpInfo.Enabled = isVisible;
                    _nudDatabaseStride.Enabled = isVisible;
                    _chbDatabaseStride.Enabled = isVisible;
                }));
        }

        private void ChbCompareCheckedChanged(object sender, EventArgs e)
        {
            _tbSongToCompare.Enabled = !_tbSongToCompare.Enabled;
        }

        private void TbSongToCompareMouseDoubleClick(object sender, MouseEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog { Filter = Resources.MusicFilter, FileName = "music.mp3" };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _tbSongToCompare.Text = ofd.FileName;
            }
        }

        private void BtnDumpInfoClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_tbPathToFile.Text))
            {
                MessageBox.Show(
                    Resources.ErrorNoFileToAnalyze,
                    Resources.SelectFile,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (string.IsNullOrEmpty(_tbOutputPath.Text))
            {
                MessageBox.Show(
                    Resources.SelectPathToDump, Resources.SelectFile, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!File.Exists(Path.GetFullPath(_tbPathToFile.Text)))
            {
                MessageBox.Show(
                    Resources.NoSuchFile, Resources.NoSuchFile, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_chbCompare.Checked)
            {
                if (string.IsNullOrEmpty(_tbSongToCompare.Text))
                {
                    MessageBox.Show(
                        Resources.ErrorNoFileToAnalyze,
                        Resources.SelectFile,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }
            }

            FadeControls(false);

            Task.Factory.StartNew(
                () =>
                    {
                        int millisecondsToProcess = (int)_nudSecondsToProcess.Value * 1000;
                        int startAtMillisecond = (int)_nudStartAtSecond.Value * 1000;
                        var databaseSong =
                            workUnitBuilder.BuildWorkUnit().On(
                                _tbPathToFile.Text, millisecondsToProcess, startAtMillisecond).WithCustomConfiguration(
                                    config =>
                                        {
                                            config.MinFrequency = (int)_nudMinFrequency.Value;
                                            config.TopWavelets = (int)_nudTopWavelets.Value;
                                            config.Stride = _chbDatabaseStride.Checked
                                                                ? (IStride)
                                                                  new RandomStride(0, (int)_nudDatabaseStride.Value)
                                                                : new StaticStride((int)_nudDatabaseStride.Value);
                                        });
                        
                        IWorkUnit querySong;
                        if (_chbCompare.Checked)
                        {
                            int comparisonStride = (int)_nudQueryStride.Value;
                            querySong =
                                workUnitBuilder.BuildWorkUnit().On(
                                    _tbSongToCompare.Text, millisecondsToProcess, startAtMillisecond).
                                    WithCustomConfiguration(
                                        config =>
                                        {
                                            config.MinFrequency = (int)_nudMinFrequency.Value;
                                            config.TopWavelets = (int)_nudTopWavelets.Value;
                                            config.Stride = _chbQueryStride.Checked
                                                                ? (IStride)new RandomStride(0, comparisonStride)
                                                                : new StaticStride(comparisonStride);
                                        });
                        }
                        else
                        {
                            querySong =
                                workUnitBuilder.BuildWorkUnit().On(
                                    _tbPathToFile.Text, millisecondsToProcess, startAtMillisecond).
                                    WithCustomConfiguration(
                                        config =>
                                            {
                                                config.MinFrequency = (int)_nudMinFrequency.Value;
                                                config.TopWavelets = (int)_nudTopWavelets.Value;
                                                config.Stride = _chbQueryStride.Checked
                                                                    ? (IStride)
                                                                      new RandomStride(0, (int)_nudQueryStride.Value)
                                                                    : new StaticStride((int)_nudQueryStride.Value);
                                            });
                        }

                        SimilarityResult similarityResult = new SimilarityResult();
                        string pathToInput = _tbPathToFile.Text;
                        string pathToOutput = _tbOutputPath.Text;

                        GetFingerprintSimilarity(fingerprintService, databaseSong, querySong, similarityResult);

                        int hashTables = (int)_nudTables.Value;
                        int hashKeys = (int)_nudKeys.Value;

                        similarityResult.AtLeastOneTableWillVoteForTheCandidate = 1
                                                                                  -
                                                                                  Math.Pow(
                                                                                      1
                                                                                      -
                                                                                      Math.Pow(
                                                                                          similarityResult.
                                                                                          AverageJaqSimilarityBetweenDatabaseAndQuerySong,
                                                                                          hashKeys),
                                                                                      hashTables);
                        similarityResult.AtLeastOneHashbucketFromHashtableWillBeConsideredACandidate =
                            Math.Pow(similarityResult.AverageJaqSimilarityBetweenDatabaseAndQuerySong, hashKeys);

                        similarityResult.WillBecomeACandidateByPassingThreshold =
                            Math.Pow(
                                similarityResult.AtLeastOneHashbucketFromHashtableWillBeConsideredACandidate,
                                (int)_nudCandidateThreshold.Value);


                        similarityResult.Info.MinFrequency = (int)_nudMinFrequency.Value;
                        similarityResult.Info.TopWavelets = (int)_nudTopWavelets.Value;
                        similarityResult.Info.RandomStride = _chbDatabaseStride.Checked;
                        similarityResult.Info.Filename = pathToInput;
                        similarityResult.ComparisonDone = _chbCompare.Checked;
                        
                        using (TextWriter writer = new StreamWriter(pathToOutput))
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(SimilarityResult));
                            serializer.Serialize(writer, similarityResult);
                            writer.Close();
                        }
                    }).ContinueWith(result => FadeControls(true));
        }

        private void GetFingerprintSimilarity(IFingerprintService service, IWorkUnit databaseSong, IWorkUnit querySong, SimilarityResult results)
        {
            double sum = 0;

            List<bool[]> fingerprintsDatabaseSong = databaseSong.GetFingerprintsUsingService(service).Result;
            List<bool[]> fingerprintsQuerySong = querySong.GetFingerprintsUsingService(service).Result;

            int count = fingerprintsDatabaseSong.Count > fingerprintsQuerySong.Count
                            ? fingerprintsQuerySong.Count
                            : fingerprintsDatabaseSong.Count;

            double max = double.MinValue;
            double min = double.MaxValue;
            for (int i = 0; i < count; i++)
            {
                int j = i;
                double value = MinHash.CalculateJaqSimilarity(fingerprintsDatabaseSong[i], fingerprintsQuerySong[j]);
                if (value > max)
                {
                    max = value;
                }

                if (value < min)
                {
                    min = value;
                }

                sum += value;
            }

            results.SumJaqSimilarityBetweenDatabaseAndQuerySong = sum;
            results.AverageJaqSimilarityBetweenDatabaseAndQuerySong = sum / count;
            results.MaxJaqSimilarityBetweenDatabaseAndQuerySong = max;
            results.MinJaqSimilarityBetweenDatabaseAndQuerySong = min;
            results.NumberOfAnalizedFingerprints = count;
        }
    }
}