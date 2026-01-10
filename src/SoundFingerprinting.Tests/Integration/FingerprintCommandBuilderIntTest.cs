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
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Moq;
    using SoundFingerprinting.Configuration.Frames;
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
            Assert.That(fingerprints, Is.Not.Null);
            int samples = (int)(fingerprints.DurationInSeconds * config.SampleRate);
            int expectedFingerprints = (int)Math.Round(((double)(samples - (config.SamplesPerFingerprint + config.SpectrogramConfig.WdftSize)) / config.Stride.NextStride));

            Assert.That(fingerprints.Count, Is.EqualTo(expectedFingerprints));
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
            Assert.That(fingerprints, Is.Not.Null);
            int expected = (int)((fingerprints.DurationInSeconds * config.SampleRate) / config.SamplesPerFingerprint);
            Assert.That(fingerprints.Count, Is.EqualTo(expected)); 
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

            Assert.That(queryResult.ContainsMatches, Is.True);
            Assert.That(queryResult.ResultEntries.Count(, Is.EqualTo(1)));
            Assert.That(queryResult.BestMatch, Is.Not.Null);
            var (bestMatch, _) = queryResult.BestMatch;
            Assert.That(bestMatch, Is.Not.Null);
            Assert.That(bestMatch.Track.Id, Is.EqualTo("id"));
            Assert.That(bestMatch.TrackCoverageWithPermittedGapsLength > secondsToProcess - 3, Is.True, $"QueryCoverageSeconds:{bestMatch.QueryLength}");
            Assert.That(Math.Abs(bestMatch.TrackStartsAt), Is.EqualTo(startAtSecond).Within(0.1d));
            Assert.That(bestMatch.Confidence > 0.7, Is.True, $"Confidence:{bestMatch.Confidence}");
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

            AssertHashDataIsTheSame(h1, h2);
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
            Assert.That(list.Count, Is.EqualTo(numberOfFingerprints));
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

            AssertHashDataIsTheSame(h1, h2);
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
            Assert.That(hash.Count, Is.EqualTo(1));
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

            Assert.That(queryResult.ContainsMatches, Is.True);
            Assert.That(queryResult.ResultEntries, Is.Not.Empty);
            Assert.That(queryResult.BestMatch, Is.Not.Null);
            var (bestMatch, _) = queryResult.BestMatch;
            Assert.That(bestMatch!.Track.Id, Is.EqualTo("1234"));
            Assert.That(bestMatch.TrackCoverageWithPermittedGapsLength, Is.EqualTo(secondsToProcess).Within(1d), $"QueryCoverageSeconds:{bestMatch.QueryLength}");
            Assert.That(Math.Abs(bestMatch.TrackStartsAt), Is.EqualTo(startAtSecond).Within(0.1d));
            Assert.That(bestMatch.Confidence, Is.EqualTo(1d).Within(0.1d), $"Confidence:{bestMatch.Confidence}");
        }

        [Test]
        public void ShouldThrowWhenModelServiceIsNull()
        {
            Assert.That((, Throws.TypeOf<ArgumentException>()) => QueryCommandBuilder.Instance.BuildQueryCommand()
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

            var avTrack = new AVTrack(new AudioTrack(audio), new VideoTrack(video));
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
            Assert.That(audioResult, Is.Not.Null);
            Assert.That(audioResult.ContainsMatches, Is.True);
            Assert.That(videoResult, Is.Not.Null);
            Assert.That(videoResult.ContainsMatches, Is.True);
            var audioBestMatch = audioResult.BestMatch;
            AssertBestMatch(audioBestMatch);

            var videoBestMatch = videoResult.BestMatch;
            AssertBestMatch(videoBestMatch);
            
            (audioResult, videoResult) = await QueryCommandBuilder.Instance
                .BuildQueryCommand()
                .From(avTrack.Audio!.Samples)
                .UsingServices(modelService)
                .Query();
            
            Assert.That(audioResult, Is.Not.Null);
            Assert.That(videoResult, Is.Null);
            AssertBestMatch(audioResult.BestMatch);
            
            (audioResult, videoResult) = await QueryCommandBuilder.Instance
                .BuildQueryCommand()
                .From(avTrack.Video!.Frames)
                .UsingServices(modelService)
                .Query(); 
            
            Assert.That(audioResult, Is.Null);
            Assert.That(videoResult, Is.Not.Null);
            AssertBestMatch(videoResult.BestMatch);
        }

        private static void AssertBestMatch(ResultEntry bestMatch)
        {
            Assert.That(bestMatch, Is.Not.Null);
            Assert.That(bestMatch.Confidence, Is.EqualTo(1).Within(0.1));
            Assert.That(bestMatch.Coverage.QueryRelativeCoverage, Is.EqualTo(1).Within(0.1));
            Assert.That(bestMatch.Coverage.TrackRelativeCoverage, Is.EqualTo(1).Within(0.1));
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

            AssertHashDataIsTheSame(hashDatas1, hashDatas2);
            AssertHashDataIsTheSame(hashDatas2, hashDatas3);
            AssertHashDataIsTheSame(hashDatas3, hashDatas4);
        }
        
        [Test]
        public async Task ShouldCreateFingerprintsFromAudioSamplesQueryWithPreviouslyCreatedFingerprintsAndGetTheRightResult()
        {
            var audioSamples = GetAudioSamples();
            var track = new TrackInfo("4321", audioSamples.Origin, audioSamples.Origin);
            var avHashes = await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(audioSamples)
                .UsingServices(audioService)
                .Hash();

            var modelService = new InMemoryModelService();
            modelService.Insert(track, avHashes);

            var (queryResult, _) = await QueryCommandBuilder.Instance
                .BuildQueryCommand()
                .From(avHashes)
                .UsingServices(modelService)
                .Query();

            Assert.That(queryResult, Is.Not.Null);
            Assert.That(queryResult.ContainsMatches, Is.True);
            Assert.That(queryResult.ResultEntries.Count(, Is.EqualTo(1)));
            var bestMatch = queryResult.BestMatch;
            Assert.That(bestMatch!.Track.Id, Is.EqualTo("4321"));
            Assert.That(Math.Abs(bestMatch.TrackStartsAt), Is.EqualTo(0).Within(0.0001d));
            Assert.That(bestMatch.TrackCoverageWithPermittedGapsLength, Is.EqualTo(audioSamples.Duration).Within(1.48d));
            Assert.That(bestMatch.Coverage.TrackRelativeCoverage, Is.EqualTo(1d).Within(0.005d));
            Assert.That(bestMatch.Confidence, Is.EqualTo(1).Within(0.01), $"Confidence:{bestMatch.Confidence}");
        }

        [Test]
        public async Task ShouldNormalizeImageFrames()
        {
            var blurNormalization = new Mock<IFrameNormalization>();
            var logImageNormalization = new Mock<IFrameNormalization>();
            
            var audioSamples = GetAudioSamples();
            await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(audioSamples)
                .WithFingerprintConfig(config =>
                {
                    config.Audio.FrameNormalizationTransform = new FrameNormalizationChain([
                        logImageNormalization.Object, blurNormalization.Object
                    ]);
                    
                    return config;
                })
                .UsingServices(audioService)
                .Hash();
            
            blurNormalization.Verify(_ => _.Normalize(It.IsAny<IEnumerable<Frame>>()));
            logImageNormalization.Verify(_ => _.Normalize(It.IsAny<IEnumerable<Frame>>()));
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