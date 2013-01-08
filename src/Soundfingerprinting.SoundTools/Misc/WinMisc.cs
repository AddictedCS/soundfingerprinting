namespace Soundfingerprinting.SoundTools.Misc
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;
    using System.Xml.Serialization;

    using Soundfingerprinting.Audio.Services;
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
            SaveFileDialog ofd = new SaveFileDialog {Filter = Resources.ExportFilter, FileName = "results.txt"};
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _tbOutputPath.Text = ofd.FileName;
            }
        }

        private void BtnDumpInfoClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_tbPathToFile.Text))
            {
                MessageBox.Show(Resources.ErrorNoFileToAnalyze, Resources.SelectFile, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (string.IsNullOrEmpty(_tbOutputPath.Text))
            {
                MessageBox.Show(Resources.SelectPathToDump, Resources.SelectFile, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!File.Exists(Path.GetFullPath(_tbPathToFile.Text)))
            {
                MessageBox.Show(Resources.NoSuchFile, Resources.NoSuchFile, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_chbCompare.Checked)
            {
                if (string.IsNullOrEmpty(_tbSongToCompare.Text))
                {
                    MessageBox.Show(Resources.ErrorNoFileToAnalyze, Resources.SelectFile, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }

            Action action =
                () =>
                {
                    using (BassAudioService audioService = new BassAudioService())
                    {
                        FadeControls(false);
                        int stride = (int)_nudStride.Value;
                        var unitOfWork = workUnitBuilder.BuildWorkUnit().On(_tbPathToFile.Text).WithCustomConfiguration(
                                config =>
                                    {
                                        config.MinFrequency = (int)_nudFreq.Value;
                                        config.TopWavelets = (int)_nudTopWavelets.Value;
                                        config.Stride = _chbStride.Checked
                                                            ? (IStride)new RandomStride(0, stride)
                                                            : new StaticStride(stride);
                                    });

                        var sameUnitOfWork = workUnitBuilder.BuildWorkUnit().On(_tbPathToFile.Text).WithCustomConfiguration(
                               config =>
                               {
                                   config.MinFrequency = (int)_nudFreq.Value;
                                   config.TopWavelets = (int)_nudTopWavelets.Value;
                                   config.Stride = new StaticStride(5115, 5115 / 2);
                               });

                        DumpResults resultObj = new DumpResults();
                        string pathToInput = _tbPathToFile.Text;
                        string pathToOutput = _tbOutputPath.Text;
                        int hashTables = (int)_nudTables.Value;
                        int hashKeys = (int)_nudKeys.Value;

                        GetFingerprintSimilarity(fingerprintService, unitOfWork, sameUnitOfWork, resultObj);
                        GetHashSimilarity(fingerprintService, hashTables, hashKeys, unitOfWork, sameUnitOfWork, resultObj);

                        if (_chbCompare.Checked)
                        {
                            int comparisonStride = (int)_nudQueryStride.Value;
                            var unitOfWorkToCompareWith =
                                workUnitBuilder.BuildWorkUnit().On(_tbSongToCompare.Text).WithCustomConfiguration(
                                    config =>
                                    {
                                        config.MinFrequency = (int)_nudFreq.Value;
                                        config.TopWavelets = (int)_nudTopWavelets.Value;
                                        config.Stride = _chbQueryStride.Checked
                                                            ? (IStride)new RandomStride(0, comparisonStride)
                                                            : new StaticStride(comparisonStride);
                                    });

                            GetFingerprintSimilarity(fingerprintService, unitOfWork, unitOfWorkToCompareWith, resultObj);
                        }

                        resultObj.Info.MinFrequency = (int)_nudFreq.Value;
                        resultObj.Info.TopWavelets = (int)_nudTopWavelets.Value;
                        resultObj.Info.StrideSize = stride;
                        resultObj.Info.RandomStride = _chbStride.Checked;
                        resultObj.Info.Filename = pathToInput;
                        resultObj.ComparisonDone = _chbCompare.Checked;

                        XmlSerializer serializer = new XmlSerializer(typeof(DumpResults));
                        TextWriter writer = new StreamWriter(pathToOutput);
                        serializer.Serialize(writer, resultObj);
                        writer.Close();
                    }
                };
            action.BeginInvoke(
                (result) =>
                {
                    action.EndInvoke(result);
                    FadeControls(true);
                },
                null);
        }

        /// <summary>
        /// Get signature similarity between 2 different songs.
        /// </summary>
        /// <param name="service">
        /// The service.
        /// </param>
        /// <param name="unitOfWork">
        /// The unit Of Work.
        /// </param>
        /// <param name="unitOfWorkToCompareWith">
        /// The unit Of Work To Compare With.
        /// </param>
        /// <param name="results">
        /// The results.
        /// </param>
        private void GetFingerprintSimilarity(IFingerprintService service, IWorkUnit unitOfWork, IWorkUnit unitOfWorkToCompareWith, DumpResults results)
        {
            double sum = 0;

            List<bool[]> imglista = unitOfWork.GetFingerprintsUsingService(service).Result;
            List<bool[]> imglistb = unitOfWorkToCompareWith.GetFingerprintsUsingService(service).Result;


            int count = imglista.Count > imglistb.Count ? imglistb.Count : imglista.Count;
            double max = double.MinValue;
            for (int i = 0; i < count; i++)
            {
                int j = i;
                double value = MinHash.CalculateSimilarity(imglista[i], imglistb[j]);
                if (value > max)
                {
                    max = value;
                }

                sum += value;
            }

            results.SumJaqFingerprintSimilarityBetweenDiffertSongs = sum;
            results.AverageJaqFingerprintsSimilarityBetweenDifferentSongs = sum / count;
            results.MaxJaqFingerprintsSimilarityBetweenDifferentSongs = max;
        }

        /// <summary>
        ///   Get hash similarity of one song
        /// </summary>
        /// <param name = "service">Fingerprint service</param>
        /// <param name = "hashTables">Number of hash tables in the LSH transformation</param>
        /// <param name = "hashKeys">Number of hash keys per table in the LSH transformation</param>
        /// <param name = "path">Path to analyzed file</param>
        /// <param name = "results">Results object to be filled with the appropriate data</param>
        private void GetHashSimilarity(IFingerprintService service, int hashTables, int hashKeys, IWorkUnit unitOfWork, IWorkUnit sameUnitOfWork, DumpResults results)
        {
            double sum = 0;
            int hashesCount = 0;
            int startindex = 0;

            List<bool[]> listDb = unitOfWork.GetFingerprintsUsingService(service).Result;
            List<bool[]> listQuery = sameUnitOfWork.GetFingerprintsUsingService(service).Result;
            IPermutations perms = new DbPermutations(ConfigurationManager.ConnectionStrings["FingerprintConnectionString"].ConnectionString);
            MinHash minHash = new MinHash(perms);
            List<int[]> minHashDb = listDb.Select(minHash.ComputeMinHashSignature).ToList();
            List<int[]> minHashQuery = listQuery.Select(minHash.ComputeMinHashSignature).ToList();

            /*Calculate Min Hash signature similarity by comparing 2 consecutive signatures*/
            int countDb = minHashDb.Count;
            int countQuery = minHashQuery.Count;
            int minHashSignatureLen = minHashDb[0].Length;
            int similarMinHashValues = 0;
            for (int i = 0; i < countDb; i++)
            {
                for (int j = 0; j < countQuery; j++)
                {
                    for (int k = 0; k < minHashSignatureLen; k++)
                        if (minHashDb[i][k] == minHashQuery[j][k])
                            similarMinHashValues++;
                }
            }
            results.Results.SumIdenticalMinHash = similarMinHashValues;
            results.Results.AverageIdenticalMinHash = (double) similarMinHashValues/(countDb*countQuery*minHashSignatureLen);

            /*Group min hash signatures into LSH Buckets*/
            List<Dictionary<int, long>> lshBucketsDb =
                minHashDb.Select(item => minHash.GroupMinHashToLSHBuckets(item, hashTables, hashKeys)).ToList();

            List<Dictionary<int, long>> lshBucketsQuery =
                minHashQuery.Select(item => minHash.GroupMinHashToLSHBuckets(item, hashTables, hashKeys)).ToList();

            int countSignatures = lshBucketsDb.Count;
            sum = 0;
            foreach (Dictionary<int, long> a in lshBucketsDb)
            {
                Dictionary<int, long>.ValueCollection aValues = a.Values;
                foreach (Dictionary<int, long> b in lshBucketsQuery)
                {
                    Dictionary<int, long>.ValueCollection bValues = b.Values;
                    hashesCount += aValues.Intersect(bValues).Count();
                }
            }

            results.Results.SumJaqLSHBucketSimilarity = -1;
            results.Results.AverageJaqLSHBucketSimilarity = -1;
            results.Results.TotalIdenticalLSHBuckets = hashesCount;
        }

        /// <summary>
        ///   Fade all controls
        /// </summary>
        /// <param name = "isVisible">Set the parameters as visible/invisible</param>
        private void FadeControls(bool isVisible)
        {
            Invoke(new Action(
                () =>
                {
                    _tbOutputPath.Enabled = isVisible;
                    _tbPathToFile.Enabled = isVisible;
                    _nudFreq.Enabled = isVisible;
                    _nudTopWavelets.Enabled = isVisible;
                    _btnDumpInfo.Enabled = isVisible;
                    _nudStride.Enabled = isVisible;
                    _chbStride.Enabled = isVisible;
                }));
        }

        /// <summary>
        ///   Check box checked
        /// </summary>
        private void ChbCompareCheckedChanged(object sender, EventArgs e)
        {
            _tbSongToCompare.Enabled = !_tbSongToCompare.Enabled;
        }

        /// <summary>
        ///   Song to compare select
        /// </summary>
        private void TbSongToCompareMouseDoubleClick(object sender, MouseEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog {Filter = Resources.MusicFilter, FileName = "music.mp3"};
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _tbSongToCompare.Text = ofd.FileName;
            }
        }
    }
}