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

    [TestFixture]
    public class FingerprintCommandBuilderIntTest : IntegrationWithSampleFilesTest
    {
        private readonly DefaultFingerprintConfiguration config = new DefaultFingerprintConfiguration();
        private readonly SoundFingerprintingAudioService audioService = new SoundFingerprintingAudioService();

        [Test]
        public async Task CreateFingerprintsFromFileAndAssertNumberOfFingerprints()
        {
            const int staticStride = 5096;

            var fingerprintConfiguration = new DefaultFingerprintConfiguration { Stride = new IncrementalStaticStride(staticStride) };

            var command = FingerprintCommandBuilder.Instance.BuildFingerprintCommand()
                                        .From(PathToWav)
                                        .WithFingerprintConfig(fingerprintConfiguration)
                                        .UsingServices(audioService);

            double seconds = audioService.GetLengthInSeconds(PathToWav);
            int samples = (int)(seconds * fingerprintConfiguration.SampleRate);
            int expectedFingerprints = (samples - fingerprintConfiguration.SamplesPerFingerprint) / staticStride;

            var fingerprints = await command.Hash();

            Assert.AreEqual(expectedFingerprints, fingerprints.Count);
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
                                            .WithFingerprintConfig(new HighPrecisionFingerprintConfiguration())
                                            .UsingServices(audioService)
                                            .Hash();

            var modelService = new InMemoryModelService();
            modelService.Insert(track, fingerprints);

            var queryResult = await QueryCommandBuilder.Instance
                               .BuildQueryCommand()
                               .From(PathToWav, secondsToProcess, startAtSecond)
                               .WithQueryConfig(new HighPrecisionQueryConfiguration())
                               .UsingServices(modelService, audioService)
                               .Query();

            Assert.IsTrue(queryResult.ContainsMatches);
            Assert.AreEqual(1, queryResult.ResultEntries.Count());
            var bestMatch = queryResult.BestMatch;
            Assert.AreEqual("id", bestMatch.Track.Id);
            Assert.IsTrue(bestMatch.CoverageWithPermittedGapsLength > secondsToProcess - 3, $"QueryCoverageSeconds:{bestMatch.QueryLength}");
            Assert.AreEqual(startAtSecond, Math.Abs(bestMatch.TrackStartsAt), 0.1d);
            Assert.IsTrue(bestMatch.Confidence > 0.7, $"Confidence:{bestMatch.Confidence}");
        }

        [Test]
        public async Task CreateFingerprintsFromFileAndFromAudioSamplesAndGetTheSameResultTest()
        {
            const int secondsToProcess = 8;
            const int startAtSecond = 1;

            var samples = audioService.ReadMonoSamplesFromFile(PathToWav, 5512, secondsToProcess, startAtSecond);

            var hashDatasFromFile = await FingerprintCommandBuilder.Instance
                                        .BuildFingerprintCommand()
                                        .From(PathToWav, secondsToProcess, startAtSecond)
                                        .UsingServices(audioService)
                                        .Hash();

            var hashDatasFromSamples = await FingerprintCommandBuilder.Instance
                                        .BuildFingerprintCommand()
                                        .From(samples)
                                        .UsingServices(audioService)
                                        .Hash();

            AssertHashDatasAreTheSame(hashDatasFromFile, hashDatasFromSamples);
        }

        [Test]
        public async Task CheckFingerprintCreationAlgorithmTest()
        {
            var format = WaveFormat.FromFile(PathToWav);
            var list = await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(PathToWav)
                .WithFingerprintConfig(configuration =>
                      {
                          configuration.Stride = new StaticStride(0);
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

            var firstHashDatas = await fingerprintCommand.Hash();
            var secondHashDatas = await fingerprintCommand.Hash();

            AssertHashDatasAreTheSame(firstHashDatas, secondHashDatas);
        }

        [Test]
        public async Task CreateFingerprintFromSamplesWhichAreExactlyEqualToMinimumLength()
        {
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
            var bestMatch = queryResult.BestMatch;
            Assert.AreEqual("1234", bestMatch.Track.Id);
            Assert.IsTrue(bestMatch.CoverageWithPermittedGapsLength > secondsToProcess - 3, $"QueryCoverageSeconds:{bestMatch.QueryLength}");
            Assert.AreEqual(startAtSecond, Math.Abs(bestMatch.TrackStartsAt), 0.1d);
            Assert.IsTrue(bestMatch.Confidence > 0.5, $"Confidence:{bestMatch.Confidence}");
        }

        [Test]
        public async Task ShouldCreateSameFingerprintsDuringDifferentParallelRuns()
        {
            var hashDatas1 = await FingerprintCommandBuilder.Instance.BuildFingerprintCommand()
                    .From(GetAudioSamples())
                    .UsingServices(audioService)
                    .Hash();

            var hashDatas2 = await FingerprintCommandBuilder.Instance.BuildFingerprintCommand()
                .From(GetAudioSamples())
                .UsingServices(audioService)
                .Hash();

            var hashDatas3 = await FingerprintCommandBuilder.Instance.BuildFingerprintCommand()
                .From(GetAudioSamples())
                .UsingServices(audioService)
                .Hash();

            var hashDatas4 = await FingerprintCommandBuilder.Instance.BuildFingerprintCommand()
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

            var queryResult = await QueryCommandBuilder.Instance.BuildQueryCommand()
                .From(fingerprints)
                .UsingServices(modelService, audioService)
                .Query();

            Assert.IsTrue(queryResult.ContainsMatches);
            Assert.AreEqual(1, queryResult.ResultEntries.Count());
            var bestMatch = queryResult.BestMatch;
            Assert.AreEqual("4321", bestMatch.Track.Id);
            Assert.AreEqual(0, Math.Abs(bestMatch.TrackStartsAt), 0.0001d);
            Assert.AreEqual(audioSamples.Duration, bestMatch.CoverageWithPermittedGapsLength, 1.48d);
            Assert.AreEqual(1d, bestMatch.RelativeCoverage, 0.005d);
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