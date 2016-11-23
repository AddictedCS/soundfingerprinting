namespace SoundFingerprinting.Tests.Integration
{
    using System;
    using System.IO;
    using System.Linq;

    using NUnit.Framework;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Audio.NAudio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.InMemory;
    using SoundFingerprinting.Strides;

    [TestFixture]
    public class FingerprintCommandBuilderIntTest : IntegrationWithSampleFilesTest
    {
        private readonly ModelService modelService;
        private readonly IFingerprintCommandBuilder fingerprintCommandBuilder;
        private readonly QueryFingerprintService queryFingerprintService;
        private readonly NAudioService audioService;
        private readonly ITagService tagService;
        private readonly IWaveFileUtility waveUtility;

        public FingerprintCommandBuilderIntTest()
        {
            audioService = new NAudioService();
            tagService = new NAudioTagService();
            waveUtility = new NAudioWaveFileUtility();
            modelService = new InMemoryModelService();
            fingerprintCommandBuilder = new FingerprintCommandBuilder();
            queryFingerprintService = new QueryFingerprintService();
        }

        [TearDown]
        public void TearDown()
        {
            var ramStorage = (RAMStorage)DependencyResolver.Current.Get<IRAMStorage>();
            ramStorage.Reset(new DefaultFingerprintConfiguration().HashingConfig.NumberOfLSHTables);
        }

        [Test]
        public void CreateFingerprintsFromFileAndAssertNumberOfFingerprints()
        {
            const int StaticStride = 5096;

            var command = fingerprintCommandBuilder.BuildFingerprintCommand()
                                        .From(PathToMp3)
                                        .WithFingerprintConfig(config =>
                                            {
                                                config.SpectrogramConfig.Stride = new IncrementalStaticStride(StaticStride);
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
        public void CreateFingerprintsInsertThenQueryAndGetTheRightResult()
        {
            const int SecondsToProcess = 10;
            const int StartAtSecond = 30;
            var info = tagService.GetTagInfo(PathToMp3);
            var track = new TrackData(info.ISRC, info.Artist, info.Title, info.Album, info.Year, (int)info.Duration);
            var trackReference = modelService.InsertTrack(track);

            var hashDatas = fingerprintCommandBuilder
                                            .BuildFingerprintCommand()
                                            .From(PathToMp3, SecondsToProcess, StartAtSecond)
                                            .UsingServices(audioService)
                                            .Hash()
                                            .Result;

            modelService.InsertHashDataForTrack(hashDatas, trackReference);

            var queryResult = queryFingerprintService.Query(hashDatas, new DefaultQueryConfiguration(), modelService);

            Assert.IsTrue(queryResult.ContainsMatches);
            Assert.AreEqual(1, queryResult.ResultEntries.Count());
            Assert.AreEqual(trackReference, queryResult.BestMatch.Track.TrackReference);
        }

        [Test]
        public void CreateFingerprintsFromFileAndFromAudioSamplesAndGetTheSameResultTest()
        {
            const int SecondsToProcess = 20;
            const int StartAtSecond = 15;

            AudioSamples samples = audioService.ReadMonoSamplesFromFile(PathToMp3, SampleRate, SecondsToProcess, StartAtSecond);

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
                                      .WithFingerprintConfig(customConfiguration => customConfiguration.Stride = new StaticStride(0))
                                      .UsingServices(audioService)
                                      .Hash()
                                      .Result;

            long expected = fileSize / (8192 * 4); // One fingerprint corresponds to a granularity of 8192 samples which is 16384 bytes
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
            DefaultFingerprintConfiguration config = new DefaultFingerprintConfiguration();

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
            return new AudioSamples
            {
                Duration = length,
                Origin = string.Empty,
                SampleRate = SampleRate,
                Samples = TestUtilities.GenerateRandomFloatArray(length)
            };
        }
    }
}
