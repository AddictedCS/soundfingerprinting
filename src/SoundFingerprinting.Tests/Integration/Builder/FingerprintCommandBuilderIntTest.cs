namespace SoundFingerprinting.Tests.Integration.Builder
{
    using System;
    using System.IO;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Audio.Bass;
    using SoundFingerprinting.Audio.NAudio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.SQL;
    using SoundFingerprinting.Strides;
    using SoundFingerprinting.Tests.Integration;

    [TestClass]
    public class FingerprintCommandBuilderIntTest : AbstractIntegrationTest
    {
        private ModelService modelService;

        private IFingerprintCommandBuilder fingerprintCommandBuilderWithBass;

        private IFingerprintCommandBuilder fingerprintCommandBuilderWithNAudio;

        private IQueryFingerprintService queryFingerprintService;

        [TestInitialize]
        public override void SetUp()
        {
            base.SetUp();

            modelService = new SqlModelService();
            DependencyResolver.Current.Bind<IAudioService, BassAudioService>();
            fingerprintCommandBuilderWithBass = new FingerprintCommandBuilder();
            DependencyResolver.Current.Bind<IAudioService, NAudioService>();
            fingerprintCommandBuilderWithNAudio = new FingerprintCommandBuilder();
            queryFingerprintService = new QueryFingerprintService(modelService);
        }

        [TestMethod]
        public void CreateFingerprintsFromDefaultFileAndAssertNumberOfFingerprints()
        {
            const int StaticStride = 5115;
            ITagService tagService = new BassTagService();
            
            var audioFingerprintingUnit = fingerprintCommandBuilderWithBass.BuildFingerprintCommand()
                                        .From(PathToMp3)
                                        .WithFingerprintConfig(config => { config.Stride = new IncrementalStaticStride(StaticStride, config.SamplesPerFingerprint); });
                                    
            double seconds = tagService.GetTagInfo(PathToMp3).Duration;
            int samples = (int)(seconds * audioFingerprintingUnit.FingerprintConfiguration.SampleRate);
            int expectedFingerprints = (samples / StaticStride) - 1;

            var fingerprints = audioFingerprintingUnit.Fingerprint().Result;

            Assert.AreEqual(expectedFingerprints, fingerprints.Count);
        }

        [TestMethod]
        public void CreateFingerprintsFromDefaultFileAndAssertNumberOfFingerprintsAndSubFingerprints()
        {
            var fingerprinter = fingerprintCommandBuilderWithBass.BuildFingerprintCommand()
                                        .From(PathToMp3)
                                        .WithDefaultFingerprintConfig()
                                        .Fingerprint();

            var fingerprints = fingerprinter.Result;
            var hashDatas = fingerprintCommandBuilderWithBass.BuildFingerprintCommand()
                                        .From(fingerprints)
                                        .WithDefaultFingerprintConfig()
                                        .Hash()
                                        .Result;

            Assert.AreEqual(fingerprints.Count, hashDatas.Count);
        }

        [TestMethod]
        public void CreateFingerprintsInsertThenQueryAndGetTheRightResult()
        {
            const int SecondsToProcess = 10;
            const int StartAtSecond = 30;
            var tagService = new BassTagService();
            TagInfo info = tagService.GetTagInfo(PathToMp3);
            TrackData track = new TrackData(
                info.ISRC, info.Artist, info.Title, info.Album, info.Year, (int)info.Duration);
            var trackReference = modelService.InsertTrack(track);

            var hashDatas = fingerprintCommandBuilderWithBass
                                            .BuildFingerprintCommand()
                                            .From(PathToMp3, SecondsToProcess, StartAtSecond)
                                            .WithDefaultFingerprintConfig()
                                            .Hash()
                                            .Result;

            modelService.InsertHashDataForTrack(hashDatas, trackReference);

            var queryResult = queryFingerprintService.Query(hashDatas, new DefaultQueryConfiguration());

            Assert.IsTrue(queryResult.IsSuccessful);
            Assert.AreEqual(1, queryResult.ResultEntries.Count);
            Assert.AreEqual(trackReference, queryResult.ResultEntries[0].Track.TrackReference);
        }

        [TestMethod]
        public void CreateFingerprintsFromFileAndFromAudioSamplesAndGetTheSameResultTest()
        {
            const int SecondsToProcess = 20;
            const int StartAtSecond = 15;
            using (var audioService = new BassAudioService())
            {
                float[] samples = audioService.ReadMonoFromFile(PathToMp3, SampleRate, SecondsToProcess, StartAtSecond);

                var hashDatasFromFile = fingerprintCommandBuilderWithBass
                                            .BuildFingerprintCommand()
                                            .From(PathToMp3, SecondsToProcess, StartAtSecond)
                                            .WithDefaultFingerprintConfig()
                                            .Hash()
                                            .Result;

                var hashDatasFromSamples = fingerprintCommandBuilderWithBass
                                            .BuildFingerprintCommand()
                                            .From(samples)
                                            .WithDefaultFingerprintConfig()
                                            .Hash()
                                            .Result;

                AssertHashDatasAreTheSame(hashDatasFromFile, hashDatasFromSamples);
            }
        }

        [TestMethod]
        public void CompareFingerprintsCreatedByDifferentProxiesTest()
        {
            var naudioFingerprints = fingerprintCommandBuilderWithNAudio.BuildFingerprintCommand()
                                                        .From(PathToMp3)
                                                        .WithDefaultFingerprintConfig()
                                                        .Fingerprint()
                                                        .Result;

            var bassFingerprints = fingerprintCommandBuilderWithBass.BuildFingerprintCommand()
                                                 .From(PathToMp3)
                                                 .WithDefaultFingerprintConfig()
                                                 .Fingerprint()
                                                 .Result;
            int unmatchedItems = 0;
            int totalmatches = 0;

            Assert.AreEqual(bassFingerprints.Count, naudioFingerprints.Count);
            for (
                int i = 0,
                    n = naudioFingerprints.Count > bassFingerprints.Count
                            ? bassFingerprints.Count
                            : naudioFingerprints.Count;
                i < n;
                i++)
            {
                for (int j = 0; j < naudioFingerprints[i].Length; j++)
                {
                    if (naudioFingerprints[i][j] != bassFingerprints[i][j])
                    {
                        unmatchedItems++;
                    }

                    totalmatches++;
                }
            }

            Assert.AreEqual(true, (float)unmatchedItems / totalmatches < 0.002, "Rate: " + ((float)unmatchedItems / totalmatches));
            Assert.AreEqual(bassFingerprints.Count, naudioFingerprints.Count);
        }

        [TestMethod]
        public void CheckFingerprintCreationAlgorithmTest()
        {
            using (BassAudioService bassAudioService = new BassAudioService())
            {
                string tempFile = Path.GetTempPath() + DateTime.Now.Ticks + ".wav";
                bassAudioService.RecodeFileToMonoWave(PathToMp3, tempFile, 5512);
                long fileSize = new FileInfo(tempFile).Length;
                
                var list = fingerprintCommandBuilderWithBass.BuildFingerprintCommand()
                                          .From(PathToMp3)
                                          .WithFingerprintConfig(customConfiguration => customConfiguration.Stride = new StaticStride(0, 0))
                                          .Fingerprint()
                                          .Result;

                long expected = fileSize / (8192 * 4); // One fingerprint corresponds to a granularity of 8192 samples which is 16384 bytes
                Assert.AreEqual(expected, list.Count);
                File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void CreateFingerprintsWithTheSameFingerprintCommandTest()
        {
            const int SecondsToProcess = 20;
            const int StartAtSecond = 15;

            var fingerprintCommand = fingerprintCommandBuilderWithBass
                                            .BuildFingerprintCommand()
                                            .From(PathToMp3, SecondsToProcess, StartAtSecond)
                                            .WithDefaultFingerprintConfig();
            
            var firstHashDatas = fingerprintCommand.Hash().Result;
            var secondHashDatas = fingerprintCommand.Hash().Result;

            AssertHashDatasAreTheSame(firstHashDatas, secondHashDatas);
        }

        [TestMethod]
        public void CreateFingerprintFromSamplesWhichAreExactlyEqualToMinimumLength()
        {
            DefaultFingerprintConfiguration config = new DefaultFingerprintConfiguration();

            float[] samples = TestUtilities.GenerateRandomFloatArray(config.SamplesPerFingerprint + config.WdftSize);

            var hash = fingerprintCommandBuilderWithBass
                                                .BuildFingerprintCommand()
                                                .From(samples)
                                                .WithDefaultFingerprintConfig()
                                                .Hash()
                                                .Result;
            Assert.AreEqual(1, hash.Count);
        }
    }
}
