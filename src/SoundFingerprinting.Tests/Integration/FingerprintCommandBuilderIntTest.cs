namespace SoundFingerprinting.Tests.Integration
{
    using System;
    using System.Linq;

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

        private readonly IModelService modelService = new InMemoryModelService();
        private readonly IFingerprintCommandBuilder fingerprintCommandBuilder = new FingerprintCommandBuilder();
        private readonly IQueryCommandBuilder queryCommandBuilder = new QueryCommandBuilder();
        private readonly IAudioService audioService = new SoundFingerprintingAudioService();

        [Test]
        public void CreateFingerprintsFromFileAndAssertNumberOfFingerprints()
        {
            const int StaticStride = 5096;

            var command = fingerprintCommandBuilder.BuildFingerprintCommand()
                                        .From(PathToWav)
                                        .WithFingerprintConfig(cnf =>
                                            {
                                                cnf.Stride = new IncrementalStaticStride(StaticStride);
                                                return cnf;
                                            })
                                        .UsingServices(audioService);

            double seconds = audioService.GetLengthInSeconds(PathToWav);
            var fingerprintConfiguration = command.FingerprintConfiguration;
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

            var hashDatas = fingerprintCommandBuilder
                                            .BuildFingerprintCommand()
                                            .From(PathToWav)
                                            .WithFingerprintConfig(new HighPrecisionFingerprintConfiguration())
                                            .UsingServices(audioService)
                                            .Hash()
                                            .Result;

            modelService.InsertHashDataForTrack(hashDatas, trackReference);

            var queryResult = queryCommandBuilder.BuildQueryCommand()
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

            var hashDatasFromFile = fingerprintCommandBuilder
                                        .BuildFingerprintCommand()
                                        .From(PathToWav, SecondsToProcess, StartAtSecond)
                                        .UsingServices(audioService)
                                        .Hash()
                                        .Result;

            var hashDatasFromSamples = fingerprintCommandBuilder
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
            var list = fingerprintCommandBuilder.BuildFingerprintCommand()
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
        public void CreateFingerprintsWithTheSameFingerprintCommandTest()
        {
            const int SecondsToProcess = 8;
            const int StartAtSecond = 1;

            var fingerprintCommand = fingerprintCommandBuilder
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

            var hash = fingerprintCommandBuilder.BuildFingerprintCommand()
                                                .From(samples)
                                                .UsingServices(audioService)
                                                .Hash()
                                                .Result;
            Assert.AreEqual(1, hash.Count);
        }

        private AudioSamples GenerateRandomAudioSamples(int length)
        {
            return new AudioSamples(TestUtilities.GenerateRandomFloatArray(length), string.Empty, SampleRate);
        }
    }
}
