namespace Soundfingerprinting.UnitTests.Fingerprinting.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using Soundfingerprinting.AudioProxies;
    using Soundfingerprinting.DbStorage;
    using Soundfingerprinting.DbStorage.Entities;
    using Soundfingerprinting.DbStorage.Utils;
    using Soundfingerprinting.Fingerprinting;
    using Soundfingerprinting.Fingerprinting.Configuration;
    using Soundfingerprinting.Fingerprinting.Wavelets;
    using Soundfingerprinting.Fingerprinting.Windows;

    [TestClass]
    public class FingerprintManagerTest : BaseTest
    {
        private string connectionstring;

        private DaoGateway dalManager;
       
        private FingerprintService fingerService;

        private IFingerprintingConfiguration fingerprintingConfiguration;

        [TestInitialize]
        public void SetUp()
        {
            connectionstring = ConnectionString;
            dalManager = new DaoGateway(connectionstring);
            fingerService = new FingerprintService(
                new BassAudioService(),
                new DefaultFingerprintingConfiguration(),
                new FingerprintDescriptor(),
                new HanningWindow(),
                new HaarWavelet());
            fingerprintingConfiguration = new DefaultFingerprintingConfiguration();
            Mock<IFingerprintService> mock = new Mock<IFingerprintService>();
            mock.SetupSet(s => s.Far = 1);
        }

        [TestCleanup]
        public void TearDown()
        {
            List<Track> tracks = dalManager.ReadTracks();
            if (tracks != null)
            {
                dalManager.DeleteTrack(tracks);
            }
        }

        [TestMethod]
        public void CreateFingerprintsFromFileAndInsertInDatabaseUsingDirectSoundProxyTest()
        {
            Album album = new Album(0, "Random");
            dalManager.InsertAlbum(album);
            Track track = new Track(0, "Track", "Track", album.Id);
            dalManager.InsertTrack(track);
            List<Fingerprint> fingerprints;
            using (DirectSoundAudioService audioService = new DirectSoundAudioService())
            {
                fingerService.AudioServiceProxy = audioService;
                List<bool[]> signatures = fingerService.CreateFingerprintsFromSpectrum(PathToWav);
                fingerprints = Fingerprint.AssociateFingerprintsToTrack(signatures, track.Id);
                dalManager.InsertFingerprint(fingerprints);
            }

            List<Fingerprint> insertedFingerprints = dalManager.ReadFingerprintsByTrackId(track.Id, 0); /*Read all fingerprints*/
            Assert.AreEqual(fingerprints.Count, insertedFingerprints.Count);

            foreach (Fingerprint fingerprint in fingerprints)
            {
                foreach (Fingerprint insertedFingerprint in insertedFingerprints)
                {
                    if (fingerprint.Id == insertedFingerprint.Id)
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
        }

        [TestMethod]
        public void CreateFingerprintsFromFileAndInsertInDatabaseUsingBassProxyTest()
        {
            Album album = new Album(0, "Track");
            dalManager.InsertAlbum(album);
            Track track = new Track(0, "Random", "Random", album.Id);

            dalManager.InsertTrack(track);
            List<Fingerprint> fingerprints;
            using (BassAudioService audioService = new BassAudioService())
            {
                fingerService.AudioServiceProxy = audioService;
                List<bool[]> signatures = fingerService.CreateFingerprintsFromSpectrum(PathToMp3);
                fingerprints = Fingerprint.AssociateFingerprintsToTrack(signatures, track.Id);
                dalManager.InsertFingerprint(fingerprints);
            }

            List<Fingerprint> insertedFingerprints = dalManager.ReadFingerprintsByTrackId(track.Id, 0);
            Assert.AreEqual(fingerprints.Count, insertedFingerprints.Count);

            foreach (Fingerprint f in fingerprints)
            {
                foreach (Fingerprint iFingerprint in insertedFingerprints.Where(iFingerprint => f.Id == iFingerprint.Id))
                {
                    Assert.AreEqual(f.Signature.Length, iFingerprint.Signature.Length);
                    for (int i = 0; i < f.Signature.Length; i++)
                    {
                        Assert.AreEqual(f.Signature[i], iFingerprint.Signature[i]);
                    }

                    Assert.AreEqual(f.TotalFingerprintsPerTrack, iFingerprint.TotalFingerprintsPerTrack);
                    Assert.AreEqual(f.TrackId, iFingerprint.TrackId);
                }
            }
        }

        [TestMethod]
        public void CreateFingerprintsFromFileAndInsertInDatabaseUsingDirectSoundProxyCheckCorrectitudeOfFingerprintsTest()
        {
            Album album = new Album(0, "Random");
            dalManager.InsertAlbum(album);
            Track track = new Track(0, "Random", "Random", album.Id);
            dalManager.InsertTrack(track);
            List<Fingerprint> fingerprints;
            List<Fingerprint> insertedFingerprints;
            using (DirectSoundAudioService audioService = new DirectSoundAudioService())
            {
                fingerService.AudioServiceProxy = audioService;
                List<bool[]> signatures = fingerService.CreateFingerprintsFromSpectrum(PathToWav);
                fingerprints = Fingerprint.AssociateFingerprintsToTrack(signatures, track.Id);
                dalManager.InsertFingerprint(fingerprints);
                signatures = fingerService.CreateFingerprintsFromSpectrum(PathToWav);
                insertedFingerprints = Fingerprint.AssociateFingerprintsToTrack(signatures, track.Id);
            }

            Assert.AreEqual(fingerprints.Count, insertedFingerprints.Count);

            foreach (Fingerprint f in fingerprints)
            {
                foreach (Fingerprint iFingerprint in insertedFingerprints.Where(iFingerprint => f.Id == iFingerprint.Id))
                {
                    Assert.AreEqual(f.Signature.Length, iFingerprint.Signature.Length);
                    for (int i = 0; i < f.Signature.Length; i++)
                    {
                        Assert.AreEqual(f.Signature[i], iFingerprint.Signature[i]);
                    }

                    Assert.AreEqual(f.TotalFingerprintsPerTrack, iFingerprint.TotalFingerprintsPerTrack);
                    Assert.AreEqual(f.TrackId, iFingerprint.TrackId);
                }
            }
        }

        [TestMethod]
        public void CreateFingerprintsFromFileAndInsertInDatabaseUsingBassProxyCheckCorrectitudeOfFingerprintsTest()
        {
            Album album = new Album(0, "Sample");
            dalManager.InsertAlbum(album);
            Track track = new Track(0, "Sample", "Sample", album.Id);

            dalManager.InsertTrack(track);
            List<Fingerprint> fingerprints;
            List<Fingerprint> insertedFingerprints;
            using (BassAudioService audioService = new BassAudioService())
            {
                fingerService.AudioServiceProxy = audioService;
                List<bool[]> signatures = fingerService.CreateFingerprintsFromSpectrum(PathToMp3);
                fingerprints = Fingerprint.AssociateFingerprintsToTrack(signatures, track.Id);
                dalManager.InsertFingerprint(fingerprints);
                signatures = fingerService.CreateFingerprintsFromSpectrum(PathToMp3);
                insertedFingerprints = Fingerprint.AssociateFingerprintsToTrack(signatures, track.Id);
            }

            Assert.AreEqual(fingerprints.Count, insertedFingerprints.Count);

            foreach (Fingerprint f in fingerprints)
            {
                foreach (var iFingerprint in insertedFingerprints)
                {
                    if (f.Id == iFingerprint.Id)
                    {
                        Assert.AreEqual(f.Signature.Length, iFingerprint.Signature.Length);
                        for (int i = 0; i < f.Signature.Length; i++)
                        {
                            Assert.AreEqual(f.Signature[i], iFingerprint.Signature[i]);
                        }

                        Assert.AreEqual(f.TotalFingerprintsPerTrack, iFingerprint.TotalFingerprintsPerTrack);
                        Assert.AreEqual(f.TrackId, iFingerprint.TrackId);
                    }
                }
            }
        }


        [TestMethod]
        public void CompareFingerprintsCreatedByDifferentProxiesTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;

            DirectSoundAudioService dsAudioService = new DirectSoundAudioService();
            BassAudioService bsAudioService = new BassAudioService();
            fingerService.AudioServiceProxy = bsAudioService;
            List<bool[]> dsFingers = fingerService.CreateFingerprintsFromSpectrum(PathToWav);
            List<bool[]> bFingers = fingerService.CreateFingerprintsFromSpectrum(PathToMp3);
            int unmatchedItems = 0;
            int totalmatches = 0;
            //Check how many bytes are different while comparing BASS Fingers and DS Fingers (normaly ~1%)
            for (int i = 0, n = dsFingers.Count > bFingers.Count ? bFingers.Count : dsFingers.Count; i < n; i++)
            {
                for (int j = 0; j < dsFingers[i].Length; j++)
                {
                    if (dsFingers[i][j] != bFingers[i][j]) unmatchedItems++;
                    totalmatches++;
                }
            }

            Assert.AreEqual(true, (float)unmatchedItems / totalmatches < 0.02); /*less than 1.5% difference*/
            Assert.AreEqual(bFingers.Count, dsFingers.Count);
            dsAudioService.Dispose();
            bsAudioService.Dispose();
        }

        [TestMethod]
        public void CreateSeveralFingerprintsTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            using (DirectSoundAudioService dsAudioService = new DirectSoundAudioService())
            {
                using (BassAudioService bsAudioService = new BassAudioService())
                {
                    fingerService.AudioServiceProxy = dsAudioService;
                    List<bool[]> dsFingers = fingerService.CreateFingerprintsFromSpectrum(PathToWav);
                    fingerService.AudioServiceProxy = bsAudioService;
                    List<bool[]> bFingers = fingerService.CreateFingerprintsFromSpectrum(PathToMp3);
                    Assert.AreEqual(dsFingers.Count, bFingers.Count);
                    int unmatched = 0;
                    for (int i = 0, n = dsFingers.Count; i < n; i++) for (int j = 0; j < dsFingers[i].Length; j++) if (dsFingers[i][j] != bFingers[i][j]) unmatched++;
                    int totalElements = dsFingers.Count * dsFingers[0].Length;
                    Assert.AreEqual(true, (float)unmatched / totalElements < 0.02);
                }
            }
        }


        [TestMethod]
        public void GetDoubleArrayFromByteTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
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

        [TestMethod]
        public void CheckFingerprintCreationAlgorithmTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            using (BassAudioService bassAudioService = new BassAudioService())
            {
                string tempFile = Path.GetTempPath() + 0 + ".wav";
                bassAudioService.RecodeTheFile(PathToMp3, tempFile, 5512);

                long fileSize = new FileInfo(tempFile).Length;
                BassAudioService audioService = new BassAudioService();
                fingerService.AudioServiceProxy = audioService;
                List<bool[]> list = fingerService.CreateFingerprintsFromSpectrum(PathToMp3);

                // One fingerprint corresponds to a granularity of 8192 samples which is 16384 bytes
                long expected = fileSize / (fingerprintingConfiguration.SamplesPerFingerprint * 4);
                Assert.AreEqual(expected, list.Count);
                audioService.Dispose();
                File.Delete(tempFile);
            }
        }

        /// <summary>
        ///   Check whether the # of fingerprints returned from the creation process is Ok
        /// </summary>
        [TestMethod]
        public void CheckFingerprintCreationAlgorithmTest1()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            long fileSize = new FileInfo(PathToWav).Length;
#pragma warning disable 612,618
            using (IAudioService proxy = new DirectSoundAudioService())
            {
#pragma warning restore 612,618
                fingerService.AudioServiceProxy = proxy;
                List<bool[]> list = fingerService.CreateFingerprintsFromSpectrum(PathToWav);

                // One fingerprint corresponds to a granularity of 8192 samples which is 16384 bytes
                long expected = fileSize / (8192 * 4);
                Assert.AreEqual(expected, list.Count);
                proxy.Dispose();
            }
        }

        [TestMethod]
        public void CheckFingerprintCreationAlgorithmTest2()
        {
             using (IAudioService proxy = new BassAudioService())
            {
                fingerService.AudioServiceProxy = proxy;
                List<bool[]> listBs = fingerService.CreateFingerprintsFromSpectrum(PathToMp3);
                // One fingerprint corresponds to a granularity of 8192 samples which is 16384 bytes
                using (IAudioService directSoundProxy = new DirectSoundAudioService())
                {
                    List<bool[]> listDs = fingerService.CreateFingerprintsFromSpectrum(PathToWav);
                    Assert.AreEqual(listBs.Count, listDs.Count);
                    proxy.Dispose();
                    directSoundProxy.Dispose();
                }
            }
        }
    }
}