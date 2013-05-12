namespace Soundfingerprinting.SoundTools.QueryDb
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    using Soundfingerprinting.Audio.Models;
    using Soundfingerprinting.Audio.Services;
    using Soundfingerprinting.Audio.Strides;
    using Soundfingerprinting.Dao;
    using Soundfingerprinting.Dao.Entities;
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

        private readonly int hashKeys;
        
        private readonly int hashTables;

        private readonly IStride queryStride;

        private readonly int secondsToAnalyze;

        private readonly int startSecond;

        private readonly int threshold;

        private readonly ITagService tagService;

        private readonly IModelService modelService;

        private readonly IFingerprintQueryBuilder fingerprintQueryBuilder;

        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private bool stopQuerying;
        
        public WinQueryResults(
            int secondsToAnalyze,
            int startSecond,
            IStride stride,
            int hashTables,
            int hashKeys,
            int thresholdTables,
            ITagService tagService,
            IModelService modelService,
            IFingerprintQueryBuilder fingerprintQueryBuilder)
        {
            InitializeComponent(); /*Initialize Designer Components*/
            Icon = Resources.Sound;
            this.secondsToAnalyze = secondsToAnalyze; /*Number of fingerprints to analyze from each song*/
            this.startSecond = startSecond;
            queryStride = stride;

            this.hashTables = hashTables;
            this.hashKeys = hashKeys;
            threshold = thresholdTables;
            this.tagService = tagService;
            this.modelService = modelService;
            this.fingerprintQueryBuilder = fingerprintQueryBuilder;

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
        }

        public void ExtractCandidatesWithMinHashAlgorithm(List<string> fileList)
        {
            int recognized = 0, verified = 0;
            IStride samplesToSkip = queryStride;
           
            /*For each song in the list, query the DATABASE*/
            for (int i = 0; i < fileList.Count; i++)
            {
                if (stopQuerying)
                {
                    cancellationTokenSource.Cancel();
                    break;
                }

                string pathToFile = fileList[i]; /*Path to song to recognize*/
                TagInfo tags = tagService.GetTagInfo(pathToFile); // Get Tags from file

                if (tags == null)
                {
                    // TAGS are null
                    AddGridLine(new object[] { "TAGS ARE NULL", pathToFile }, Color.Red);
                    continue;
                }

                string artist = tags.Artist; // Artist
                string title = tags.Title; // Title
                double duration = tags.Duration; // Duration

                // Check whether the duration is ok
                if (duration < MinTrackLength || duration > MaxTrackLength)
                {
                    // Duration too small
                    AddGridLine(new object[] { "BAD DURATION", pathToFile }, Color.Red);
                    continue;
                }

                // Check whether the tags are properly defined
                if (string.IsNullOrEmpty(artist) || string.IsNullOrEmpty(title))
                {
                    // Title or Artist tag is null
                    AddGridLine(new object[] { "NO TAGS", pathToFile }, Color.Red);
                    continue;
                }

                /*Get correct track trackId*/
                Track actualTrack = modelService.ReadTrackByArtistAndTitleName(artist, title);
                if (actualTrack == null)
                {
                    AddGridLine(new object[] { title + "-" + artist, "No such song in the database!", -1, false, 0, -1, -1, -1, -1, -1, -1, -1, 0 }, Color.Red);
                    continue;
                }

                fingerprintQueryBuilder.BuildQuery()
                                       .From(pathToFile, secondsToAnalyze * 1000, startSecond * 1000)
                                       .WithCustomConfigurations(
                                            fingerprintConfig =>
                                            {
                                                fingerprintConfig.Stride = samplesToSkip;
                                            },
                                            queryConfig =>
                                            {
                                                queryConfig.NumberOfLSHTables = hashTables;
                                                queryConfig.NumberOfMinHashesPerTable = hashKeys;
                                                queryConfig.ThresholdVotes = threshold;
                                            })
                                       .Query(cancellationTokenSource.Token)
                                       .ContinueWith(
                                            t =>
                                            {
                                                QueryResult queryResult = t.Result;
                                                if (!queryResult.IsSuccessful)
                                                {
                                                    verified++;
                                                    AddGridLine(new object[] { title + "-" + artist, "No candidates!", -1, false, 0, -1, -1, -1, -1, -1, -1, -1, 0 }, Color.Red);
                                                    return;
                                                }

                                                Track recognizedTrack = queryResult.BestMatch;
                                                recognized++;
                                                verified++;

                                                AddGridLine(
                                                    new object[]
                                                        {
                                                            title + "-" + artist, /*Actual title and artist*/
                                                            recognizedTrack.Title + "-" + recognizedTrack.Artist, /*Recognized Title Track Name*/
                                                            1, /*Position in the ordered list*/
                                                            actualTrack.Id == recognizedTrack.Id, /*Found?*/
                                                            -1, /*Average hamming distance*/
                                                            -1, -1, -1, -1, -1, -1, -1, -1, 0
                                                        },
                                                    Color.Empty);

                                                _tbResults.Text = ((float)recognized / verified).ToString(CultureInfo.InvariantCulture);
                                            },
                                           TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        public void ExtractCandidatesUsingSamples(float[] samples)
        {
            int recognized = 0, verified = 0;
            IStride samplesToSkip = queryStride;
            
            fingerprintQueryBuilder.BuildQuery()
                                   .From(samples)
                                   .WithCustomConfigurations(
                                        fingerprintConfig =>
                                        {
                                            fingerprintConfig.Stride = samplesToSkip;
                                        },
                                        queryConfig =>
                                        {
                                            queryConfig.NumberOfLSHTables = hashTables;
                                            queryConfig.NumberOfMinHashesPerTable = hashKeys;
                                            queryConfig.ThresholdVotes = threshold;
                                        })
                                  .Query(cancellationTokenSource.Token)
                                  .ContinueWith(
                                        t =>
                                        {
                                            var queryResult = t.Result;
                                            if (!queryResult.IsSuccessful)
                                            {
                                                AddGridLine(new object[] { string.Empty, "No candidates!", -1, false, 0, -1, -1, -1, -1, -1, -1, -1, 0 }, Color.Red);
                                                return;
                                            }

                                            Track recognizedTrack = queryResult.BestMatch;
                                            recognized++;
                                            verified++;

                                            AddGridLine(
                                                 new object[]
                                                    {
                                                        "Uknown", /*Actual title and artist*/
                                                        recognizedTrack.Title + "-" + recognizedTrack.Artist, /*Recognized Title Track Name*/
                                                        1, /*Position in the ordered list*/
                                                        true, /*Found?*/
                                                        -1, /*Average hamming distance*/
                                                        -1, -1, -1, -1, -1, -1, -1, -1, 0
                                                    },
                                                 Color.Empty);

                                            _tbResults.Text = ((float)recognized / verified).ToString(CultureInfo.InvariantCulture);
                                        },
                                      TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void AddGridLine(object[] parameters, Color color)
        {
            int index = _dgvResults.Rows.Add(parameters);
            _dgvResults.FirstDisplayedScrollingRowIndex = index;
            if (color != Color.Empty)
            {
                _dgvResults.Rows[index].DefaultCellStyle.BackColor = color;
            }
        }

        private void BtnStopClick(object sender, EventArgs e)
        {
            stopQuerying = true;
            cancellationTokenSource.Cancel();
            Close();
        }

        private void BtnExportClick(object sender, EventArgs e)
        {
            /*Export into CSV file*/
            WinUtils.ExportInExcel(_dgvResults, "Recognition Rate", _tbResults.Text, "Date", DateTime.Now);
        }
    }
}