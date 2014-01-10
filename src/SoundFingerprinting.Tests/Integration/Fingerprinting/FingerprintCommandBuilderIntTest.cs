namespace SoundFingerprinting.Tests.Integration.Fingerprinting
{
    using System;
    using System.IO;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Audio.Bass;
    using SoundFingerprinting.Audio.NAudio;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Dao;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Hashing.LSH;
    using SoundFingerprinting.Hashing.MinHash;
    using SoundFingerprinting.Strides;
    using SoundFingerprinting.Tests.Integration;

    [TestClass]
    public class FingerprintCommandBuilderIntTest : AbstractIntegrationTest
    {
        private readonly ModelService modelService;

        private readonly IFingerprintCommandBuilder fingerprintCommandBuilderWithBass;

        private readonly IFingerprintCommandBuilder fingerprintCommandBuilderWithNAudio;

        private readonly IQueryFingerprintService queryFingerprintService;

        public FingerprintCommandBuilderIntTest()
        {
            modelService = new ModelService();
            var fingerprintService = new FingerprintService();
            var minHashService = new MinHashService(new DefaultPermutations());
            var lshService = new LSHService();
            fingerprintCommandBuilderWithBass = new FingerprintCommandBuilder(fingerprintService, new BassAudioService(), minHashService, lshService);
            fingerprintCommandBuilderWithNAudio = new FingerprintCommandBuilder(fingerprintService, new NAudioService(), minHashService, lshService);
            queryFingerprintService = new QueryFingerprintService(modelService);
        }

        [TestMethod]
        public void CreateFingerprintsFromDefaultFileAndAssertNumberOfFingerprints()
        {
            const int StaticStride = 5115;
            ITagService tagService = new BassAudioService();
            
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
                                        .From(PathToMp3)
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
            ITagService tagService = new BassAudioService();
            TagInfo info = tagService.GetTagInfo(PathToMp3);
            int releaseYear = info.Year;
            TrackData track = new TrackData(info.ISRC, info.Artist, info.Title, info.Album, releaseYear, (int)info.Duration);
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
            Assert.AreEqual(trackReference.HashCode, queryResult.BestMatch.TrackReference.HashCode);
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

            Assert.AreEqual(true, (float)unmatchedItems / totalmatches < 0.02);
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
    }
}
