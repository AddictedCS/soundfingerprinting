namespace Soundfingerprinting.UnitTests.Fingerprinting.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Soundfingerprinting.Audio.Services;
    using Soundfingerprinting.Audio.Strides;
    using Soundfingerprinting.Dao;
    using Soundfingerprinting.DbStorage.Entities;
    using Soundfingerprinting.DbStorage.Utils;
    using Soundfingerprinting.Fingerprinting;
    using Soundfingerprinting.Fingerprinting.Configuration;
    using Soundfingerprinting.Fingerprinting.FFT;
    using Soundfingerprinting.Fingerprinting.FFT.FFTW;
    using Soundfingerprinting.Fingerprinting.Wavelets;
    using Soundfingerprinting.Fingerprinting.WorkUnitBuilder;

    [TestClass]
    public class FingerprintManagerTest : BaseTest
    {
        private ModelService modelService;
        private IFingerprintService fingerprintService;
        private IFingerprintingUnitsBuilder fingerprintingUnitsBuilderWithBass;
        private IFingerprintingUnitsBuilder fingerprintingUnitsBuilderWithDirectSound;
        private IFingerprintingConfiguration defaultConfiguration;

        [TestInitialize]
        public void SetUp()
        {
            modelService = new ModelService(new MsSqlDatabaseProviderFactory(new DefaultConnectionStringFactory()), new ModelBinderFactory());
            fingerprintService = new FingerprintService(new FingerprintDescriptor(), new SpectrumService(new CachedFFTWService()), new WaveletService(new StandardHaarWaveletDecomposition()));
            defaultConfiguration = new DefaultFingerprintingConfiguration();
            fingerprintingUnitsBuilderWithBass = new FingerprintingUnitsBuilder(fingerprintService, new BassAudioService());
#pragma warning disable 612,618
            fingerprintingUnitsBuilderWithDirectSound = new FingerprintingUnitsBuilder(fingerprintService, new DirectSoundAudioService());
#pragma warning restore 612,618
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
            var signatures = fingerprintingUnitsBuilderWithDirectSound.BuildFingerprints()
                                            .On(PathToWav)
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
            var signatures = fingerprintingUnitsBuilderWithBass.BuildFingerprints()
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
            var directSoundFingerprints = fingerprintingUnitsBuilderWithDirectSound.BuildFingerprints()
                                                        .On(PathToWav)
                                                        .With(defaultConfiguration)
                                                        .RunAlgorithm()
                                                        .Result;

            var bassFingerprints = fingerprintingUnitsBuilderWithBass.BuildFingerprints()
                                                 .On(PathToMp3)
                                                 .With(defaultConfiguration)
                                                 .RunAlgorithm()
                                                 .Result;
            int unmatchedItems = 0;
            int totalmatches = 0;

            for (
                int i = 0,
                    n = directSoundFingerprints.Count > bassFingerprints.Count
                            ? bassFingerprints.Count
                            : directSoundFingerprints.Count;
                i < n;
                i++)
            {
                for (int j = 0; j < directSoundFingerprints[i].Length; j++)
                {
                    if (directSoundFingerprints[i][j] != bassFingerprints[i][j])
                    {
                        unmatchedItems++;
                    }

                    totalmatches++;
                }
            }

            Assert.AreEqual(true, (float)unmatchedItems / totalmatches < 0.02);
            Assert.AreEqual(bassFingerprints.Count, directSoundFingerprints.Count);
        }

        [TestMethod]
        public void CheckFingerprintCreationAlgorithmTest()
        {
            using (BassAudioService bassAudioService = new BassAudioService())
            {
                string tempFile = Path.GetTempPath() + DateTime.Now.Ticks + ".wav";
                bassAudioService.RecodeTheFile(PathToMp3, tempFile, 5512);

                long fileSize = new FileInfo(tempFile).Length;
                var list = fingerprintingUnitsBuilderWithBass.BuildFingerprints()
                                          .On(PathToMp3)
                                          .WithCustomConfiguration(customConfiguration => customConfiguration.Stride = new StaticStride(0, 0))
                                          .RunAlgorithm()
                                          .Result;
                long expected = fileSize / (8192 * 4); // One fingerprint corresponds to a granularity of 8192 samples which is 16384 bytes
                Assert.AreEqual(expected, list.Count);
                File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void GetDoubleArrayFromByteTest()
        {
            byte[] byteArray = TestUtilities.GenerateRandomInputByteArray(128 * 64);
            bool silence = false;
            float[] array = ArrayUtils.GetDoubleArrayFromSamples(byteArray, 128 * 64, ref silence);
            for (int i = 0; i < array.Length; i++)
            {
                switch (byteArray[i])
                {
                    case 255:
                        Assert.AreEqual(-1, array[i]);
                        break;
                    case 0:
                        Assert.AreEqual(0, array[i]);
                        break;
                    case 1:
                        Assert.AreEqual(1, array[i]);
                        break;
                    default:
                        Assert.Fail("Wrong input");
                        break;
                }
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