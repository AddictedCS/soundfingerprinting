namespace SoundFingerprinting.Tests.Integration
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Audio;

    using Builder;
    using Configuration;
    using DAO.Data;
    using InMemory;
    using NUnit.Framework;

    using SoundFingerprinting.Data;
    using SoundFingerprinting.DAO;

    using Strides;

    [TestFixture]
    public class FingerprintCommandBuilderIntTest : IntegrationWithSampleFilesTest
    {
        private readonly DefaultFingerprintConfiguration config = new DefaultFingerprintConfiguration();
        private readonly IAudioService audioService = new SoundFingerprintingAudioService();

        private IModelService modelService = new InMemoryModelService();

        [TearDown]
        public void TearDown()
        {
            modelService = new InMemoryModelService();
        }

        [Test]
        public async Task CreateFingerprintsFromFileAndAssertNumberOfFingerprints()
        {
            const int StaticStride = 5096;

            var fingerprintConfiguration = new DefaultFingerprintConfiguration { Stride = new IncrementalStaticStride(StaticStride) };

            var command = FingerprintCommandBuilder.Instance.BuildFingerprintCommand()
                                        .From(PathToWav)
                                        .WithFingerprintConfig(fingerprintConfiguration)
                                        .UsingServices(audioService);

            double seconds = audioService.GetLengthInSeconds(PathToWav);
            int samples = (int)(seconds * fingerprintConfiguration.SampleRate);
            int expectedFingerprints = (samples - fingerprintConfiguration.SamplesPerFingerprint) / StaticStride;

            var fingerprints = await command.Hash();

            Assert.AreEqual(expectedFingerprints, fingerprints.Count);
        }

        [Test]
        public async Task ShouldCreateFingerprintsInsertThenQueryAndGetTheRightResult()
        {
            const int SecondsToProcess = 8;
            const int StartAtSecond = 2;
            var track = new TrackInfo("isrc", "artist", "title", 200d);

            var fingerprints = await FingerprintCommandBuilder.Instance
                                            .BuildFingerprintCommand()
                                            .From(PathToWav)
                                            .WithFingerprintConfig(new HighPrecisionFingerprintConfiguration())
                                            .UsingServices(audioService)
                                            .Hash();

            var trackReference = modelService.Insert(track, fingerprints);

            var queryResult = await QueryCommandBuilder.Instance
                               .BuildQueryCommand()
                               .From(PathToWav, SecondsToProcess, StartAtSecond)
                               .WithQueryConfig(new HighPrecisionQueryConfiguration())
                               .UsingServices(modelService, audioService)
                               .Query();

            Assert.IsTrue(queryResult.ContainsMatches);
            Assert.AreEqual(1, queryResult.ResultEntries.Count());
            var bestMatch = queryResult.BestMatch;
            Assert.AreEqual(trackReference, bestMatch.Track.TrackReference);
            Assert.IsTrue(bestMatch.QueryMatchLength > SecondsToProcess - 3, $"QueryMatchLength:{bestMatch.QueryLength}");
            Assert.AreEqual(StartAtSecond, Math.Abs(bestMatch.TrackStartsAt), 0.1d);
            Assert.IsTrue(bestMatch.Confidence > 0.7, $"Confidence:{bestMatch.Confidence}");
        }

        [Test]
        public async Task CreateFingerprintsFromFileAndFromAudioSamplesAndGetTheSameResultTest()
        {
            const int SecondsToProcess = 8;
            const int StartAtSecond = 1;

            var samples = audioService.ReadMonoSamplesFromFile(PathToWav, SampleRate, SecondsToProcess, StartAtSecond);

            var hashDatasFromFile = await FingerprintCommandBuilder.Instance
                                        .BuildFingerprintCommand()
                                        .From(PathToWav, SecondsToProcess, StartAtSecond)
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
            int numberOfSamples = (int) format.Length / bytesPerSample;
            int numberOfDownsampledSamples = (int)(numberOfSamples / ((double)format.SampleRate / config.SampleRate));
            long numberOfFingerprints = numberOfDownsampledSamples / config.SamplesPerFingerprint;
            Assert.AreEqual(numberOfFingerprints, list.Count);
        }

        [Test]
        public async Task CreateFingerprintsWithTheSameFingerprintCommandTest()
        {
            const int SecondsToProcess = 8;
            const int StartAtSecond = 1;

            var fingerprintCommand = FingerprintCommandBuilder.Instance
                                            .BuildFingerprintCommand()
                                            .From(PathToWav, SecondsToProcess, StartAtSecond)
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
            const int SecondsToProcess = 10;
            const int StartAtSecond = 30;
            var audioSamples = GetAudioSamples();
            var track = new TrackInfo(string.Empty, audioSamples.Origin, audioSamples.Origin, audioSamples.Duration);
            var fingerprints = await FingerprintCommandBuilder.Instance
                    .BuildFingerprintCommand()
                    .From(audioSamples)
                    .UsingServices(audioService)
                    .Hash();

            var trackReference = modelService.Insert(track, fingerprints);

            var querySamples = GetQuerySamples(GetAudioSamples(), StartAtSecond, SecondsToProcess);

            var queryResult = await QueryCommandBuilder.Instance.BuildQueryCommand()
                    .From(new AudioSamples(querySamples, string.Empty, audioSamples.SampleRate))
                    .UsingServices(modelService, audioService)
                    .Query();

            Assert.IsTrue(queryResult.ContainsMatches);
            Assert.AreEqual(1, queryResult.ResultEntries.Count());
            var bestMatch = queryResult.BestMatch;
            Assert.AreEqual(trackReference, bestMatch.Track.TrackReference);
            Assert.IsTrue(bestMatch.QueryMatchLength > SecondsToProcess - 3, $"QueryMatchLength:{bestMatch.QueryLength}");
            Assert.AreEqual(StartAtSecond, Math.Abs(bestMatch.TrackStartsAt), 0.1d);
            Assert.IsTrue(bestMatch.Confidence > 0.7, $"Confidence:{bestMatch.Confidence}");
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
            return new AudioSamples(TestUtilities.GenerateRandomFloatArray(length), string.Empty, SampleRate);
        }
    }
}
