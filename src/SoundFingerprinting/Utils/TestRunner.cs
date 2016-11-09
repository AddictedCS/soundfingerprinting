namespace SoundFingerprinting.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Math;
    using SoundFingerprinting.Query;
    using SoundFingerprinting.Strides;

    public delegate void PositiveNotFoundEvent(object sender, EventArgs e);

    public delegate void PositiveFoundEvent(object sender, EventArgs e);

    public delegate void NegativeNotFoundEvent(object sender, EventArgs e);

    public delegate void NegativeFoundEvent(object sender, EventArgs e);

    public delegate void TestIterationFinishedEvent(object sender, EventArgs e);

    public delegate void OngoingTestRunnerActionEvent(object sender, EventArgs e);

    internal class TestRunner
    {
        private readonly ITestRunnerUtils utils;
        private readonly TestRunnerConfig testRunnerConfig = new TestRunnerConfig();
        private readonly string[] scenarious;
        private readonly IModelService modelService;
        private readonly IAudioService audioService;
        private readonly ITagService tagService;
        private readonly IFingerprintCommandBuilder fcb;
        private readonly IQueryCommandBuilder qcb;
        private readonly string pathToResultsFolder;

        private IStride lastInsertStride;
        private StringBuilder suite;

        public TestRunner(
            string[] scenarious,
            IModelService modelService,
            IAudioService audioService,
            ITagService tagService,
            IFingerprintCommandBuilder fcb,
            IQueryCommandBuilder qcb,
            string pathToResultsFolder)
            : this(DependencyResolver.Current.Get<ITestRunnerUtils>())
        {
            this.scenarious = scenarious;
            this.modelService = modelService;
            this.audioService = audioService;
            this.tagService = tagService;
            this.fcb = fcb;
            this.qcb = qcb;
            this.pathToResultsFolder = pathToResultsFolder;
        }

        internal TestRunner(ITestRunnerUtils utils)
        {
            this.utils = utils;
        }

        public event PositiveNotFoundEvent PositiveNotFoundEvent;

        public event PositiveFoundEvent PositiveFoundEvent;

        public event NegativeNotFoundEvent NegativeNotFoundEvent;

        public event NegativeFoundEvent NegativeFoundEvent;

        public event TestIterationFinishedEvent TestIterationFinishedEvent;

        public event OngoingTestRunnerActionEvent OngoingActionEvent;
        
        public void Run()
        {
            suite = TestRunnerWriter.StartSuite();
            foreach (var scenario in scenarious)
            {
                string[] parameters = scenario.Split(',');
                RunTest(parameters);
            }

            TestRunnerWriter.SaveSuiteResultsToFolder(suite, pathToResultsFolder);
            OnOngoingActionEvent(new TestRunnerOngoingEventArgs { Message = "All test scenarious finished!" });
        }

        private void RunTest(string[] parameters)
        {
            string action = parameters[0];
            switch (action)
            {
                case "Insert":
                    string folderWithSongs = parameters[1];
                    var stride = utils.ToStride(parameters[2], parameters[3], parameters[4], testRunnerConfig.SamplesPerFingerprint);
                    DeleteAll();
                    Insert(folderWithSongs, stride);
                    lastInsertStride = stride;
                    break;
                case "Run":
                    string folderWithPositives = parameters[1];
                    string folderWithNegatives = parameters[2];
                    var queryStride = utils.ToStride(
                        parameters[3], parameters[4], parameters[5], testRunnerConfig.SamplesPerFingerprint);
                    int seconds = int.Parse(parameters[6]);
                    var startAts = ToStartAts(parameters[7]);
                    RunTestScenario(folderWithPositives, folderWithNegatives,  queryStride, seconds, startAts);
                    break;
            }
        }

        private void RunTestScenario(string folderWithPositives, string folderWithNegatives, IStride queryStride, int seconds, List<int> startAts)
        {
            int iterations = startAts.Count;
            var positives = AllFiles(folderWithPositives);
            var negatives = AllFiles(folderWithNegatives);
            for (int iteration = 0; iteration < iterations; ++iteration)
            {
              OnOngoingActionEvent(
                    new TestRunnerOngoingEventArgs
                        {
                            Message =
                                string.Format(
                                    "Iteration {0} out of {1} with {2}, query seconds {3}",
                                    iteration + 1,
                                    iterations,
                                    queryStride,
                                    seconds)
                        });

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                int trueNegatives = 0, truePositives = 0, falseNegatives = 0, falsePositives = 0, verified = 0;
                List<int> truePositiveHammingDistance = new List<int>();
                List<int> falseNegativesHammingDistance = new List<int>();
                List<int> falsePositivesHammingDistance = new List<int>();
                var sb = TestRunnerWriter.StartTestIteration();
                Parallel.ForEach(
                    positives,
                    GetParallelOptions(),
                    positive =>
                        {
                            verified++;
                            var tags = GetTagsFromFile(positive);
                            var actualTrack = GetActualTrack(tags);
                            var queryResult = BuildQuery(queryStride, seconds, positive, startAts[iteration]).Result;
                            if (!queryResult.IsSuccessful)
                            {
                                falseNegatives++;
                                var notFoundLine = GetNotFoundLine(tags);
                                AppendLine(sb, notFoundLine);
                                this.OnPositiveNotFound(
                                    GetTestRunnerEventArgs(
                                        truePositives,
                                        trueNegatives,
                                        falsePositives,
                                        falseNegatives,
                                        notFoundLine,
                                        verified));
                                return;
                            }

                            var recognizedTrack = queryResult.BestMatch.Track;
                            bool isSuccessful = recognizedTrack.TrackReference.Equals(actualTrack.TrackReference);
                            if (isSuccessful)
                            {
                                truePositives++;
                                truePositiveHammingDistance.Add(queryResult.BestMatch.MatchedFingerprints);
                            }
                            else
                            {
                                falseNegatives++;
                                falseNegativesHammingDistance.Add(queryResult.BestMatch.MatchedFingerprints);
                            }

                            var foundLine = GetFoundLine(ToTrackString(actualTrack), recognizedTrack, isSuccessful, queryResult);
                            AppendLine(sb, foundLine);
                            OnPositiveFoundEvent(GetTestRunnerEventArgs(truePositives, trueNegatives, falsePositives, falseNegatives, foundLine, verified));
                        });

                Parallel.ForEach(
                    negatives,
                    GetParallelOptions(),
                    negative =>
                        {
                            verified++;
                            var tags = GetTagsFromFile(negative);
                            var queryResult = BuildQuery(queryStride, seconds, negative, startAts[iteration]).Result;
                            if (!queryResult.IsSuccessful)
                            {
                                trueNegatives++;
                                var notFoundLine = GetNotFoundLine(tags);
                                AppendLine(sb, notFoundLine);
                                OnNegativeNotFoundEvent(
                                    GetTestRunnerEventArgs(
                                        truePositives,
                                        trueNegatives,
                                        falsePositives,
                                        falseNegatives,
                                        notFoundLine,
                                        verified));
                                return;
                            }

                            var recognizedTrack = queryResult.BestMatch.Track;
                            falsePositivesHammingDistance.Add(queryResult.BestMatch.MatchedFingerprints);
                            falsePositives++;
                            var foundLine = GetFoundLine(ToTrackString(tags), recognizedTrack, false, queryResult);
                            AppendLine(sb, foundLine);
                            OnNegativeFoundEvent(
                                GetTestRunnerEventArgs(
                                    truePositives, trueNegatives, falsePositives, falseNegatives, foundLine, verified));
                        });

                stopwatch.Stop();
                var fscore = new FScore(truePositives, trueNegatives, falsePositives, falseNegatives);
                var stats = HammingDistanceResultStatistics.From(
                    truePositiveHammingDistance,
                    falseNegativesHammingDistance,
                    falsePositivesHammingDistance,
                    testRunnerConfig.Percentiles);
                TestRunnerWriter.FinishTestIteration(sb, fscore, stats, stopwatch.ElapsedMilliseconds);
                TestRunnerWriter.SaveTestIterationToFolder(
                    sb, pathToResultsFolder, queryStride, this.GetInsertMetadata(), seconds, startAts[iteration]);

                var finishedTestIteration = GetTestRunnerEventArgsForFinishedTestIteration(
                    queryStride, seconds, startAts, fscore, stats, iteration, stopwatch, verified);
                OnTestIterationFinishedEvent(finishedTestIteration);
                TestRunnerWriter.AppendLine(suite, finishedTestIteration.RowWithDetails);
            }
        }

        private ParallelOptions GetParallelOptions()
        {
            return new ParallelOptions { MaxDegreeOfParallelism = 2 };
        }

        private TestRunnerEventArgs GetTestRunnerEventArgsForFinishedTestIteration(IStride queryStride, int seconds, List<int> startAts, FScore fscore, HammingDistanceResultStatistics statistics, int iteration, Stopwatch stopwatch, int verified)
        {
            return new TestRunnerEventArgs
                {
                    FScore = fscore,
                    RowWithDetails =
                        new object[]
                            {
                                this.GetInsertMetadata(), queryStride.ToString(), seconds, startAts[iteration],
                                fscore.Precision, fscore.Recall, fscore.F1, 
                                statistics.TruePositiveInfo,
                                statistics.TruePositivePercentileInfo,
                                statistics.FalseNegativesInfo,
                                statistics.FalseNegativesPercentileInfo,
                                statistics.FalsePositivesInfo,
                                statistics.FalsePositivesPercentileInfo,
                                (double)stopwatch.ElapsedMilliseconds / 1000
                            },
                    Verified = verified
                };
        }

        private string ToTrackString(TrackData actualTrack)
        {
            return string.Format("{0}-{1}", actualTrack.Artist, actualTrack.Title);
        }

        private string ToTrackString(TagInfo tag)
        {
            return string.Format("{0}-{1}", tag.Artist, tag.Title);
        }

        private string GetInsertMetadata()
        {
            return this.lastInsertStride != null ? this.lastInsertStride.ToString() : string.Empty;
        }

        private void AppendLine(StringBuilder sb, object[] objects)
        {
            TestRunnerWriter.AppendLine(sb, objects);
        }

        private TestRunnerEventArgs GetTestRunnerEventArgs(int truePositives, int trueNegatives, int falsePositives, int falseNegatives, object[] line, int verified)
        {
            return new TestRunnerEventArgs
                {
                    FScore = new FScore(truePositives, trueNegatives, falsePositives, falseNegatives),
                    RowWithDetails = line,
                    Verified = verified
                };
        }

        private TrackData GetActualTrack(TagInfo tags)
        {
            return !string.IsNullOrEmpty(tags.ISRC)
                       ? modelService.ReadTrackByISRC(tags.ISRC)
                       : modelService.ReadTrackByArtistAndTitleName(tags.Artist, tags.Title).FirstOrDefault();
        }

        private object[] GetNotFoundLine(TagInfo tags)
        {
            return new object[]
                {
                    ToTrackString(tags), "No match found!", false, 0, 0, 0, string.Empty, string.Empty 
                };
        }

        private object[] GetFoundLine(string actualTrack, TrackData recognizedTrack, bool isSuccessful, QueryResult queryResult)
        {
            return new object[]
                {
                    actualTrack, ToTrackString(recognizedTrack), isSuccessful,
                    queryResult.BestMatch.MatchedFingerprints, queryResult.AnalyzedTracksCount, queryResult.AnalyzedSubFingerprintsCount,
                    queryResult.BestMatch.SequenceLength, queryResult.BestMatch.SequenceStart
                };
        }


        private Task<QueryResult> BuildQuery(IStride queryStride, int seconds, string positive, int startAt)
        {
            return this.qcb.BuildQueryCommand()
                .From(positive, seconds, startAt)
                .WithConfigs(fingerprintConfig => { fingerprintConfig.SpectrogramConfig.Stride = queryStride; }, queryConfig => { })
                .UsingServices(modelService, audioService)
                .Query();
        }

        private void Insert(string folderWithSongs, IStride stride)
        {
            var allFiles = AllFiles(folderWithSongs).ToList();
            int inserted = 0;
            Parallel.ForEach(
                allFiles,
                GetParallelOptions(),
                file =>
                    {
                        OnOngoingActionEvent(new TestRunnerOngoingEventArgs { Message = string.Format("Inserting tracks {0} out of {1}. Track {2}", inserted++, allFiles.Count, System.IO.Path.GetFileNameWithoutExtension(file)) });
                        var track = InsertTrack(file);
                        var hashes = fcb.BuildFingerprintCommand()
                                        .From(file)
                                        .WithFingerprintConfig(config => { config.SpectrogramConfig.Stride = stride; })
                                        .UsingServices(audioService)
                                        .Hash()
                                        .Result;

                        modelService.InsertHashDataForTrack(hashes, track);
                    });
        }

        private IModelReference InsertTrack(string file)
        {
            var tags = GetTagsFromFile(file);
            var trackData = new TrackData(tags);
            return modelService.InsertTrack(trackData);
        }

        private void DeleteAll()
        {
            var tracks = modelService.ReadAllTracks();
            int deleted = 0;
            foreach (var track in tracks)
            {
                modelService.DeleteTrack(track.TrackReference);
                OnOngoingActionEvent(
                    new TestRunnerOngoingEventArgs
                        {
                            Message = string.Format("Deleted {0} out of {1} tracks from storage", deleted++, tracks.Count)
                        });
            }
        }

        private IEnumerable<string> AllFiles(string rootFolder)
        {
            return utils.ListFiles(rootFolder, testRunnerConfig.AudioFileFilters);
        }

        private bool IsDuplicateTrack(string isrc, string artist, string title)
        {
            return modelService.ContainsTrack(isrc, artist, title);
        }

        private List<int> ToStartAts(string startAtSeconds)
        {
            return utils.ParseInts(startAtSeconds, testRunnerConfig.StartAtsSeparator);
        }

        private TagInfo GetTagsFromFile(string path)
        {
            var tags = tagService.GetTagInfo(path);
            if (tags == null || !tags.IsTrackUniquelyIdentifiable())
            {
                throw new Exception(
                    string.Format(
                        "Could not extract tags from file {0}. Track does not contain enought data to be identified as unique. Currate your input data!",
                        path));
            }

            return tags;
        }

        private void OnPositiveNotFound(EventArgs e)
        {
            PositiveNotFoundEvent handler = PositiveNotFoundEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnPositiveFoundEvent(EventArgs e)
        {
            PositiveFoundEvent handler = PositiveFoundEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnNegativeNotFoundEvent(EventArgs e)
        {
            NegativeNotFoundEvent handler = NegativeNotFoundEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnNegativeFoundEvent(EventArgs e)
        {
            NegativeFoundEvent handler = NegativeFoundEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnTestIterationFinishedEvent(EventArgs e)
        {
            TestIterationFinishedEvent handler = this.TestIterationFinishedEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnOngoingActionEvent(EventArgs e)
        {
            OngoingTestRunnerActionEvent handler = this.OngoingActionEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
