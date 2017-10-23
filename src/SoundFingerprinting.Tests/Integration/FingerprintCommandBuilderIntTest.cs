namespace SoundFingerprinting.Tests.Integration
{
    using System;
    using System.IO;
    using System.Linq;

    using Audio;
    using Audio.NAudio;

    using Builder;
    using Configuration;
    using DAO.Data;
    using InMemory;
    using NUnit.Framework;
    using Strides;

    [TestFixture]
    [Category("RequiresWindowsDLL")]
    public class FingerprintCommandBuilderIntTest : IntegrationWithSampleFilesTest
    {
        private readonly DefaultFingerprintConfiguration config = new DefaultFingerprintConfiguration();

        private readonly IModelService modelService = new InMemoryModelService();
        private readonly IFingerprintCommandBuilder fingerprintCommandBuilder = new FingerprintCommandBuilder();
        private readonly IQueryCommandBuilder queryCommandBuilder = new QueryCommandBuilder();
        private readonly IAudioService audioService = new NAudioService();
        private readonly ITagService tagService = new NAudioTagService();
        private readonly IWaveFileUtility waveUtility = new NAudioWaveFileUtility();

        [Test]
        public void CreateFingerprintsFromFileAndAssertNumberOfFingerprints()
        {
            const int StaticStride = 5096;

            var command = fingerprintCommandBuilder.BuildFingerprintCommand()
                                        .From(PathToMp3)
                                        .WithFingerprintConfig(cnf =>
                                            {
                                                cnf.Stride = new IncrementalStaticStride(StaticStride);
                                                return cnf;
                                            })
                                        .UsingServices(audioService);

            double seconds = tagService.GetTagInfo(PathToMp3).Duration;
            var fingerprintConfiguration = command.FingerprintConfiguration;
            int samples = (int)(seconds * fingerprintConfiguration.SampleRate);
            int expectedFingerprints = (samples - fingerprintConfiguration.SamplesPerFingerprint - fingerprintConfiguration.SpectrogramConfig.WdftSize) / StaticStride * StaticStride / StaticStride;

            var fingerprints = command.Hash().Result;

            Assert.AreEqual(expectedFingerprints, fingerprints.Count);
        }

        [Test]
        public void ShouldCreateFingerprintsInsertThenQueryAndGetTheRightResult()
        {
            const int SecondsToProcess = 10;
            const int StartAtSecond = 30;
            var tags = tagService.GetTagInfo(PathToMp3);
            var track = new TrackData(tags);
            var trackReference = modelService.InsertTrack(track);

            var hashDatas = fingerprintCommandBuilder
                                            .BuildFingerprintCommand()
                                            .From(PathToMp3)
                                            .UsingServices(audioService)
                                            .Hash()
                                            .Result;

            modelService.InsertHashDataForTrack(hashDatas, trackReference);

            var queryResult = queryCommandBuilder.BuildQueryCommand()
                               .From(PathToMp3, SecondsToProcess, StartAtSecond)
                               .UsingServices(modelService, audioService)
                               .Query()
                               .Result;

            Assert.IsTrue(queryResult.ContainsMatches);
            Assert.AreEqual(1, queryResult.ResultEntries.Count());
            var bestMatch = queryResult.BestMatch;
            Assert.AreEqual(trackReference, bestMatch.Track.TrackReference);
            Assert.IsTrue(bestMatch.QueryMatchLength > SecondsToProcess - 3, string.Format("QueryMatchLength:{0}", bestMatch.QueryLength));
            Assert.AreEqual(StartAtSecond, Math.Abs(bestMatch.TrackStartsAt), 0.1d);
            Assert.IsTrue(bestMatch.Confidence > 0.7, string.Format("Confidence:{0}", bestMatch.Confidence));
        }

        [Test]
        public void CreateFingerprintsFromFileAndFromAudioSamplesAndGetTheSameResultTest()
        {
            const int SecondsToProcess = 20;
            const int StartAtSecond = 15;

            var samples = audioService.ReadMonoSamplesFromFile(PathToMp3, SampleRate, SecondsToProcess, StartAtSecond);

            var hashDatasFromFile = fingerprintCommandBuilder
                                        .BuildFingerprintCommand()
                                        .From(PathToMp3, SecondsToProcess, StartAtSecond)
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
            string tempFile = Path.GetTempPath() + DateTime.Now.Ticks + ".wav";
            RecodeFileToWaveFile(tempFile);
            long fileSize = new FileInfo(tempFile).Length;

            var list = fingerprintCommandBuilder.BuildFingerprintCommand()
                .From(PathToMp3)
                .WithFingerprintConfig(
                      customConfiguration =>
                      {
                          customConfiguration.Stride = new StaticStride(0);
                          return customConfiguration;
                      })
                .UsingServices(audioService)
                .Hash()
                .Result;

            long expected = fileSize / (config.SamplesPerFingerprint * sizeof(float)); // One fingerprint corresponds to a granularity of 8192 samples which is 16384 bytes
            Assert.AreEqual(expected, list.Count);
            File.Delete(tempFile);
        }

        [Test]
        public void CreateFingerprintsWithTheSameFingerprintCommandTest()
        {
            const int SecondsToProcess = 20;
            const int StartAtSecond = 15;

            var fingerprintCommand = fingerprintCommandBuilder
                                            .BuildFingerprintCommand()
                                            .From(PathToMp3, SecondsToProcess, StartAtSecond)
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

        private void RecodeFileToWaveFile(string tempFile)
        {
            var samples = audioService.ReadMonoSamplesFromFile(PathToMp3, SampleRate);
            waveUtility.WriteSamplesToFile(samples.Samples, SampleRate, tempFile);
        }

        private AudioSamples GenerateRandomAudioSamples(int length)
        {
            return new AudioSamples(TestUtilities.GenerateRandomFloatArray(length), string.Empty, SampleRate);
        }
    }
}
