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
        public void CreateFingerprintsFromFileAndAssertNumberOfFingerprints()
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

            var fingerprints = command.Hash()
                                      .Result;

            Assert.AreEqual(expectedFingerprints, fingerprints.Count);
        }

        [Test]
        public void ShouldCreateFingerprintsInsertThenQueryAndGetTheRightResult()
        {
            const int SecondsToProcess = 8;
            const int StartAtSecond = 2;
            var track = new TrackData(GetTagInfo());
            var trackReference = modelService.InsertTrack(track);

            var hashDatas = FingerprintCommandBuilder.Instance
                                            .BuildFingerprintCommand()
                                            .From(PathToWav)
                                            .WithFingerprintConfig(new HighPrecisionFingerprintConfiguration())
                                            .UsingServices(audioService)
                                            .Hash()
                                            .Result;

            modelService.InsertHashDataForTrack(hashDatas, trackReference);

            var queryResult = QueryCommandBuilder.Instance
                               .BuildQueryCommand()
                               .From(PathToWav, SecondsToProcess, StartAtSecond)
                               .WithQueryConfig(new HighPrecisionQueryConfiguration())
                               .UsingServices(modelService, audioService)
                               .Query()
                               .Result;

            Assert.IsTrue(queryResult.ContainsMatches);
            Assert.AreEqual(1, queryResult.ResultEntries.Count());
            var bestMatch = queryResult.BestMatch;
            Assert.AreEqual(trackReference, bestMatch.Track.TrackReference);
            Assert.IsTrue(bestMatch.QueryMatchLength > SecondsToProcess - 3, $"QueryMatchLength:{bestMatch.QueryLength}");
            Assert.AreEqual(StartAtSecond, Math.Abs(bestMatch.TrackStartsAt), 0.1d);
            Assert.IsTrue(bestMatch.Confidence > 0.7, $"Confidence:{bestMatch.Confidence}");
        }

        [Test]
        public void CreateFingerprintsFromFileAndFromAudioSamplesAndGetTheSameResultTest()
        {
            const int SecondsToProcess = 8;
            const int StartAtSecond = 1;

            var samples = audioService.ReadMonoSamplesFromFile(PathToWav, SampleRate, SecondsToProcess, StartAtSecond);

            var hashDatasFromFile = FingerprintCommandBuilder.Instance
                                        .BuildFingerprintCommand()
                                        .From(PathToWav, SecondsToProcess, StartAtSecond)
                                        .UsingServices(audioService)
                                        .Hash()
                                        .Result;

            var hashDatasFromSamples = FingerprintCommandBuilder.Instance
                                        .BuildFingerprintCommand()
                                        .From(samples)
                                        .UsingServices(audioService)
                                        .Hash()
                                        .Result;

            AssertHashDatasAreTheSame(hashDatasFromFile, hashDatasFromSamples);
        }

        [Test]
        public void CheckFingerprintCreationAlgorithmTest()
        {
            var format = WaveFormat.FromFile(PathToWav);
            var list = FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(PathToWav)
                .WithFingerprintConfig(configuration =>
                      {
                          configuration.Stride = new StaticStride(0);
                          return configuration;
                      })
                .UsingServices(audioService)
                .Hash()
                .Result;

            int bytesPerSample = format.Channels * format.BitsPerSample / 8;
            int numberOfSamples = (int) format.Length / bytesPerSample;
            int numberOfDownsampledSamples = (int)(numberOfSamples / ((double)format.SampleRate / config.SampleRate));
            long numberOfFingerprints = numberOfDownsampledSamples / config.SamplesPerFingerprint;
            Assert.AreEqual(numberOfFingerprints, list.Count);
        }

        [Test]
        public void CreateFingerprintsWithTheSameFingerprintCommandTest()
        {
            const int SecondsToProcess = 8;
            const int StartAtSecond = 1;

            var fingerprintCommand = FingerprintCommandBuilder.Instance
                                            .BuildFingerprintCommand()
                                            .From(PathToWav, SecondsToProcess, StartAtSecond)
                                            .UsingServices(audioService);

            var firstHashDatas = fingerprintCommand.Hash().Result;
            var secondHashDatas = fingerprintCommand.Hash().Result;

            AssertHashDatasAreTheSame(firstHashDatas, secondHashDatas);
        }

        [Test]
        public void CreateFingerprintFromSamplesWhichAreExactlyEqualToMinimumLength()
        {
            var samples = GenerateRandomAudioSamples(config.SamplesPerFingerprint + config.SpectrogramConfig.WdftSize);

            var hash = FingerprintCommandBuilder.Instance.BuildFingerprintCommand()
                                                .From(samples)
                                                .UsingServices(audioService)
                                                .Hash()
                                                .Result;
            Assert.AreEqual(1, hash.Count);
        }

        [Test]
        public void ShouldCreateFingerprintsFromAudioSamplesQueryAndGetTheRightResult()
        {
            const int SecondsToProcess = 10;
            const int StartAtSecond = 30;
            var audioSamples = GetAudioSamples();
            var track = new TrackData(string.Empty, audioSamples.Origin, audioSamples.Origin, string.Empty, 1986, audioSamples.Duration);
            var trackReference = modelService.InsertTrack(track);
            var hashDatas = FingerprintCommandBuilder.Instance.BuildFingerprintCommand()
                    .From(audioSamples)
                    .UsingServices(audioService)
                    .Hash()
                    .Result;

            modelService.InsertHashDataForTrack(hashDatas, trackReference);

            var querySamples = GetQuerySamples(GetAudioSamples(), StartAtSecond, SecondsToProcess);

            var queryResult = QueryCommandBuilder.Instance.BuildQueryCommand()
                    .From(new AudioSamples(querySamples, string.Empty, audioSamples.SampleRate))
                    .UsingServices(modelService, audioService)
                    .Query()
                    .Result;

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

        private AudioSamples GenerateRandomAudioSamples(int length)
        {
            return new AudioSamples(TestUtilities.GenerateRandomFloatArray(length), string.Empty, SampleRate);
        }
    }
}
