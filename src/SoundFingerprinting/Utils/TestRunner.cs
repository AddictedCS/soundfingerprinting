namespace SoundFingerprinting.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Math;
    using SoundFingerprinting.Query;
    using SoundFingerprinting.Strides;

    public delegate void TestRunnerEvent(object sender, EventArgs e);

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

        public event TestRunnerEvent PositiveNotFoundEvent;

        public event TestRunnerEvent PositiveFoundEvent;

        public event TestRunnerEvent NegativeNotFoundEvent;

        public event TestRunnerEvent NegativeFoundEvent;

        public event TestRunnerEvent TestIterationFinishedEvent;

        public event TestRunnerEvent OngoingActionEvent;
        
        public void Run()
        {
            suite = TestRunnerWriter.StartSuite();
            foreach (var scenario in scenarious)
            {
                string[] parameters = scenario.Split(',');
                RunTest(parameters);
            }

            TestRunnerWriter.SaveSuiteResultsToFolder(suite, pathToResultsFolder);
            OnTestRunnerEvent(OngoingActionEvent, new TestRunnerOngoingEventArgs { Message = "All test scenarious finished!" });
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
            var positives = AllFiles(folderWithPositives).ToList();
            var negatives = AllFiles(folderWithNegatives).ToList();
            for (int iteration = 0; iteration < iterations; ++iteration)
            {
                OnTestRunnerEvent(
                    OngoingActionEvent,
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
                int currentIteration = iteration;
                Parallel.ForEach(
                    positives,
                    positive =>
                        {
                            Interlocked.Increment(ref verified);
                            var tags = GetTagsFromFile(positive);
                            var actualTrack = GetActualTrack(tags);
                            var queryResult = BuildQuery(queryStride, seconds, positive, startAts[currentIteration]).Result;
                            if (!queryResult.ContainsMatches)
                            {
                                Interlocked.Increment(ref falseNegatives);
                                var notFoundLine = GetNotFoundLine(tags);
                                AppendLine(sb, notFoundLine);
                                OnTestRunnerEvent(
                                    PositiveNotFoundEvent,
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
                                Interlocked.Increment(ref truePositives);
                                truePositiveHammingDistance.Add(queryResult.BestMatch.HammingSimilaritySum);
                            }
                            else
                            {
                                Interlocked.Increment(ref falsePositives);
                                falseNegativesHammingDistance.Add(queryResult.BestMatch.HammingSimilaritySum);
                            }

                            var foundLine = GetFoundLine(ToTrackString(actualTrack), recognizedTrack, isSuccessful, queryResult);
                            AppendLine(sb, foundLine);
                            OnTestRunnerEvent(PositiveFoundEvent, GetTestRunnerEventArgs(truePositives, trueNegatives, falsePositives, falseNegatives, foundLine, verified));
                        });

                Parallel.ForEach(
                    negatives,
                    negative =>
                        {
                            Interlocked.Increment(ref verified);
                            var tags = GetTagsFromFile(negative);
                            var queryResult = BuildQuery(queryStride, seconds, negative, startAts[currentIteration]).Result;
                            if (!queryResult.ContainsMatches)
                            {
                                Interlocked.Increment(ref trueNegatives);
                                var notFoundLine = GetNotFoundLine(tags);
                                AppendLine(sb, notFoundLine);
                                OnTestRunnerEvent(
                                    NegativeNotFoundEvent,
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
                            falsePositivesHammingDistance.Add(queryResult.BestMatch.HammingSimilaritySum);
                            Interlocked.Increment(ref falsePositives);
                            var foundLine = GetFoundLine(ToTrackString(tags), recognizedTrack, false, queryResult);
                            AppendLine(sb, foundLine);
                            OnTestRunnerEvent(
                                NegativeFoundEvent,
                                GetTestRunnerEventArgs(truePositives, trueNegatives, falsePositives, falseNegatives, foundLine, verified));
                        });

                stopwatch.Stop();
                var fscore = new FScore(truePositives, trueNegatives, falsePositives, falseNegatives);
                var stats = HammingDistanceResultStatistics.From(
                    truePositiveHammingDistance,
                    falseNegativesHammingDistance,
                    falsePositivesHammingDistance,
                    testRunnerConfig.Percentiles);
                TestRunnerWriter.FinishTestIteration(sb, fscore, stats, stopwatch.ElapsedMilliseconds);
                TestRunnerWriter.SaveTestIterationToFolder(sb, pathToResultsFolder, queryStride, GetInsertMetadata(), seconds, startAts[currentIteration]);

                var finishedTestIteration = GetTestRunnerEventArgsForFinishedTestIteration(queryStride, seconds, startAts, fscore, stats, iteration, stopwatch, verified);
                OnTestRunnerEvent(TestIterationFinishedEvent, finishedTestIteration);
                TestRunnerWriter.AppendLine(suite, finishedTestIteration.RowWithDetails);
            }
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
            return new object[] { ToTrackString(tags), "No match found!", false, 0, 0, 0, 0, 0 };
        }

        private object[] GetFoundLine(string actualTrack, TrackData recognizedTrack, bool isSuccessful, QueryResult queryResult)
        {
            var bestMatch = queryResult.BestMatch;
            return new object[]
                {
                    actualTrack, ToTrackString(recognizedTrack), isSuccessful,
                    bestMatch.HammingSimilaritySum, bestMatch.Confidence,
                    bestMatch.Coverage,
                    bestMatch.QueryMatchLength, bestMatch.TrackStartsAt
                };
        }

        private Task<QueryResult> BuildQuery(IStride queryStride, int seconds, string positive, int startAt)
        {
            return this.qcb.BuildQueryCommand()
                .From(positive, seconds, startAt)
                .WithFingerprintConfig(fingerprintConfig => { fingerprintConfig.SpectrogramConfig.Stride = queryStride; })
                .UsingServices(modelService, audioService)
                .Query();
        }

        private void Insert(string folderWithSongs, IStride stride)
        {
            var allFiles = AllFiles(folderWithSongs).ToList();
            int inserted = 0;
            Parallel.ForEach(
                allFiles,
                file =>
                    {
                        OnTestRunnerEvent(
                            OngoingActionEvent,
                            new TestRunnerOngoingEventArgs
                                {
                                    Message = string.Format(
                                            "Inserting tracks {0} out of {1}. Track {2}",
                                            Interlocked.Increment(ref inserted),
                                            allFiles.Count,
                                            System.IO.Path.GetFileNameWithoutExtension(file))
                                });
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
                OnTestRunnerEvent(
                    OngoingActionEvent,
                    new TestRunnerOngoingEventArgs
                        {
                            Message = string.Format("Deleted {0} out of {1} tracks from storage", Interlocked.Increment(ref deleted), tracks.Count)
                        });
            }
        }

        private IEnumerable<string> AllFiles(string rootFolder)
        {
            return utils.ListFiles(rootFolder, testRunnerConfig.AudioFileFilters);
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

        private void OnTestRunnerEvent(TestRunnerEvent runnerEvent, EventArgs args)
        {
            var handler = runnerEvent;
            if (handler != null)
            {
                handler(this, args);
            }
        }
    }
}
