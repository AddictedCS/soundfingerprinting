namespace SoundFingerprinting.Tests.Integration.Fingerprinting
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Audio.Bass;
    using SoundFingerprinting.Audio.NAudio;
    using SoundFingerprinting.Dao;
    using SoundFingerprinting.Dao.Entities;
    using SoundFingerprinting.Hashing.MinHash;
    using SoundFingerprinting.Strides;
    using SoundFingerprinting.Tests.Integration;

    [TestClass]
    public class FingerprintUnitBuilderIntTest : AbstractIntegrationTest
    {
        private readonly ModelService modelService = new ModelService();
        private readonly IFingerprintUnitBuilder fingerprintUnitBuilderWithBass = new FingerprintUnitBuilder(new FingerprintService(), new BassAudioService(), new MinHashService(new DefaultPermutations()));
        private readonly IFingerprintUnitBuilder fingerprintUnitBuilderWithNAudio = new FingerprintUnitBuilder(new FingerprintService(), new NAudioService(), new MinHashService(new DefaultPermutations()));

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
            Album album = new Album("Track");
            modelService.InsertAlbum(album);
            Track track = new Track("Random", "Random", album.Id);
            modelService.InsertTrack(track);
            return track;
        }
    }
}