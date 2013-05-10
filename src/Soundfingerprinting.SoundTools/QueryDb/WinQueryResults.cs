namespace Soundfingerprinting.SoundTools.QueryDb
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Windows.Forms;

    using Soundfingerprinting.Audio.Models;
    using Soundfingerprinting.Audio.Services;
    using Soundfingerprinting.Audio.Strides;
    using Soundfingerprinting.Dao;
    using Soundfingerprinting.Dao.Entities;
    using Soundfingerprinting.Fingerprinting;
    using Soundfingerprinting.Fingerprinting.WorkUnitBuilder;
    using Soundfingerprinting.Hashing;
    using Soundfingerprinting.Hashing.MinHash;
    using Soundfingerprinting.Query;
    using Soundfingerprinting.SoundTools.Properties;

    /// <summary>
    ///   <c>WinQueryResult</c> form, which will show all the results related to the recognition process
    /// </summary>
    public partial class WinQueryResults : Form
    {
        /// <summary>
        ///   Minimum track length (in seconds)
        /// </summary>
        private const int MinTrackLength = 20;

        /// <summary>
        ///   Maximum track length (in seconds)
        /// </summary>
        private const int MaxTrackLength = 60 * 20;

        #region DataGrid Columns

        private const string ColSongName = "SongNameTitle";
        private const string ColResultName = "ResultSongNameTitle";
        private const string ColHit = "CounterHit";
        private const string ColResult = "Result";
        private const string ColPosition = "Position";
        private const string ColHammingAvg = "HammingAvg";
        private const string ColHammingAvgByTrack = "HammingAvgByTrack";
        private const string ColMinHamming = "MinHamming";
        private const string ColSortValue = "SortValue";
        private const string ColStartQueryIndex = "StartQueryIndex";
        private const string ColTotalTables = "TotalTables";
        private const string ColTotalTrackVotes = "TotalTrackVotes";
        private const string ColElapsedTime = "ElapsedTime";
        private const string ColTotalFingerprints = "TotalFingerprints";
        private const string ColSimilarity = "Similarity";

        #endregion

        /// <summary>
        ///   List of files [.mp3] which are going to be recognized
        /// </summary>
        private readonly List<string> fileList;

        /// <summary>
        ///   Number of hash keys per table used in MinHash + LSH schema [normally 5]
        /// </summary>
        private readonly int hashKeys;

        /// <summary>
        ///   Number of hash tables used in the MinHash + LSH schema [normally 20]
        /// </summary>
        private readonly int hashTables;

        /// <summary>
        ///   Permutation storage
        /// </summary>
        private readonly IPermutations permStorage;

        /// <summary>
        ///   The size of the query [E.g. 253 samples]
        /// </summary>
        private readonly IStride queryStride;

        /// <summary>
        ///   Number of fingerprints to analyze
        /// </summary>
        private readonly int secondsToAnalyze;

        /// <summary>
        ///   Start second
        /// </summary>
        private readonly int startSecond;

        /// <summary>
        ///   Recognition threshold 17% ~5 tables from 25 hash functions
        /// </summary>
        private readonly int threshold;

        /// <summary>
        ///   Running thread allows one to Abort forcibly the recognition
        /// </summary>
        private Thread runningThread;

        /// <summary>
        ///   Flag which signalizes stop of the recognition process
        /// </summary>
        private bool stopQuerying;


        public WinQueryResults(int secondsToAnalyze, int startSecond, IStride stride, List<string> fileList, int hashTables, int hashKeys, int thresholdTables)
        {
            InitializeComponent(); /*Initialize Designer Components*/
            Icon = Resources.Sound;
            permStorage = new DbPermutations(ConfigurationManager.ConnectionStrings["FingerprintConnectionString"].ConnectionString);
            this.secondsToAnalyze = secondsToAnalyze; /*Number of fingerprints to analyze from each song*/
            this.startSecond = startSecond;
            this.fileList = fileList; /*List of files to analyze*/
            queryStride = stride;
            this.hashTables = hashTables;
            this.hashKeys = hashKeys;
            threshold = thresholdTables;

            // ReSharper disable PossibleNullReferenceException
            _dgvResults.Columns.Add(ColSongName, "Initial Song");
            _dgvResults.Columns[ColSongName].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvResults.Columns.Add(ColResultName, "Result Song");
            _dgvResults.Columns[ColResultName].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvResults.Columns.Add(ColPosition, "Position");
            _dgvResults.Columns[ColPosition].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvResults.Columns.Add(ColResult, "Result");
            _dgvResults.Columns[ColResult].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvResults.Columns.Add(ColHammingAvg, "Hamming Avg.");
            _dgvResults.Columns[ColHammingAvg].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvResults.Columns.Add(ColHammingAvgByTrack, "Hamming Avg. By Track");
            _dgvResults.Columns[ColHammingAvgByTrack].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvResults.Columns.Add(ColMinHamming, "Min. Hamming");
            _dgvResults.Columns[ColMinHamming].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvResults.Columns.Add(ColSortValue, "Sort Value");
            _dgvResults.Columns[ColSortValue].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvResults.Columns.Add(ColTotalTables, "Total Table Votes");
            _dgvResults.Columns[ColTotalTables].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvResults.Columns.Add(ColTotalTrackVotes, "Total Track Votes");
            _dgvResults.Columns[ColTotalTrackVotes].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvResults.Columns.Add(ColStartQueryIndex, "Query Index Start");
            _dgvResults.Columns[ColStartQueryIndex].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvResults.Columns.Add(ColTotalFingerprints, "Total Fingerprints");
            _dgvResults.Columns[ColTotalFingerprints].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvResults.Columns.Add(ColSimilarity, "Min Similarity");
            _dgvResults.Columns[ColSimilarity].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvResults.Columns.Add(ColElapsedTime, "Elapsed Time");
            _dgvResults.Columns[ColElapsedTime].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            // ReSharper restore PossibleNullReferenceException

            _btnExport.Enabled = false;
            _nudTotal.Value = this.fileList.Count;
            Action action = ExtractCandidatesWithMinHashAlgorithm; /*Extract candidates using MinHash + LSH algorithm*/
            action.BeginInvoke(
                result =>
                    {
                        try
                        {
                            action.EndInvoke(result);
                        }
                        catch (ThreadAbortException)
                        {
                            /*Recognition aborted*/
                            return;
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(
                                Resources.RecognitionEndedWithAnError + e.Message + Resources.LineFeed + e.StackTrace,
                                Resources.Error,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                            return;
                        }

                        MessageBox.Show(Resources.RecognitionEnded, Resources.RecognitionEnded, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Invoke(
                            new Action(
                                () =>
                                    {
                                        _btnStop.Enabled = true;
                                        _btnExport.Enabled = true;
                                    }));
                    },
                action);
        }

        public IModelService ModelService { get; set; }

        public IFingerprintQueryBuilder FingerprintQueryBuilder { get; set; }

        public ITagService TagService { get; set; }

        /// <summary>
        ///   Extract Candidates from the underlying data fingerprint using Min Hash + LSH Schema
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1407:ArithmeticExpressionsMustDeclarePrecedence", Justification = "Reviewed. Suppression is OK here.")]
        private void ExtractCandidatesWithMinHashAlgorithm()
        {
            runningThread = Thread.CurrentThread;
            int recognized = 0, verified = 0;
            IStride samplesToSkip = queryStride;
            Action<int> action = check => _nudChecked.Value = check;
            Action<object[], Color> actionAddItems = (parameters, color) =>
                {
                    int index = _dgvResults.Rows.Add(parameters);
                    _dgvResults.FirstDisplayedScrollingRowIndex = index;
                    if (color != Color.Empty)
                    {
                        _dgvResults.Rows[index].DefaultCellStyle.BackColor = color;
                    }
                };

            Action<float> actionRecognition = recognition => _tbResults.Text = recognition.ToString(CultureInfo.InvariantCulture);

            /*For each song in the list, query the DATABASE*/
            for (int i = 0; i < fileList.Count; i++)
            {
                if (InvokeRequired)
                {
                    Invoke(action, i);
                }
                else
                {
                    action(i);
                }

                if (stopQuerying)
                {
                    break;
                }

                string pathToFile = fileList[i]; /*Path to song to recognize*/
                TagInfo tags = TagService.GetTagInfo(pathToFile); // Get Tags from file

                if (tags == null)
                {
                    // TAGS are null
                    Invoke(actionAddItems, new object[] { "TAGS ARE NULL", pathToFile }, Color.Red);
                    continue;
                }

                string artist = tags.Artist; // Artist
                string title = tags.Title; // Title
                double duration = tags.Duration; // Duration

                // Check whether the duration is ok
                if (duration < MinTrackLength || duration > MaxTrackLength)
                {
                    // Duration too small
                    Invoke(actionAddItems, new object[] { "BAD DURATION", pathToFile }, Color.Red);
                    continue;
                }

                // Check whether the tags are properly defined
                if (string.IsNullOrEmpty(artist) || string.IsNullOrEmpty(title))
                {
                    // Title or Artist tag is null
                    Invoke(actionAddItems, new object[] { "NO TAGS", pathToFile }, Color.Red);
                    continue;
                }

                long elapsedMiliseconds = 0;

                /*Get correct track trackId*/
                Track actualTrack = ModelService.ReadTrackByArtistAndTitleName(artist, title);
                if (actualTrack == null)
                {
                    Invoke(
                        actionAddItems,
                        new object[] { title + "-" + artist, "No such song in the database!", -1, false, 0, -1, -1, -1, -1, -1, -1, -1, elapsedMiliseconds },
                        Color.Red);
                    continue;
                }

                QueryResult queryResult =
                    FingerprintQueryBuilder.BuildQuery().From(pathToFile, secondsToAnalyze * 1000, startSecond * 1000).WithCustomConfigurations(
                        fingerprintConfig =>
                            {
                                fingerprintConfig.Stride = samplesToSkip;
                            },
                        queryConfig =>
                            {
                                queryConfig.NumberOfLSHTables = hashTables;
                                queryConfig.NumberOfMinHashesPerTable = hashKeys;
                                queryConfig.ThresholdVotes = threshold;
                            }).Query().Result;


                if (!queryResult.IsSuccessful)
                {
                    verified++;
                    Invoke(
                        actionAddItems, new object[] { title + "-" + artist, "No candidates!", -1, false, 0, -1, -1, -1, -1, -1, -1, -1, elapsedMiliseconds }, Color.Red);
                    continue;
                }

                Track recognizedTrack = queryResult.BestMatch;
                recognized++;
                verified++;

                Invoke(
                    actionAddItems,
                    new object[]
                        {
                            title + "-" + artist, /*Actual title and artist*/
                            recognizedTrack.Title + "-" + recognizedTrack.Artist, /*Recognized Title Track Name*/
                            1, /*Position in the ordered list*/
                            actualTrack.Id == recognizedTrack.Id, /*Found?*/
                            -1, /*Average hamming distance*/
                            -1, -1, -1, -1, -1, -1, -1, -1, elapsedMiliseconds
                        },
                    Color.Empty);

                Invoke(actionRecognition, (float)recognized / verified);
            }
        }

        /// <summary>
        /// Callback invoked when <c>Stop</c> button is pressed, meaning that the recognition is over
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void BtnStopClick(object sender, EventArgs e)
        {
            stopQuerying = true;
            Invoke(new Action(() => { _btnStop.Enabled = false; }));
        }

        /// <summary>
        /// Callback invoked when the <c>Export</c> button is pressed
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void BtnExportClick(object sender, EventArgs e)
        {
            /*Export into CSV file*/
            WinUtils.ExportInExcel(_dgvResults, "Recognition Rate", _tbResults.Text, "Date", DateTime.Now);
        }


        /// <summary>
        /// Callback invoked when the Win Query results is closing
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void WinQueryResultsFormClosing(object sender, FormClosingEventArgs e)
        {
            if (runningThread != null)
            {
                runningThread.Abort();
            }
        }
    }
}