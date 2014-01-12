namespace SoundFingerprinting.Tests.Integration.Builder
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Audio.Bass;
    using SoundFingerprinting.Audio.NAudio;
    using SoundFingerprinting.Builder;
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
            var tagService = new BassAudioService();
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
            Assert.AreEqual(1, queryResult.Results.Count);
            Assert.AreEqual(trackReference.HashCode, queryResult.Results[0].Track.TrackReference.HashCode);
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

        private List<HashData> SortHashesByFirstValueOfHashBin(IEnumerable<HashData> hashDatasFromFile)
        {
            return hashDatasFromFile.OrderBy(hashData => hashData.HashBins[0]).ToList();
        }

        private void AssertHashDatasAreTheSame(List<HashData> firstHashDatas, List<HashData> secondHashDatas)
        {
            Assert.AreEqual(firstHashDatas.Count, secondHashDatas.Count);
         
            // hashes are not ordered the same way as parallel computation is involved
            firstHashDatas = SortHashesByFirstValueOfHashBin(firstHashDatas);
            secondHashDatas = SortHashesByFirstValueOfHashBin(secondHashDatas);

            for (int i = 0; i < firstHashDatas.Count; i++)
            {
                for (int j = 0; j < firstHashDatas[i].SubFingerprint.Length; j++)
                {
                    Assert.AreEqual(firstHashDatas[i].SubFingerprint[j], secondHashDatas[i].SubFingerprint[j]);
                }

                for (int j = 0; j < firstHashDatas[i].HashBins.Length; j++)
                {
                    Assert.AreEqual(firstHashDatas[i].HashBins[j], secondHashDatas[i].HashBins[j]);
                }
            }
        }
    }
}
