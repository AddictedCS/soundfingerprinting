namespace SoundFingerprinting.SQL.Tests.Integration
{
    using System;
    using System.IO;
    using System.Transactions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Audio.Bass;
    using SoundFingerprinting.Audio.NAudio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Strides;
    using SoundFingerprinting.Tests;
    using SoundFingerprinting.Tests.Integration;

    [TestClass]
    public class FingerprintCommandBuilderIntTest : AbstractIntegrationTest
    {
        private readonly ModelService modelService;
        private readonly IFingerprintCommandBuilder fingerprintCommandBuilder;
        private readonly IQueryFingerprintService queryFingerprintService;
        private readonly BassAudioService bassAudioService;
        private readonly NAudioService naudioAudioService;

        private TransactionScope transactionPerTestScope;

        public FingerprintCommandBuilderIntTest()
        {
            bassAudioService = new BassAudioService();
            naudioAudioService = new NAudioService();
            modelService = new SqlModelService();
            fingerprintCommandBuilder = new FingerprintCommandBuilder();
            queryFingerprintService = new QueryFingerprintService();
        }

        [TestInitialize]
        public void SetUp()
        {
            transactionPerTestScope = new TransactionScope();
        }

        [TestCleanup]
        public void TearDown()
        {
            transactionPerTestScope.Dispose();
        }

        [TestMethod]
        public void CreateFingerprintsFromDefaultFileAndAssertNumberOfFingerprints()
        {
            const int StaticStride = 5115;
            var tagService = new BassTagService();

            var audioFingerprintingUnit = fingerprintCommandBuilder.BuildFingerprintCommand()
                                        .From(PathToMp3)
                                        .WithFingerprintConfig(config => { config.Stride = new IncrementalStaticStride(StaticStride, config.SamplesPerFingerprint); })
                                        .UsingServices(bassAudioService);
                                    
            double seconds = tagService.GetTagInfo(PathToMp3).Duration;
            int samples = (int)(seconds * audioFingerprintingUnit.FingerprintConfiguration.SampleRate);
            int expectedFingerprints = (samples / StaticStride) - 1;

            var fingerprints = audioFingerprintingUnit.Fingerprint().Result;

            Assert.AreEqual(expectedFingerprints, fingerprints.Count);
        }

        [TestMethod]
        public void CreateFingerprintsFromDefaultFileAndAssertNumberOfFingerprintsAndSubFingerprints()
        {
            var fingerprinter = fingerprintCommandBuilder.BuildFingerprintCommand()
                                        .From(PathToMp3)
                                        .WithDefaultFingerprintConfig()
                                        .UsingServices(bassAudioService)
                                        .Fingerprint();

            var fingerprints = fingerprinter.Result;
            var hashDatas = fingerprintCommandBuilder.BuildFingerprintCommand()
                                        .From(fingerprints)
                                        .WithDefaultFingerprintConfig()
                                        .UsingServices(bassAudioService)
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
            var info = tagService.GetTagInfo(PathToMp3);
            var track = new TrackData(info.ISRC, info.Artist, info.Title, info.Album, info.Year, (int)info.Duration);
            var trackReference = modelService.InsertTrack(track);

            var hashDatas = fingerprintCommandBuilder
                                            .BuildFingerprintCommand()
                                            .From(PathToMp3, SecondsToProcess, StartAtSecond)
                                            .WithDefaultFingerprintConfig()
                                            .UsingServices(bassAudioService)
                                            .Hash()
                                            .Result;

            modelService.InsertHashDataForTrack(hashDatas, trackReference);

            var queryResult = queryFingerprintService.Query(modelService, hashDatas, new DefaultQueryConfiguration());

            Assert.IsTrue(queryResult.IsSuccessful);
            Assert.AreEqual(1, queryResult.ResultEntries.Count);
            Assert.AreEqual(trackReference, queryResult.ResultEntries[0].Track.TrackReference);
        }

        [TestMethod]
        public void CreateFingerprintsFromFileAndFromAudioSamplesAndGetTheSameResultTest()
        {
            const int SecondsToProcess = 20;
            const int StartAtSecond = 15;
            var audioService = new BassAudioService();

            float[] samples = audioService.ReadMonoSamplesFromFile(PathToMp3, SampleRate, SecondsToProcess, StartAtSecond);

            var hashDatasFromFile = fingerprintCommandBuilder
                                        .BuildFingerprintCommand()
                                        .From(PathToMp3, SecondsToProcess, StartAtSecond)
                                        .WithDefaultFingerprintConfig()
                                        .UsingServices(bassAudioService)
                                        .Hash()
                                        .Result;

            var hashDatasFromSamples = fingerprintCommandBuilder
                                        .BuildFingerprintCommand()
                                        .From(samples)
                                        .WithDefaultFingerprintConfig()
                                        .UsingServices(bassAudioService)
                                        .Hash()
                                        .Result;

            AssertHashDatasAreTheSame(hashDatasFromFile, hashDatasFromSamples);
        }

        [TestMethod]
        public void CompareFingerprintsCreatedByDifferentProxiesTest()
        {
            var naudioFingerprints = fingerprintCommandBuilder.BuildFingerprintCommand()
                                                        .From(PathToMp3)
                                                        .WithDefaultFingerprintConfig()
                                                        .UsingServices(naudioAudioService)
                                                        .Fingerprint()
                                                        .Result;

            var bassFingerprints = fingerprintCommandBuilder.BuildFingerprintCommand()
                                                 .From(PathToMp3)
                                                 .WithDefaultFingerprintConfig()
                                                 .UsingServices(bassAudioService)
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
            string tempFile = Path.GetTempPath() + DateTime.Now.Ticks + ".wav";
            bassAudioService.RecodeFileToMonoWave(PathToMp3, tempFile, 5512);
            long fileSize = new FileInfo(tempFile).Length;

            var list = fingerprintCommandBuilder.BuildFingerprintCommand()
                                      .From(PathToMp3)
                                      .WithFingerprintConfig(customConfiguration => customConfiguration.Stride = new StaticStride(0, 0))
                                      .UsingServices(bassAudioService)
                                      .Fingerprint()
                                      .Result;

            long expected = fileSize / (8192 * 4); // One fingerprint corresponds to a granularity of 8192 samples which is 16384 bytes
            Assert.AreEqual(expected, list.Count);
            File.Delete(tempFile);
        }

        [TestMethod]
        public void CreateFingerprintsWithTheSameFingerprintCommandTest()
        {
            const int SecondsToProcess = 20;
            const int StartAtSecond = 15;

            var fingerprintCommand = fingerprintCommandBuilder
                                            .BuildFingerprintCommand()
                                            .From(PathToMp3, SecondsToProcess, StartAtSecond)
                                            .WithDefaultFingerprintConfig()
                                            .UsingServices(bassAudioService);
            
            var firstHashDatas = fingerprintCommand.Hash().Result;
            var secondHashDatas = fingerprintCommand.Hash().Result;

            AssertHashDatasAreTheSame(firstHashDatas, secondHashDatas);
        }

        [TestMethod]
        public void CreateFingerprintFromSamplesWhichAreExactlyEqualToMinimumLength()
        {
            DefaultFingerprintConfiguration config = new DefaultFingerprintConfiguration();

            float[] samples = TestUtilities.GenerateRandomFloatArray(config.SamplesPerFingerprint + config.WdftSize);

            var hash = fingerprintCommandBuilder.BuildFingerprintCommand()
                                                .From(samples)
                                                .WithDefaultFingerprintConfig()
                                                .UsingServices(bassAudioService)
                                                .Hash()
                                                .Result;
            Assert.AreEqual(1, hash.Count);
        }
    }
}
