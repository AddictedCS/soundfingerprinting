namespace SoundFingerprinting.Tests.Integration.Fingerprinting
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Audio.Bass;
    using SoundFingerprinting.Audio.NAudio;
    using SoundFingerprinting.Dao;
    using SoundFingerprinting.Dao.Entities;
    using SoundFingerprinting.Hashing;
    using SoundFingerprinting.Hashing.LSH;
    using SoundFingerprinting.Hashing.MinHash;
    using SoundFingerprinting.Query;
    using SoundFingerprinting.Query.Configuration;
    using SoundFingerprinting.Strides;
    using SoundFingerprinting.Tests.Integration;

    [TestClass]
    public class FingerprintUnitBuilderIntTest : AbstractIntegrationTest
    {
        private readonly ModelService modelService = new ModelService();
        private readonly IFingerprintUnitBuilder fingerprintUnitBuilderWithBass = new FingerprintUnitBuilder(new FingerprintService(), new BassAudioService(), new MinHashService(new DefaultPermutations()));
        private readonly IFingerprintUnitBuilder fingerprintUnitBuilderWithNAudio = new FingerprintUnitBuilder(new FingerprintService(), new NAudioService(), new MinHashService(new DefaultPermutations()));
        private readonly ILSHService lshService = new LSHService();

        [TestMethod]
        public void CreateFingerprintsFromDefaultFileAndAssertNumberOfFingerprints()
        {
            const int StaticStride = 5115;
            ITagService tagService = new BassAudioService();
            
            var audioFingerprintingUnit = fingerprintUnitBuilderWithBass.BuildAudioFingerprintingUnit()
                                        .From(PathToMp3)
                                        .WithCustomAlgorithmConfiguration(config => { config.Stride = new IncrementalStaticStride(StaticStride, config.SamplesPerFingerprint); });
                                    
            double seconds = tagService.GetTagInfo(PathToMp3).Duration;
            int samples = (int)(seconds * audioFingerprintingUnit.Configuration.SampleRate);
            int expectedFingerprints = (samples / StaticStride) - 1;

            var fingerprints = audioFingerprintingUnit.FingerprintIt().AsIs().Result;

            Assert.AreEqual(expectedFingerprints, fingerprints.Count);
        }

        [TestMethod]
        public void CreateFingerprintsFromDefaultFileAndAssertNumberOfFingerprintsAndSubFingerprints()
        {
            var fingerprinter = fingerprintUnitBuilderWithBass.BuildAudioFingerprintingUnit()
                                        .From(PathToMp3)
                                        .WithDefaultAlgorithmConfiguration()
                                        .FingerprintIt();

            var fingerprints = fingerprinter.AsIs().Result;
            var subFingerprints = fingerprinter.HashIt().AsIs().Result;

            Assert.AreEqual(fingerprints.Count, subFingerprints.Count);
        }

        [TestMethod]
        public void CreateFingerprintsFromFileAndInsertInDatabaseUsingDirectSoundProxyTest()
        {
            var track = InsertTrack();
            var fingerprints = fingerprintUnitBuilderWithNAudio.BuildAudioFingerprintingUnit()
                                            .From(PathToMp3)
                                            .WithDefaultAlgorithmConfiguration()
                                            .FingerprintIt()
                                            .ForTrack(track.Id)
                                            .Result;

            modelService.InsertFingerprint(fingerprints);
            var insertedFingerprints = modelService.ReadFingerprintsByTrackId(track.Id, 0);
            
            AssertFingerprintsAreEquals(fingerprints, insertedFingerprints);
        }

        [TestMethod]
        public void CreateFingerprintsInsertThenQueryAndGetTheRightResult()
        {
            const int StaticStride = 5115;
            const int SecondsToProcess = 10;
            const int StartAtSecond = 30;
            DefaultQueryConfiguration defaultQueryConfiguration = new DefaultQueryConfiguration();
            QueryFingerprintService queryFingerprintService = new QueryFingerprintService(new CombinedHashingAlgorithm(), modelService);
            ITagService tagService = new BassAudioService();
            TagInfo info = tagService.GetTagInfo(PathToMp3);
            int releaseYear = info.Year;
            Track track = new Track(info.ISRC, info.Artist, info.Title, info.Album, releaseYear, (int)info.Duration);

            modelService.InsertTrack(track);

            var fingerprinter = fingerprintUnitBuilderWithBass.BuildAudioFingerprintingUnit()
                                            .From(PathToMp3, SecondsToProcess, StartAtSecond)
                                            .WithCustomAlgorithmConfiguration(config =>
                                            {
                                                config.Stride = new IncrementalStaticStride(StaticStride, config.SamplesPerFingerprint);
                                            })
                                            .FingerprintIt();

            var fingerprints = fingerprinter.AsIs().Result;
            var subFingerprints = fingerprinter.HashIt().ForTrack(track.Id).Result;

            modelService.InsertSubFingerprint(subFingerprints);

            List<HashBinMinHash> hashBins = new List<HashBinMinHash>();
            foreach (SubFingerprint subFingerprint in subFingerprints)
            {
                long[] groupedSubFingerprint = lshService.Hash(subFingerprint.Signature, defaultQueryConfiguration.NumberOfLSHTables, defaultQueryConfiguration.NumberOfMinHashesPerTable);
                for (int i = 0; i < groupedSubFingerprint.Length; i++)
                {
                    int tableNumber = i + 1;
                    hashBins.Add(new HashBinMinHash(groupedSubFingerprint[i], tableNumber, subFingerprint.Id));
                }
            }

            modelService.InsertHashBin(hashBins);

            QueryResult result = queryFingerprintService.Query(fingerprints, defaultQueryConfiguration);

            Assert.IsTrue(result.IsSuccessful);
            Assert.AreEqual(track.Id, result.BestMatch.Id);
        }

        [TestMethod]
        public void CreateFingerprintsFromFileAndInsertInDatabaseUsingBassProxyTest()
        {
            var track = InsertTrack();
            var fingerprints = fingerprintUnitBuilderWithBass.BuildAudioFingerprintingUnit()
                                            .From(PathToMp3)
                                            .WithDefaultAlgorithmConfiguration()
                                            .FingerprintIt()
                                            .ForTrack(track.Id)
                                            .Result;

            modelService.InsertFingerprint(fingerprints);
            var insertedFingerprints = modelService.ReadFingerprintsByTrackId(track.Id, 0);

            AssertFingerprintsAreEquals(fingerprints, insertedFingerprints);
        }

        [TestMethod]
        public void CompareFingerprintsCreatedByDifferentProxiesTest()
        {
            var naudioFingerprints = fingerprintUnitBuilderWithNAudio.BuildAudioFingerprintingUnit()
                                                        .From(PathToMp3)
                                                        .WithDefaultAlgorithmConfiguration()
                                                        .FingerprintIt()
                                                        .AsIs()
                                                        .Result;

            var bassFingerprints = fingerprintUnitBuilderWithBass.BuildAudioFingerprintingUnit()
                                                 .From(PathToMp3)
                                                 .WithDefaultAlgorithmConfiguration()
                                                 .FingerprintIt()
                                                 .AsIs()
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
                var list = fingerprintUnitBuilderWithBass.BuildAudioFingerprintingUnit()
                                          .From(PathToMp3)
                                          .WithCustomAlgorithmConfiguration(customConfiguration => customConfiguration.Stride = new StaticStride(0, 0))
                                          .FingerprintIt()
                                          .AsIs()
                                          .Result;
                long expected = fileSize / (8192 * 4); // One fingerprint corresponds to a granularity of 8192 samples which is 16384 bytes
                Assert.AreEqual(expected, list.Count);
                File.Delete(tempFile);
            }
        }

        private void AssertFingerprintsAreEquals(IReadOnlyCollection<Fingerprint> fingerprints, ICollection<Fingerprint> insertedFingerprints)
        {
            Assert.AreEqual(fingerprints.Count, insertedFingerprints.Count);
            foreach (var fingerprint in fingerprints)
            {
                int fingerprintId = fingerprint.Id;
                foreach (var insertedFingerprint in
                    insertedFingerprints.Where(fingerprintSignature => fingerprintSignature.Id == fingerprintId))
                {
                    Assert.AreEqual(fingerprint.Signature.Length, insertedFingerprint.Signature.Length);

                    for (int i = 0; i < fingerprint.Signature.Length; i++)
                    {
                        Assert.AreEqual(fingerprint.Signature[i], insertedFingerprint.Signature[i]);
                    }

                    Assert.AreEqual(fingerprint.TotalFingerprintsPerTrack, insertedFingerprint.TotalFingerprintsPerTrack);
                    Assert.AreEqual(fingerprint.TrackId, insertedFingerprint.TrackId);
                }
            }
        }

        private Track InsertTrack()
        {
            Track track = new Track("ISRC", "Artist", "Title", "Album", 1986, 360);
            modelService.InsertTrack(track);
            return track;
        }
    }
}
