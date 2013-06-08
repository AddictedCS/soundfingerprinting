namespace Soundfingerprinting.SoundTools.Misc
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using System.Xml.Serialization;

    using Soundfingerprinting.Builder;
    using Soundfingerprinting.Hashing.Utils;
    using Soundfingerprinting.SoundTools.Properties;
    using Soundfingerprinting.Strides;
    
    public partial class WinMisc : Form
    {
        private readonly IFingerprintUnitBuilder fingerprintUnitBuilder;

        public WinMisc(IFingerprintUnitBuilder fingerprintUnitBuilder)
        {
            this.fingerprintUnitBuilder = fingerprintUnitBuilder;

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
                        bool normalizeSignal = _cbNormalize.Checked;

                        int secondsToProcess = (int)_nudSecondsToProcess.Value;
                        int startAtSecond = (int)_nudStartAtSecond.Value;
                        int firstQueryStride = (int)_nudFirstQueryStride.Value;

                        var databaseSong =
                            fingerprintUnitBuilder.BuildFingerprints().On(
                                _tbPathToFile.Text, secondsToProcess, startAtSecond).WithCustomConfiguration(
                                    config =>
                                        {
                                            config.MinFrequency = (int)_nudMinFrequency.Value;
                                            config.TopWavelets = (int)_nudTopWavelets.Value;
                                            config.Stride = _chbDatabaseStride.Checked
                                                                ? (IStride)
                                                                  new IncrementalRandomStride(0, (int)_nudDatabaseStride.Value, config.SamplesPerFingerprint)
                                                                : new IncrementalStaticStride((int)_nudDatabaseStride.Value, config.SamplesPerFingerprint);
                                            config.NormalizeSignal = normalizeSignal;
                                            config.UseDynamicLogBase = _cbDynamicLog.Checked;
                                        });

                        IFingerprintUnit querySong;
                        int comparisonStride = (int)_nudQueryStride.Value;
                        if (_chbCompare.Checked)
                        {
                            querySong =
                                fingerprintUnitBuilder.BuildFingerprints()
                                                          .On(_tbSongToCompare.Text, secondsToProcess, startAtSecond)
                                                          .WithCustomConfiguration(
                                                              config =>
                                                                  {
                                                                      config.MinFrequency = (int)_nudMinFrequency.Value;
                                                                      config.TopWavelets = (int)_nudTopWavelets.Value;
                                                                      config.Stride = _chbQueryStride.Checked
                                                                                          ? (IStride)
                                                                                            new IncrementalRandomStride(
                                                                                                0, comparisonStride, config.SamplesPerFingerprint, firstQueryStride)
                                                                                          : new IncrementalStaticStride(
                                                                                                comparisonStride, config.SamplesPerFingerprint, firstQueryStride);
                                                                      config.NormalizeSignal = normalizeSignal;
                                                                      config.UseDynamicLogBase = _cbDynamicLog.Checked;
                                                                  });
                        }
                        else
                        {
                            querySong =
                                fingerprintUnitBuilder.BuildFingerprints()
                                                          .On(_tbPathToFile.Text, secondsToProcess, startAtSecond)
                                                          .WithCustomConfiguration(
                                                              config =>
                                                                  {
                                                                      config.MinFrequency = (int)_nudMinFrequency.Value;
                                                                      config.TopWavelets = (int)_nudTopWavelets.Value;
                                                                      config.Stride = _chbQueryStride.Checked
                                                                                          ? (IStride)
                                                                                            new IncrementalRandomStride(
                                                                                                0, comparisonStride, config.SamplesPerFingerprint, firstQueryStride)
                                                                                          : new IncrementalStaticStride(
                                                                                                comparisonStride, config.SamplesPerFingerprint, firstQueryStride);
                                                                      config.NormalizeSignal = normalizeSignal;
                                                                      config.UseDynamicLogBase = _cbDynamicLog.Checked;
                                                                  });
                        }

                        SimilarityResult similarityResult = new SimilarityResult();
                        string pathToInput = _tbPathToFile.Text;
                        string pathToOutput = _tbOutputPath.Text;
                        int iterations = (int)_nudIterations.Value;
                        int hashTables = (int)_nudTables.Value;
                        int hashKeys = (int)_nudKeys.Value;

                        for (int i = 0; i < iterations; i++)
                        {
                            GetFingerprintSimilarity(databaseSong, querySong, similarityResult);
                        }

                        similarityResult.Info.MinFrequency = (int)_nudMinFrequency.Value;
                        similarityResult.Info.TopWavelets = (int)_nudTopWavelets.Value;
                        similarityResult.Info.IsQueryStrideRandom = _chbQueryStride.Checked;
                        similarityResult.Info.IsDatabaseStrideRandom = _chbDatabaseStride.Checked;
                        similarityResult.Info.Filename = pathToInput;
                        similarityResult.Info.QueryStrideSize = (int)_nudQueryStride.Value;
                        similarityResult.Info.DatabaseStrideSize = (int)_nudDatabaseStride.Value;
                        similarityResult.Info.QueryFirstStrideSize = (int)_nudFirstQueryStride.Value;
                        similarityResult.Info.Iterations = iterations;
                        similarityResult.Info.HashTables = hashTables;
                        similarityResult.Info.HashKeys = hashKeys;
                        similarityResult.ComparisonDone = _chbCompare.Checked;
                        
                        if (_chbCompare.Checked)
                        {
                            similarityResult.Info.ComparedWithFile = _tbSongToCompare.Text;
                        }

                        similarityResult.SumJaqSimilarityBetweenDatabaseAndQuerySong /= iterations;
                        similarityResult.AverageJaqSimilarityBetweenDatabaseAndQuerySong /= iterations;
                        similarityResult.AtLeastOneTableWillVoteForTheCandidate = 1 - Math.Pow(1 - Math.Pow(similarityResult.AverageJaqSimilarityBetweenDatabaseAndQuerySong, hashKeys), hashTables);
                        similarityResult.AtLeastOneHashbucketFromHashtableWillBeConsideredACandidate = Math.Pow(similarityResult.AverageJaqSimilarityBetweenDatabaseAndQuerySong, hashKeys);
                        similarityResult.WillBecomeACandidateByPassingThreshold = Math.Pow(similarityResult.AtLeastOneHashbucketFromHashtableWillBeConsideredACandidate, (int)_nudCandidateThreshold.Value);

                        using (TextWriter writer = new StreamWriter(pathToOutput))
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(SimilarityResult));
                            serializer.Serialize(writer, similarityResult);
                            writer.Close();
                        }
                    }).ContinueWith(result => FadeControls(true));
        }

        private void GetFingerprintSimilarity(IFingerprintUnit databaseSong, IFingerprintUnit querySong, SimilarityResult results)
        {
            double sum = 0;

            List<bool[]> fingerprintsDatabaseSong = databaseSong.RunAlgorithm().Result;
            List<bool[]> fingerprintsQuerySong = querySong.RunAlgorithm().Result;
            
            double max = double.MinValue;
            double min = double.MaxValue;
            int comparisonsCount = 0;
            for (int i = 0; i < fingerprintsDatabaseSong.Count; i++)
            {
                for (int j = 0; j < fingerprintsQuerySong.Count; j++)
                {
                    double value = HashingUtils.CalculateJaqSimilarity(fingerprintsDatabaseSong[i], fingerprintsQuerySong[j]);
                    if (value > max)
                    {
                        max = value;
                    }

                    if (value < min)
                    {
                        min = value;
                    }

                    sum += value;
                    comparisonsCount++;
                }
            }

            results.SumJaqSimilarityBetweenDatabaseAndQuerySong += sum;
            results.AverageJaqSimilarityBetweenDatabaseAndQuerySong += sum / comparisonsCount;
            if (max > results.MaxJaqSimilarityBetweenDatabaseAndQuerySong)
            {
                results.MaxJaqSimilarityBetweenDatabaseAndQuerySong = max;
            }

            if (min < results.MinJaqSimilarityBetweenDatabaseAndQuerySong)
            {
                results.MinJaqSimilarityBetweenDatabaseAndQuerySong = min;
            }

            results.NumberOfAnalizedFingerprints = comparisonsCount;
        }
    }
}