namespace SoundFingerprinting.Tests.Fingerprinting.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SoundFingerprinting.Audio.Bass;
    using SoundFingerprinting.Audio.NAudio;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Dao;
    using SoundFingerprinting.Dao.Entities;
    using SoundFingerprinting.FFT;
    using SoundFingerprinting.FFT.FFTW;
    using SoundFingerprinting.Hashing.MinHash;
    using SoundFingerprinting.Strides;
    using SoundFingerprinting.Utils;
    using SoundFingerprinting.Wavelets;

    [TestClass]
    public class FingerprintUnitBuilderIntTest : BaseTest
    {
        private ModelService modelService;
        private IFingerprintService fingerprintService;
        private IFingerprintUnitBuilder fingerprintUnitBuilderWithBass;
        private IFingerprintUnitBuilder fingerprintUnitBuilderWithNAudio;
        private IFingerprintingConfiguration defaultConfiguration;
        private IPermutations permutations;

        [TestInitialize]
        public void SetUp()
        {
            modelService = new ModelService(new MsSqlDatabaseProviderFactory(new DefaultConnectionStringFactory()), new ModelBinderFactory());
            fingerprintService = new FingerprintService(new FingerprintDescriptor(), new SpectrumService(new CachedFFTWService(new FFTWService86())), new WaveletService(new StandardHaarWaveletDecomposition()));
            defaultConfiguration = new DefaultFingerprintingConfiguration();
            permutations = new DefaultPermutations();
            fingerprintUnitBuilderWithBass = new FingerprintUnitBuilder(fingerprintService, new BassAudioService(), new MinHashService(permutations));
            fingerprintUnitBuilderWithNAudio = new FingerprintUnitBuilder(fingerprintService, new NAudioService(), new MinHashService(permutations));
        }

        [TestCleanup]
        public void TearDown()
        {
            var tracks = modelService.ReadTracks();
            if (tracks != null)
            {
                modelService.DeleteTrack(tracks);
            }
        }

        [TestMethod]
        public void CreateFingerprintsFromFileAndInsertInDatabaseUsingDirectSoundProxyTest()
        {
            var track = InsertTrack();
            var signatures = fingerprintUnitBuilderWithNAudio.BuildFingerprints()
                                            .On(PathToMp3)
                                            .With(defaultConfiguration)
                                            .RunAlgorithm()
                                            .Result;

            var fingerprints = AssociateFingerprintsToTrack(signatures, track.Id);
            modelService.InsertFingerprint(fingerprints);
            var insertedFingerprints = modelService.ReadFingerprintsByTrackId(track.Id, 0);
            
            AssertFingerprintsAreEquals(fingerprints, insertedFingerprints);
        }

        [TestMethod]
        public void CreateFingerprintsFromFileAndInsertInDatabaseUsingBassProxyTest()
        {
            var track = InsertTrack();
            var signatures = fingerprintUnitBuilderWithBass.BuildFingerprints()
                                            .On(PathToMp3)
                                            .With(defaultConfiguration)
                                            .RunAlgorithm()
                                            .Result;

            var fingerprints = AssociateFingerprintsToTrack(signatures, track.Id);
            modelService.InsertFingerprint(fingerprints);
            var insertedFingerprints = modelService.ReadFingerprintsByTrackId(track.Id, 0);

            AssertFingerprintsAreEquals(fingerprints, insertedFingerprints);
        }

        [TestMethod]
        public void CompareFingerprintsCreatedByDifferentProxiesTest()
        {
            var naudioFingerprints = fingerprintUnitBuilderWithNAudio.BuildFingerprints()
                                                        .On(PathToMp3)
                                                        .With(defaultConfiguration)
                                                        .RunAlgorithm()
                                                        .Result;

            var bassFingerprints = fingerprintUnitBuilderWithBass.BuildFingerprints()
                                                 .On(PathToMp3)
                                                 .With(defaultConfiguration)
                                                 .RunAlgorithm()
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
                var list = fingerprintUnitBuilderWithBass.BuildFingerprints()
                                          .On(PathToMp3)
                                          .WithCustomConfiguration(customConfiguration => customConfiguration.Stride = new StaticStride(0, 0))
                                          .RunAlgorithm()
                                          .Result;
                long expected = fileSize / (8192 * 4); // One fingerprint corresponds to a granularity of 8192 samples which is 16384 bytes
                Assert.AreEqual(expected, list.Count);
                File.Delete(tempFile);
            }
        }

        private void AssertFingerprintsAreEquals(List<Fingerprint> fingerprints, IList<Fingerprint> insertedFingerprints)
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
            Album album = new Album(0, "Track");
            modelService.InsertAlbum(album);
            Track track = new Track(0, "Random", "Random", album.Id);
            modelService.InsertTrack(track);
            return track;
        }

        private List<Fingerprint> AssociateFingerprintsToTrack(IEnumerable<bool[]> fingerprintSignatures, int trackId)
        {
            const int FakeId = -1;
            List<Fingerprint> fingers = new List<Fingerprint>();
            int c = 0;
            foreach (bool[] signature in fingerprintSignatures)
            {
                fingers.Add(new Fingerprint(FakeId, signature, trackId, c));
                c++;
            }

            return fingers;
        }
    }
}