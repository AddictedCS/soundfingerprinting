namespace Soundfingerprinting.SoundTools.Misc
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using System.Xml.Serialization;

    using Soundfingerprinting.Audio.Strides;
    using Soundfingerprinting.Fingerprinting;
    using Soundfingerprinting.Fingerprinting.Windows;
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
                        IWindowFunction windowFunction = GetWindowFunction();
                        bool normalizeSignal = _cbNormalize.Checked;

                        int millisecondsToProcess = (int)_nudSecondsToProcess.Value * 1000;
                        int startAtMillisecond = (int)_nudStartAtSecond.Value * 1000;
                        int firstQueryStride = (int)_nudFirstQueryStride.Value;

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
                                            config.WindowFunction = windowFunction;
                                            config.NormalizeSignal = normalizeSignal;
                                            config.UseDynamicLogBase = _cbDynamicLog.Checked;
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
                                                                    ? (IStride)new RandomStride(0, comparisonStride, firstQueryStride)
                                                                    : new StaticStride(comparisonStride, firstQueryStride);
                                                config.WindowFunction = windowFunction;
                                                config.NormalizeSignal = normalizeSignal;
                                                config.UseDynamicLogBase = _cbDynamicLog.Checked;
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
                                                                      new RandomStride(0, (int)_nudQueryStride.Value, firstQueryStride)
                                                                    : new StaticStride((int)_nudQueryStride.Value, firstQueryStride);
                                                config.WindowFunction = windowFunction;
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
                            GetFingerprintSimilarity(fingerprintService, databaseSong, querySong, similarityResult);
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

        private IWindowFunction GetWindowFunction()
        {
            if (_cbUseNoWindow.Checked)
            {
                return new NoWindow();
            }

            return new HanningWindow();
        }

        private void GetFingerprintSimilarity(IFingerprintService service, IWorkUnit databaseSong, IWorkUnit querySong, SimilarityResult results)
        {
            double sum = 0;

            List<bool[]> fingerprintsDatabaseSong = databaseSong.GetFingerprintsUsingService(service).Result;
            List<bool[]> fingerprintsQuerySong = querySong.GetFingerprintsUsingService(service).Result;
            
            double max = double.MinValue;
            double min = double.MaxValue;
            int comparisonsCount = 0;
            for (int i = 0; i < fingerprintsDatabaseSong.Count; i++)
            {
                for (int j = 0; j < fingerprintsQuerySong.Count; j++)
                {
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