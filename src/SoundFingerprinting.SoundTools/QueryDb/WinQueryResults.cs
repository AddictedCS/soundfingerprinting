namespace SoundFingerprinting.SoundTools.QueryDb
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Dao;
    using SoundFingerprinting.Dao.Entities;
    using SoundFingerprinting.Query;
    using SoundFingerprinting.SoundTools.Properties;
    using SoundFingerprinting.Strides;

    public partial class WinQueryResults : Form
    {
        private const int MinTrackLength = 5;
        private const int MaxTrackLength = 60 * 20;

        private const string ColSongName = "SongNameTitle";
        private const string ColResultName = "ResultSongNameTitle";
        private const string ColResult = "Result";
        private const string ColHammingAvg = "HammingAvg";
        private const string ColNumberOfCandidates = "NumberOfCandidates";
        private const string ColISRC = "ISRC";


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
             _dgvResults.Columns.Add(ColResult, "ISRC Match");
            _dgvResults.Columns[ColResult].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvResults.Columns.Add(ColHammingAvg, "Hamming Distance");
            _dgvResults.Columns[ColHammingAvg].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvResults.Columns.Add(ColNumberOfCandidates, "Number of candidates");
            _dgvResults.Columns[ColNumberOfCandidates].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvResults.Columns.Add(ColISRC, "Result ISRC");
            _dgvResults.Columns[ColISRC].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            
            // ReSharper restore PossibleNullReferenceException
            _btnExport.Enabled = false;
        }

        public void ExtractCandidatesWithMinHashAlgorithm(List<string> fileList)
        {
            int recognized = 0, verified = 0;
            IStride samplesToSkip = queryStride;
            Action<object[], Color> actionInterface = AddGridLine;
            ParallelOptions parallelOptions = new ParallelOptions
                { 
                    MaxDegreeOfParallelism = 4, CancellationToken = cancellationTokenSource.Token 
                };

            Task.Factory.StartNew(() =>
                {
                    Parallel.For(
                        0,
                        fileList.Count,
                        parallelOptions,
                        (index, loopState) =>
                        {
                            if (stopQuerying)
                            {
                                cancellationTokenSource.Cancel();
                                loopState.Stop();
                            }

                            string pathToFile = fileList[index]; /*Path to song to recognize*/
                            TagInfo tags = tagService.GetTagInfo(pathToFile); // Get Tags from file

                            if (tags == null || tags.IsEmpty)
                            {
                                Invoke(actionInterface, new object[] { string.Empty, pathToFile }, Color.Red);
                                return;
                            }

                            string artist = string.IsNullOrEmpty(tags.Artist)
                                                ? Path.GetFileNameWithoutExtension(pathToFile)
                                                : tags.Artist; // Artist
                            string title = string.IsNullOrEmpty(tags.Title)
                                               ? Path.GetFileNameWithoutExtension(pathToFile)
                                               : tags.Title; // Title
                            string isrc = tags.ISRC;
                            double duration = tags.Duration; // Duration

                            // Check whether the duration is ok
                            if (duration < MinTrackLength || duration > MaxTrackLength || secondsToAnalyze > duration)
                            {
                                // Duration too small
                                Invoke(actionInterface, new object[] { "BAD DURATION", pathToFile }, Color.Red);
                                return;
                            }

                            Track actualTrack = null;
                            if (!string.IsNullOrEmpty(isrc))
                            {
                                actualTrack = modelService.ReadTrackByISRC(isrc);
                            }

                            var queryResult =
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
                                                        .Result;

                            if (cancellationTokenSource.IsCancellationRequested)
                            {
                                return;
                            }

                            if (!queryResult.IsSuccessful)
                            {
                                Invoke(
                                    actionInterface,
                                    new object[]
                                    {
                                        title + "-" + artist, "No match found!", false, -1, -1, "No match found!" 
                                    },
                                    Color.Red);

                                if (actualTrack != null)
                                {
                                    verified++;
                                }

                                return;
                            }

                            verified++;
                            Track recognizedTrack = queryResult.BestMatch;
                            bool isSuccessful = actualTrack == null || recognizedTrack.Id == actualTrack.Id;
                            if (isSuccessful)
                            {
                                recognized++;
                            }

                            Invoke(
                                actionInterface,
                                new object[]
                                {
                                    title + "-" + artist, recognizedTrack.Title + "-" + recognizedTrack.Artist,
                                    isSuccessful, queryResult.Similarity, queryResult.NumberOfCandidates,
                                    recognizedTrack.ISRC
                                },
                                Color.Empty);

                            Invoke(new Action(
                                    () =>
                                    {
                                        _tbResults.Text =
                                            ((float)recognized / verified).ToString(CultureInfo.InvariantCulture);
                                    }));
                        });
                }).ContinueWith(task =>
                    { 
                        MessageBox.Show("Finished!", "Finished query!"); 
                    });
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