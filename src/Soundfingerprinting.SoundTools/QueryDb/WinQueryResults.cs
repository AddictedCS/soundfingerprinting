namespace Soundfingerprinting.SoundTools.QueryDb
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    using Soundfingerprinting.Audio;
    using Soundfingerprinting.Dao;
    using Soundfingerprinting.Dao.Entities;
    using Soundfingerprinting.Query;
    using Soundfingerprinting.SoundTools.Properties;
    using Soundfingerprinting.Strides;

    public partial class WinQueryResults : Form
    {
        private const int MinTrackLength = 20;
        private const int MaxTrackLength = 60 * 20;

        private const string ColSongName = "SongNameTitle";
        private const string ColResultName = "ResultSongNameTitle";
        private const string ColResult = "Result";
        private const string ColHammingAvg = "HammingAvg";
        private const string ColElapsedTime = "ElapsedTime";

        private readonly int hashKeys;
        private readonly int hashTables;
        private readonly int secondsToAnalyze;
        private readonly int startSecond;
        private readonly int threshold;

        private readonly IStride queryStride;
        private readonly ITagService tagService;
        private readonly IModelService modelService;
        private readonly IFingerprintQueryBuilder fingerprintQueryBuilder;
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private bool stopQuerying;
        
        public WinQueryResults(
            int secondsToAnalyze,
            int startSecond,
            int hashTables,
            int hashKeys,
            int threshold,
            IStride queryStride,
            ITagService tagService,
            IModelService modelService,
            IFingerprintQueryBuilder fingerprintQueryBuilder)
        {
            InitializeComponent(); 
            Icon = Resources.Sound;
            this.secondsToAnalyze = secondsToAnalyze;
            this.startSecond = startSecond;
            this.queryStride = queryStride;
            this.hashTables = hashTables;
            this.hashKeys = hashKeys;
            this.threshold = threshold;
            this.tagService = tagService;
            this.modelService = modelService;
            this.fingerprintQueryBuilder = fingerprintQueryBuilder;

            // ReSharper disable PossibleNullReferenceException
            _dgvResults.Columns.Add(ColSongName, "Initial Song");
            _dgvResults.Columns[ColSongName].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvResults.Columns.Add(ColResultName, "Result Song");
            _dgvResults.Columns[ColResultName].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
             _dgvResults.Columns.Add(ColResult, "Result");
            _dgvResults.Columns[ColResult].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvResults.Columns.Add(ColHammingAvg, "Hamming Distance");
            _dgvResults.Columns[ColHammingAvg].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
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
                    AddGridLine(new object[] { title + "-" + artist, "No such song in the database!", false, -1, -1 }, Color.Red);
                    continue;
                }

                fingerprintQueryBuilder.BuildQuery()
                                       .From(pathToFile, secondsToAnalyze, startSecond)
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
                                                if (cancellationTokenSource.IsCancellationRequested)
                                                {
                                                    return;
                                                }

                                                verified++;
                                                QueryResult queryResult = t.Result;
                                                if (!queryResult.IsSuccessful)
                                                {
                                                    AddGridLine(new object[] { title + "-" + artist, "No candidates!", false, -1, -1 }, Color.Red);
                                                    return;
                                                }

                                                Track recognizedTrack = queryResult.BestMatch;
                                                if (recognizedTrack.Id == actualTrack.Id)
                                                {
                                                    recognized++;
                                                }

                                                AddGridLine(
                                                    new object[]
                                                        {
                                                            title + "-" + artist, recognizedTrack.Title + "-" + recognizedTrack.Artist, actualTrack.Id == recognizedTrack.Id, -1, -1
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
                                                AddGridLine(new object[] { string.Empty, "No candidates!", false, -1, -1 }, Color.Red);
                                                return;
                                            }

                                            Track recognizedTrack = queryResult.BestMatch;
                                            recognized++;
                                            verified++;

                                            AddGridLine(
                                                 new object[]
                                                    {
                                                        "Uknown Song",
                                                        recognizedTrack.Title + "-" + recognizedTrack.Artist,
                                                        true, -1, -1
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
            WinUtils.ExportInExcel(_dgvResults, "Recognition Rate", _tbResults.Text, "Date", DateTime.Now);
        }
    }
}