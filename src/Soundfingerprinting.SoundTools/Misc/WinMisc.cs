// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;
using Soundfingerprinting.AudioProxies;
using Soundfingerprinting.AudioProxies.Strides;
using Soundfingerprinting.Fingerprinting;
using Soundfingerprinting.Hashing;
using Soundfingerprinting.SoundTools.Properties;

namespace Soundfingerprinting.SoundTools.Misc
{
    /// <summary>
    ///   Miscellaneous empirical tests
    /// </summary>
    public partial class WinMisc : Form
    {
        /// <summary>
        ///   Constructor
        /// </summary>
        public WinMisc()
        {
            InitializeComponent();
            Icon = Resources.Sound;
        }

        /// <summary>
        ///   Path to *.mp3 file was selected
        /// </summary>
        private void TbPathToFileMouseDoubleClick(object sender, MouseEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog {Filter = Resources.MusicFilter, FileName = "music.mp3"};
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _tbPathToFile.Text = ofd.FileName;
            }
        }


        /// <summary>
        ///   Path to output file was chosen
        /// </summary>
        private void TbOutputPathMouseDoubleClick(object sender, MouseEventArgs e)
        {
            SaveFileDialog ofd = new SaveFileDialog {Filter = Resources.ExportFilter, FileName = "results.txt"};
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _tbOutputPath.Text = ofd.FileName;
            }
        }

        /// <summary>
        ///   Dump information into file
        /// </summary>
        private void BtnDumpInfoClick(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(_tbPathToFile.Text))
            {
                MessageBox.Show(Resources.ErrorNoFileToAnalyze, Resources.SelectFile, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (String.IsNullOrEmpty(_tbOutputPath.Text))
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
                if (String.IsNullOrEmpty(_tbSongToCompare.Text))
                {
                    MessageBox.Show(Resources.ErrorNoFileToAnalyze, Resources.SelectFile, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
            Action action =
                () =>
                {
                    using (BassProxy proxy = new BassProxy())
                    {
                        FadeControls(false);
                        int minFreq = (int) _nudFreq.Value;
                        int topWavelets = (int) _nudTopWavelets.Value;
                        int stride = (int) _nudStride.Value;
                        IStride objStride = (_chbStride.Checked) ? (IStride) new RandomStride(0, stride) : new StaticStride(stride);
                        FingerprintManager manager = new FingerprintManager {MinFrequency = minFreq, TopWavelets = topWavelets};
                        DumpResults resultObj = new DumpResults();
                        string pathToInput = _tbPathToFile.Text;
                        string pathToOutput = _tbOutputPath.Text;
                        int hashTables = (int) _nudTables.Value;
                        int hashKeys = (int) _nudKeys.Value;
                        stride = (int) _nudQueryStride.Value;
                        int numFingerprints = (int) _nudNumberOfSubsequent.Value;
                        IStride queryStride = (_chbQueryStride.Checked) ? (IStride) new RandomStride(0, stride) : new StaticStride(stride);
                        queryStride = new StaticStride(5115, 5115/2);
                        GetFingerprintSimilarity(manager, objStride, queryStride, numFingerprints, proxy, pathToInput, resultObj);
                        GetHashSimilarity(manager, objStride, queryStride, numFingerprints, hashTables, hashKeys, proxy, pathToInput, resultObj);

                        if (_chbCompare.Checked)
                        {
                            string pathToDifferent = _tbSongToCompare.Text;
                            GetFingerprintSimilarity(manager, objStride, proxy, pathToInput, pathToDifferent, resultObj);
                        }
                        resultObj.Info.MinFrequency = minFreq;
                        resultObj.Info.TopWavelets = topWavelets;
                        resultObj.Info.StrideSize = stride;
                        resultObj.Info.RandomStride = _chbStride.Checked;
                        resultObj.Info.Filename = pathToInput;
                        resultObj.ComparisonDone = _chbCompare.Checked;

                        XmlSerializer serializer = new XmlSerializer(typeof (DumpResults));
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
                }, null);
        }

        /// <summary>
        ///   Get fingerprint similarity between 2 different songs.
        /// </summary>
        /// <param name = "manager">Fingerprint manager used in file decomposition</param>
        /// <param name = "stride">Stride object parameter</param>
        /// <param name = "proxy">Proxy to the audio object</param>
        /// <param name = "path">Path to first file</param>
        /// <param name = "differentPath">Path to different file</param>
        /// <param name = "results">Results object to be filled with the corresponding data</param>
        private static void GetFingerprintSimilarity(FingerprintManager manager, IStride stride, IAudio proxy, string path, string differentPath, DumpResults results)
        {
            int startindex = 0;
            int count = 0;
            double sum = 0;

            List<bool[]> imglista = manager.CreateFingerprints(proxy, path, stride);
            List<bool[]> imglistb = manager.CreateFingerprints(proxy, differentPath, stride);


            count = imglista.Count > imglistb.Count ? imglistb.Count : imglista.Count;
            double max = double.MinValue;
            for (int i = 0; i < count; i++)
            {
                int j = i;
                double value = MinHash.CalculateSimilarity(imglista[i], imglistb[j]);
                if (value > max)
                    max = value;
                sum += value;
            }

            results.SumJaqFingerprintSimilarityBetweenDiffertSongs = sum;
            results.AverageJaqFingerprintsSimilarityBetweenDifferentSongs = sum/count;
            results.MaxJaqFingerprintsSimilarityBetweenDifferentSongs = max;
        }

        /// <summary>
        ///   Get fingerprint similarity of one song
        /// </summary>
        /// <param name = "manager">Fingerprint manager used in file decomposition</param>
        /// <param name = "dbstride">Database creation stride</param>
        /// <param name = "queryStride">Query stride</param>
        /// <param name = "numberOfItemsToCompare">Number of subsequent elements to compare with</param>
        /// <param name = "proxy">Proxy</param>
        /// <param name = "path">Path to first file</param>
        /// <param name = "results">Results object to be filled with the corresponding data</param>
        private static void GetFingerprintSimilarity(FingerprintManager manager, IStride dbstride, IStride queryStride, int numberOfItemsToCompare, IAudio proxy, string path, DumpResults results)
        {
            int startindex = 0;
            int count = 0;
            double sum = 0;

            List<bool[]> list = manager.CreateFingerprints(proxy, path, dbstride);
            List<bool[]> listToCompare = manager.CreateFingerprints(proxy, path, queryStride);

            count = list.Count;
            int toCompare = listToCompare.Count;

            double max = double.MinValue;

            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < toCompare; j++)
                {
                    double value = MinHash.CalculateSimilarity(list[i], listToCompare[j]);
                    if (value > max)
                        max = value;
                    sum += value;
                }
            }

            results.Results.SumJaqFingerprintsSimilarity = sum;
            results.Results.AverageJaqFingerprintSimilarity = sum/(count*toCompare);
            results.Results.MaxJaqFingerprintSimilarity = max;
        }

        /// <summary>
        ///   Get hash similarity of one song
        /// </summary>
        /// <param name = "manager">Fingerprint manager</param>
        /// <param name = "dbstride">Database stride between fingerprints</param>
        /// <param name = "queryStride">Query stride between fingerprints</param>
        /// <param name = "numberOfFingerprintsToAnalyze">Number of fingerprints to analyze</param>
        /// <param name = "hashTables">Number of hash tables in the LSH transformation</param>
        /// <param name = "hashKeys">Number of hash keys per table in the LSH transformation</param>
        /// <param name = "proxy">Audio proxy</param>
        /// <param name = "path">Path to analyzed file</param>
        /// <param name = "results">Results object to be filled with the appropriate data</param>
        private static void GetHashSimilarity(FingerprintManager manager, IStride dbstride, IStride queryStride, int numberOfFingerprintsToAnalyze, int hashTables, int hashKeys, IAudio proxy, string path, DumpResults results)
        {
            double sum = 0;
            int hashesCount = 0;
            int startindex = 0;

            List<bool[]> listDb = manager.CreateFingerprints(proxy, path, dbstride);
            List<bool[]> listQuery = manager.CreateFingerprints(proxy, path, queryStride);
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

        /// <summary>
        ///   On window form loading event
        /// </summary>
        private void WinMiscLoad(object sender, EventArgs e)
        {
            FingerprintManager manager = new FingerprintManager();
            _nudFreq.Value = manager.MinFrequency;
            _nudTopWavelets.Value = manager.TopWavelets;
        }
    }
}