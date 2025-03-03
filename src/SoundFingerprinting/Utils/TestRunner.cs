namespace SoundFingerprinting.Utils
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.DAO.Data;
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
            : this(new TestRunnerUtils())
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
                    var stride = utils.ToStride(parameters[2], parameters[3], parameters[4]);
                    DeleteAll();
                    var sb = TestRunnerWriter.StartInsert();
                    Insert(folderWithSongs, stride, sb);
                    TestRunnerWriter.SaveInsertDataToFolder(sb, pathToResultsFolder, stride);
                    lastInsertStride = stride;
                    break;
                case "Run":
                    string folderWithPositives = parameters[1];
                    string folderWithNegatives = parameters[2];
                    var queryStride = utils.ToStride(parameters[3], parameters[4], parameters[5]);
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
                OnTestRunnerEvent(OngoingActionEvent, new TestRunnerOngoingEventArgs
                        {
                            Message = $"Iteration {iteration + 1} out of {iterations} with {queryStride}, query seconds {seconds}"
                        });

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                int trueNegatives = 0, truePositives = 0, falseNegatives = 0, falsePositives = 0, verified = 0;
                var truePositiveHammingDistance = new ConcurrentBag<int>();
                var falseNegativesHammingDistance = new ConcurrentBag<int>();
                var falsePositivesHammingDistance = new ConcurrentBag<int>();
                var sb = TestRunnerWriter.StartTestIteration();
                int currentIteration = iteration;
                int startAt = startAts[currentIteration];
                Parallel.ForEach(
                    positives,
                    positive =>
                        {
                            Interlocked.Increment(ref verified);
                            var tags = GetTagsFromFile(positive);
                            var actualTrack = GetActualTrack(tags);
                            var queryResult = BuildQuery(queryStride, seconds, positive, startAt).Result;
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
                            bool isSuccessful = recognizedTrack.Id.Equals(actualTrack.Id);
                            if (isSuccessful)
                            {
                                Interlocked.Increment(ref truePositives);
                                truePositiveHammingDistance.Add((int)queryResult.BestMatch.Score);
                            }
                            else
                            {
                                Interlocked.Increment(ref falsePositives);
                                falseNegativesHammingDistance.Add((int)queryResult.BestMatch.Score);
                            }

                            var foundLine = GetFoundLine(ToTrackString(actualTrack.Artist, actualTrack.Title), recognizedTrack, isSuccessful, queryResult);
                            AppendLine(sb, foundLine);
                            OnTestRunnerEvent(PositiveFoundEvent, GetTestRunnerEventArgs(truePositives, trueNegatives, falsePositives, falseNegatives, foundLine, verified));
                        });

                Parallel.ForEach(
                    negatives,
                    negative =>
                        {
                            Interlocked.Increment(ref verified);
                            var tags = GetTagsFromFile(negative);
                            var queryResult = BuildQuery(queryStride, seconds, negative, startAt).Result;
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
                            falsePositivesHammingDistance.Add((int)queryResult.BestMatch.Score);
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
                TestRunnerWriter.SaveTestIterationToFolder(sb, pathToResultsFolder, queryStride, GetInsertMetadata(), seconds, startAt);

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
                                GetInsertMetadata(), queryStride.ToString(), seconds, startAts[iteration],
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

        private string ToTrackString(string artist, string title)
        {
            return $"{artist}-{title}";
        }

        private string ToTrackString(TagInfo tag)
        {
            return $"{tag.Artist}-{tag.Title}";
        }

        private string GetInsertMetadata()
        {
            return lastInsertStride != null ? lastInsertStride.ToString() : string.Empty;
        }

        private void AppendLine(StringBuilder sb, object[] objects)
        {
            lock (this)
            {
                TestRunnerWriter.AppendLine(sb, objects);
            }
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

        private TrackInfo GetActualTrack(TagInfo tags)
        {
            return modelService.ReadTrackById(tags.ISRC);
        }

        private object[] GetNotFoundLine(TagInfo tags)
        {
            return new object[] { ToTrackString(tags), "No match found!", false, 0, 0, 0, 0, 0 };
        }

        private object[] GetFoundLine(string actualTrack, TrackData recognizedTrack, bool isSuccessful, QueryResult queryResult)
        {
            var bestMatch = queryResult.BestMatch;
            return
            [
                actualTrack, ToTrackString(recognizedTrack.Artist, recognizedTrack.Title), isSuccessful,
                    bestMatch.Score, bestMatch.Confidence,
                    bestMatch.Coverage.TrackRelativeCoverage,
                    bestMatch.TrackCoverageWithPermittedGapsLength, bestMatch.TrackStartsAt
            ];
        }

        private async Task<QueryResult> BuildQuery(IStride queryStride, int seconds, string positive, int startAt)
        {
            var (audio, _) = await qcb.BuildQueryCommand()
                .From(positive, seconds, startAt)
                .WithQueryConfig(queryConfig =>
                    {
                        queryConfig.Audio.Stride = queryStride;
                        return queryConfig;
                    })
                .UsingServices(modelService, audioService)
                .Query();
            return audio!;
        }

        private void Insert(string folderWithSongs, IStride stride, StringBuilder sb)
        {
            var allFiles = AllFiles(folderWithSongs);
            int inserted = 0;
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            Parallel.ForEach(
                allFiles,
                file =>
                    {
                        OnTestRunnerEvent(
                            OngoingActionEvent,
                            new TestRunnerOngoingEventArgs
                                {
                                    Message = $"Inserting tracks {Interlocked.Increment(ref inserted)} out of {allFiles.Count}. Track {System.IO.Path.GetFileNameWithoutExtension(file)}"
                                });
                        var track = GetTrack(file);
                        var hashes = fcb.BuildFingerprintCommand()
                                        .From(file)
                                        .WithFingerprintConfig(
                                            config =>
                                            {
                                                config.Audio.Stride = stride;
                                                return config;
                                            })
                                        .UsingServices(audioService)
                                        .Hash()
                                        .Result;

                        modelService.Insert(track, hashes);
                    });
 
            stopWatch.Stop();
            sb.AppendLine($"{inserted},{stopWatch.ElapsedMilliseconds / 1000}");
        }

        private TrackInfo GetTrack(string file)
        {
            var tags = GetTagsFromFile(file);
            return new TrackInfo(tags.ISRC, tags.Artist, tags.Title);
        }

        private void DeleteAll()
        {
            var trackIds = modelService.GetTrackIds().ToList();
            int deleted = 0;
            foreach (var id in trackIds)
            {
                modelService.DeleteTrack(id);
                OnTestRunnerEvent(OngoingActionEvent,
                    new TestRunnerOngoingEventArgs
                        {
                            Message = $"Deleted {Interlocked.Increment(ref deleted)} out of {trackIds.Count} tracks from storage"
                        });
            }
        }

        private ConcurrentBag<string> AllFiles(string rootFolder)
        {
            return new ConcurrentBag<string>(utils.ListFiles(rootFolder, testRunnerConfig.AudioFileFilters));
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
                throw new Exception($"Could not extract tags from file {path}. Track does not contain enought data to be identified as unique. Currate your input data!");
            }

            return tags;
        }

        private void OnTestRunnerEvent(TestRunnerEvent runnerEvent, EventArgs args)
        {
            runnerEvent?.Invoke(this, args);
        }
    }
}
