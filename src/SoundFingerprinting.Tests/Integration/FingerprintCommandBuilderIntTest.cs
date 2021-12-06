namespace SoundFingerprinting.Tests.Integration
{
    using Audio;
    using Builder;
    using Configuration;
    using InMemory;
    using NUnit.Framework;
    using SoundFingerprinting.Data;
    using Strides;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using SoundFingerprinting.Content;
    using SoundFingerprinting.Query;

    [TestFixture]
    public class FingerprintCommandBuilderIntTest : IntegrationWithSampleFilesTest
    {
        private readonly SoundFingerprintingAudioService audioService = new ();

        [Test]
        public async Task CreateFingerprintsFromFileAndAssertNumberOfFingerprints()
        {
            var (fingerprints, _) = await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(PathToWav)
                .Hash();

            var config = new DefaultFingerprintConfiguration();
            int samples = (int)(fingerprints.DurationInSeconds * config.SampleRate);
            int expectedFingerprints = (int)Math.Round(((double)(samples - (config.SamplesPerFingerprint + config.SpectrogramConfig.WdftSize)) / config.Stride.NextStride));

            Assert.AreEqual(expectedFingerprints, fingerprints.Count);
        }

        [Test]
        public async Task CreateFingerprintsWithModifiedStride()
        {
            var (fingerprints, _) = await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(PathToWav)
                .WithFingerprintConfig(config =>
                {
                    config.Audio.Stride = new IncrementalStaticStride(config.Audio.SamplesPerFingerprint);
                    return config;
                })
                .Hash();

            var config = new DefaultFingerprintConfiguration();
            int expected = (int)((fingerprints.DurationInSeconds * config.SampleRate) / config.SamplesPerFingerprint);
            Assert.AreEqual(expected, fingerprints.Count); 
        }

        [Test]
        public async Task ShouldCreateFingerprintsInsertThenQueryAndGetTheRightResult()
        {
            const int secondsToProcess = 8;
            const int startAtSecond = 2;
            var track = new TrackInfo("id", "title", "artist");

            var fingerprints = await FingerprintCommandBuilder.Instance
                                            .BuildFingerprintCommand()
                                            .From(PathToWav)
                                            .Hash();

            var modelService = new InMemoryModelService();
            modelService.Insert(track, fingerprints);

            var queryResult = await QueryCommandBuilder.Instance
                               .BuildQueryCommand()
                               .From(PathToWav, secondsToProcess, startAtSecond)
                               .UsingServices(modelService)
                               .Query();

            Assert.IsTrue(queryResult.ContainsMatches);
            Assert.AreEqual(1, queryResult.ResultEntries.Count());
            Assert.IsNotNull(queryResult.BestMatch);
            var (bestMatch, _) = queryResult.BestMatch;
            Assert.AreEqual("id", bestMatch.Track.Id);
            Assert.IsTrue(bestMatch.TrackCoverageWithPermittedGapsLength > secondsToProcess - 3, $"QueryCoverageSeconds:{bestMatch.QueryLength}");
            Assert.AreEqual(startAtSecond, Math.Abs(bestMatch.TrackStartsAt), 0.1d);
            Assert.IsTrue(bestMatch.Confidence > 0.7, $"Confidence:{bestMatch.Confidence}");
        }

        [Test]
        public async Task CreateFingerprintsFromFileAndFromAudioSamplesAndGetTheSameResultTest()
        {
            const int secondsToProcess = 8;
            const int startAtSecond = 1;

            var samples = audioService.ReadMonoSamplesFromFile(PathToWav, 5512, secondsToProcess, startAtSecond);

            var (h1, _) = await FingerprintCommandBuilder.Instance
                                        .BuildFingerprintCommand()
                                        .From(PathToWav, secondsToProcess, startAtSecond)
                                        .UsingServices(audioService)
                                        .Hash();

            var (h2, _) = await FingerprintCommandBuilder.Instance
                                        .BuildFingerprintCommand()
                                        .From(samples)
                                        .Hash();

            AssertHashDatasAreTheSame(h1, h2);
        }

        [Test]
        public async Task CheckFingerprintCreationAlgorithmTest()
        {
            var config = new DefaultFingerprintConfiguration();
            var format = WaveFormat.FromFile(PathToWav);
            var list = await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(PathToWav)
                .WithFingerprintConfig(configuration =>
                      {
                          configuration.Audio.Stride = new IncrementalStaticStride(8192);
                          return configuration;
                      })
                .UsingServices(audioService)
                .Hash();

            int bytesPerSample = format.Channels * format.BitsPerSample / 8;
            int numberOfSamples = (int)format.Length / bytesPerSample;
            int numberOfDownsampledSamples = (int)(numberOfSamples / ((double)format.SampleRate / config.SampleRate));
            long numberOfFingerprints = numberOfDownsampledSamples / config.SamplesPerFingerprint;
            Assert.AreEqual(numberOfFingerprints, list.Count);
        }

        [Test]
        public async Task CreateFingerprintsWithTheSameFingerprintCommandTest()
        {
            const int secondsToProcess = 8;
            const int startAtSecond = 1;

            var fingerprintCommand = FingerprintCommandBuilder.Instance
                                            .BuildFingerprintCommand()
                                            .From(PathToWav, secondsToProcess, startAtSecond)
                                            .UsingServices(audioService);

            var (h1, _) = await fingerprintCommand.Hash();
            var (h2, _) = await fingerprintCommand.Hash();

            AssertHashDatasAreTheSame(h1, h2);
        }

        [Test]
        public async Task CreateFingerprintFromSamplesWhichAreExactlyEqualToMinimumLength()
        {
            var config = new DefaultFingerprintConfiguration();
            var samples = GenerateRandomAudioSamples(config.SamplesPerFingerprint + config.SpectrogramConfig.WdftSize);

            var hash = await FingerprintCommandBuilder.Instance.BuildFingerprintCommand()
                                                .From(samples)
                                                .UsingServices(audioService)
                                                .Hash();
            Assert.AreEqual(1, hash.Count);
        }

        [Test]
        public async Task ShouldCreateFingerprintsFromAudioSamplesQueryAndGetTheRightResult()
        {
            const int secondsToProcess = 10;
            const int startAtSecond = 30;
            var audioSamples = GetAudioSamples();
            var track = new TrackInfo("1234", audioSamples.Origin, audioSamples.Origin);
            var fingerprints = await FingerprintCommandBuilder.Instance
                    .BuildFingerprintCommand()
                    .From(audioSamples)
                    .UsingServices(audioService)
                    .Hash();

            var modelService = new InMemoryModelService();
            modelService.Insert(track, fingerprints);

            var querySamples = GetQuerySamples(GetAudioSamples(), startAtSecond, secondsToProcess);

            var queryResult = await QueryCommandBuilder.Instance
                    .BuildQueryCommand()
                    .From(new AudioSamples(querySamples, string.Empty, audioSamples.SampleRate))
                    .UsingServices(modelService, audioService)
                    .Query();

            Assert.IsTrue(queryResult.ContainsMatches);
            Assert.AreEqual(1, queryResult.ResultEntries.Count());
            Assert.IsNotNull(queryResult.BestMatch);
            var (bestMatch, _) = queryResult.BestMatch;
            Assert.AreEqual("1234", bestMatch.Track.Id);
            Assert.IsTrue(bestMatch.TrackCoverageWithPermittedGapsLength > secondsToProcess - 3, $"QueryCoverageSeconds:{bestMatch.QueryLength}");
            Assert.AreEqual(startAtSecond, Math.Abs(bestMatch.TrackStartsAt), 0.1d);
            Assert.IsTrue(bestMatch.Confidence > 0.5, $"Confidence:{bestMatch.Confidence}");
        }

        [Test]
        public void ShouldThrowWhenModelServiceIsNull()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => QueryCommandBuilder.Instance.BuildQueryCommand()
                .From(TestUtilities.GenerateRandomAudioSamples(120))
                .Query());
        }

        [Test]
        public async Task ShouldQueryDifferentMediaTypesAndGetRightResults()
        {
            var modelService = new InMemoryModelService();
            var track = new TrackInfo("1", string.Empty, string.Empty, MediaType.Audio | MediaType.Video);
            var audio = TestUtilities.GenerateRandomAudioSamples(120 * 5512);
            var video = TestUtilities.GenerateRandomFrames(120 * 30);

            var avTrack = new AVTrack(new AudioTrack(audio, 120), new VideoTrack(video, 120));
            var avHashes = await FingerprintCommandBuilder.Instance.BuildFingerprintCommand()
                .From(avTrack)
                .Hash();
            
            modelService.Insert(track, avHashes);

            var avResults = await QueryCommandBuilder.Instance
                .BuildQueryCommand()
                .From(avTrack)
                .UsingServices(modelService)
                .Query();

            var (audioResult, videoResult) = avResults;
            Assert.IsTrue(audioResult.ContainsMatches);
            Assert.IsTrue(videoResult.ContainsMatches);
            var audioBestMatch = audioResult.BestMatch;
            AssertBestMatch(audioBestMatch);

            var videoBestMatch = videoResult.BestMatch;
            AssertBestMatch(videoBestMatch);
            
            (audioResult, videoResult) = await QueryCommandBuilder.Instance
                .BuildQueryCommand()
                .From(avTrack.Audio!.Samples)
                .UsingServices(modelService)
                .Query();
            
            Assert.IsNotNull(audioResult);
            Assert.IsNull(videoResult);
            AssertBestMatch(audioResult.BestMatch);
            
            (audioResult, videoResult) = await QueryCommandBuilder.Instance
                .BuildQueryCommand()
                .From(avTrack.Video!.Frames)
                .UsingServices(modelService)
                .Query(); 
            
            Assert.IsNull(audioResult);
            Assert.IsNotNull(videoResult);
            AssertBestMatch(videoResult.BestMatch);
        }

        private static void AssertBestMatch(ResultEntry bestMatch)
        {
            Assert.IsNotNull(bestMatch);
            Assert.AreEqual(1, bestMatch.Confidence, 0.1);
            Assert.AreEqual(1, bestMatch.QueryRelativeCoverage, 0.1);
            Assert.AreEqual(1, bestMatch.TrackRelativeCoverage, 0.1);
        }

        [Test]
        public async Task ShouldCreateSameFingerprintsDuringDifferentParallelRuns()
        {
            var (hashDatas1, _) = await FingerprintCommandBuilder.Instance.BuildFingerprintCommand()
                    .From(GetAudioSamples())
                    .UsingServices(audioService)
                    .Hash();

            var (hashDatas2, _) = await FingerprintCommandBuilder.Instance.BuildFingerprintCommand()
                .From(GetAudioSamples())
                .UsingServices(audioService)
                .Hash();

            var (hashDatas3, _) = await FingerprintCommandBuilder.Instance.BuildFingerprintCommand()
                .From(GetAudioSamples())
                .UsingServices(audioService)
                .Hash();

            var (hashDatas4, _) = await FingerprintCommandBuilder.Instance.BuildFingerprintCommand()
                .From(GetAudioSamples())
                .UsingServices(audioService)
                .Hash();

            AssertHashDatasAreTheSame(hashDatas1, hashDatas2);
            AssertHashDatasAreTheSame(hashDatas2, hashDatas3);
            AssertHashDatasAreTheSame(hashDatas3, hashDatas4);
        }
        
        [Test]
        public async Task ShouldCreateFingerprintsFromAudioSamplesQueryWithPreviouslyCreatedFingerprintsAndGetTheRightResult()
        {
            var audioSamples = GetAudioSamples();
            var track = new TrackInfo("4321", audioSamples.Origin, audioSamples.Origin);
            var fingerprints = await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(audioSamples)
                .UsingServices(audioService)
                .Hash();

            var modelService = new InMemoryModelService();
            modelService.Insert(track, fingerprints);

            var (queryResult, _) = await QueryCommandBuilder.Instance.BuildQueryCommand()
                .From(fingerprints)
                .UsingServices(modelService, audioService)
                .Query();

            Assert.IsTrue(queryResult.ContainsMatches);
            Assert.AreEqual(1, queryResult.ResultEntries.Count());
            var bestMatch = queryResult.BestMatch;
            Assert.AreEqual("4321", bestMatch!.Track.Id);
            Assert.AreEqual(0, Math.Abs(bestMatch.TrackStartsAt), 0.0001d);
            Assert.AreEqual(audioSamples.Duration, bestMatch.TrackCoverageWithPermittedGapsLength, 1.48d);
            Assert.AreEqual(1d, bestMatch.TrackRelativeCoverage, 0.005d);
            Assert.AreEqual(1, bestMatch.Confidence, 0.01, $"Confidence:{bestMatch.Confidence}");
        }

        private static float[] GetQuerySamples(AudioSamples audioSamples, int startAtSecond, int secondsToProcess)
        {
            int sampleRate = audioSamples.SampleRate;
            float[] querySamples = new float[sampleRate * secondsToProcess];
            int startAt = startAtSecond * sampleRate;
            Array.Copy(audioSamples.Samples, startAt, querySamples, 0, querySamples.Length);
            return querySamples;
        }

        private static AudioSamples GenerateRandomAudioSamples(int length)
        {
            return new AudioSamples(TestUtilities.GenerateRandomFloatArray(length), string.Empty, 5512);
        }
    }
}