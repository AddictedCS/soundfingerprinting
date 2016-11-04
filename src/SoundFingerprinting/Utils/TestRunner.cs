namespace SoundFingerprinting.Utils
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Math;
    using SoundFingerprinting.Query;
    using SoundFingerprinting.Strides;

    public delegate void PositiveNotFoundEvent(object sender, EventArgs e);

    public delegate void PositiveTrackFoundEvent(object sender, EventArgs e);

    public delegate void NegativeNotFoundEvent(object sender, EventArgs e);

    public delegate void NegativeFoundEvent(object sender, EventArgs e);

    internal class TestRunner
    {
        private readonly List<string> audioFileFilters = new List<string> { "*.mp3" };

        private readonly string[] scenarious;
        private readonly IModelService modelService;
        private readonly IAudioService audioService;
        private readonly ITagService tagService;
        private readonly IFingerprintCommandBuilder fcb;
        private readonly IQueryCommandBuilder qcb;
        private int samplesPerFingerprint = new DefaultFingerprintConfiguration().SamplesPerFingerprint;

        public TestRunner(
            string[] scenarious,
            IModelService modelService,
            IAudioService audioService,
            ITagService tagService,
            IFingerprintCommandBuilder fcb,
            IQueryCommandBuilder qcb,
            string pathToResultsFolder)
        {
            this.scenarious = scenarious;
            this.modelService = modelService;
            this.audioService = audioService;
            this.tagService = tagService;
            this.fcb = fcb;
            this.qcb = qcb;
        }

        public event PositiveNotFoundEvent PositiveNotFoundEvent;

        public event PositiveTrackFoundEvent PositiveFoundEvent;

        public event NegativeNotFoundEvent NegativeNotFoundEvent;

        public event NegativeFoundEvent NegativeFoundEvent;

        public void Run()
        {
            foreach (var scenario in scenarious)
            {
                string[] parameters = scenario.Split(',');
                RunTest(parameters);
            }
        }

        private void RunTest(string[] parameters)
        {
            string action = parameters[0];
            this.samplesPerFingerprint = new DefaultFingerprintConfiguration().SamplesPerFingerprint;
            switch (action)
            {
                case "Insert":
                    string folderWithSongs = parameters[1];
                    var stride = StrideUtils.ToStride(
                        parameters[2],
                        int.Parse(parameters[3]),
                        int.Parse(parameters[4]),
                        this.samplesPerFingerprint);
                    DeleteAll();
                    Insert(folderWithSongs, stride);
                    break;
                case "Run":
                    string folderWithPositives = parameters[1];
                    string folderWithNegatives = parameters[2];
                    var queryStride = StrideUtils.ToStride(
                        parameters[3],
                        int.Parse(parameters[4]),
                        int.Parse(parameters[5]),
                        this.samplesPerFingerprint);

                    int seconds = int.Parse(parameters[6]);
                    List<int> startAts = ToStartAts(parameters[7]);
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
                int trueNegatives = 0, truePositives = 0, falseNegatives = 0, falsePositives = 0, verified = 0;
                var sb = new StringBuilder();
                foreach (var positive in positives)
                {
                    verified++;
                    var tags = GetTagsFromFile(positive);
                    var actualTrack = GetActualTrack(tags);
                    var queryResult = BuildQuery(queryStride, seconds, positive, startAts[iteration]).Result;
                    if (!queryResult.IsSuccessful)
                    {
                        falseNegatives++;
                        var notFoundLine = this.GetNotFoundLine(tags);
                        AppendLine(sb, notFoundLine);
                        OnTruePositiveNotFound(
                            GetTestRunnerEventArgs(
                                truePositives, trueNegatives, falsePositives, falseNegatives, notFoundLine, verified));
                        continue;
                    }

                    var recognizedTrack = queryResult.BestMatch.Track;
                    bool isSuccessful = recognizedTrack.TrackReference.Equals(actualTrack.TrackReference);
                    if (isSuccessful)
                    {
                        truePositives++;
                    }

                    var foundLine = GetFoundLine(recognizedTrack, isSuccessful, queryResult);
                    AppendLine(sb, foundLine);
                    OnTrackFoundEvent(
                        GetTestRunnerEventArgs(
                            truePositives, trueNegatives, falsePositives, falseNegatives, foundLine, verified));
                }

                foreach (var negative in negatives)
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
                                truePositives, trueNegatives, falsePositives, falseNegatives, notFoundLine, verified));
                        continue;
                    }

                    var recognizedTrack = queryResult.BestMatch.Track;
                    falsePositives++;
                    var foundLine = GetFoundLine(recognizedTrack, false, queryResult);
                    AppendLine(sb, foundLine);
                    OnNegativeFoundEvent(
                        GetTestRunnerEventArgs(
                            truePositives, trueNegatives, falsePositives, falseNegatives, foundLine, verified));
                }
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
                    string.Format("{0}-{1}", tags.Title, tags.Artist), "No match found!", false, 0, 0, "No match found!",
                    string.Empty, string.Empty
                };
        }

        private object[] GetFoundLine(TrackData recognizedTrack, bool isSuccessful, QueryResult queryResult)
        {
            return new object[]
                {
                    string.Format("{0}-{1}", recognizedTrack.Title, recognizedTrack.Artist), isSuccessful,
                    queryResult.BestMatch.MatchedFingerprints, queryResult.AnalyzedTracksCount, recognizedTrack.ISRC,
                    queryResult.BestMatch.SequenceLength, queryResult.BestMatch.SequenceStart
                };
        }


        private Task<QueryResult> BuildQuery(IStride queryStride, int seconds, string positive, int startAt)
        {
            return this.qcb.BuildQueryCommand()
                .From(positive, seconds, startAt)
                .WithConfigs(fingerprintConfig => { fingerprintConfig.SpectrogramConfig.Stride = queryStride; }, queryConfig => { })
                .UsingServices(this.modelService, this.audioService)
                .Query();
        }

        private void AppendLine(StringBuilder sb, object[] line)
        {
             // TODO AddLine here
        }

        private void Insert(string folderWithSongs, IStride stride)
        {
            var allFiles = AllFiles(folderWithSongs);
            foreach (var file in allFiles)
            {
                var track = InsertTrack(file);

                var hashes = fcb.BuildFingerprintCommand()
                                .From(file)
                                .WithFingerprintConfig(config => { config.SpectrogramConfig.Stride = stride; })
                                .UsingServices(audioService)
                                .Hash()
                                .Result;

                modelService.InsertHashDataForTrack(hashes, track);
            }
        }

        private IModelReference InsertTrack(string file)
        {
            var tags = this.GetTagsFromFile(file);
            IModelReference track;
            lock (this)
            {
                if (this.IsDuplicateTrack(tags.ISRC, tags.Artist, tags.Title))
                {
                    throw new Exception(string.Format("Duplicate track found {0}. Currate your input data!", file));
                }

                var trackData = new TrackData(tags);
                track = this.modelService.InsertTrack(trackData);
            }

            return track;
        }

        private void DeleteAll()
        {
            var tracks = modelService.ReadAllTracks();
            foreach (var track in tracks)
            {
                modelService.DeleteTrack(track.TrackReference);
            }
        }

        private IEnumerable<string> AllFiles(string rootFolder)
        {
            return audioFileFilters.SelectMany(filter => Directory.GetFiles(rootFolder, filter, SearchOption.AllDirectories))
                                   .ToList();
        }

        private bool IsDuplicateTrack(string isrc, string artist, string title)
        {
            return modelService.ContainsTrack(isrc, artist, title);
        }

        private List<int> ToStartAts(string repeatIntervals)
        {
            var startAts = repeatIntervals.Split('|');
            return startAts.Select(int.Parse).ToList();
        }

        private TagInfo GetTagsFromFile(string path)
        {
            var tags = this.tagService.GetTagInfo(path);
            if (tags == null || !tags.IsTrackUniquelyIdentifiable())
            {
                throw new Exception(
                    string.Format(
                        "Could not extract tags from file {0}. Track does not contain enought data to be identified as unique. Currate your input data!",
                        path));
            }

            return tags;
        }

        private void OnTruePositiveNotFound(EventArgs e)
        {
            PositiveNotFoundEvent handler = this.PositiveNotFoundEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnTrackFoundEvent(EventArgs e)
        {
            PositiveTrackFoundEvent handler = this.PositiveFoundEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnNegativeNotFoundEvent(EventArgs e)
        {
            NegativeNotFoundEvent handler = this.NegativeNotFoundEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnNegativeFoundEvent(EventArgs e)
        {
            NegativeFoundEvent handler = this.NegativeFoundEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
