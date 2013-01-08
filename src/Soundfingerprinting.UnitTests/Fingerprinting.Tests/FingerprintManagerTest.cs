namespace Soundfingerprinting.UnitTests.Fingerprinting.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Soundfingerprinting.Audio.Services;
    using Soundfingerprinting.DbStorage;
    using Soundfingerprinting.DbStorage.Entities;
    using Soundfingerprinting.DbStorage.Utils;
    using Soundfingerprinting.Fingerprinting;
    using Soundfingerprinting.Fingerprinting.Configuration;
    using Soundfingerprinting.Fingerprinting.Wavelets;
    using Soundfingerprinting.Fingerprinting.WorkUnitBuilder;

    [TestClass]
    public class FingerprintManagerTest : BaseTest
    {
        private string connectionstring;

        private DaoGateway dalManager;

        private IFingerprintService fingerprintingServiceWithBass;

        private IFingerprintService fingerprintingServiceWithDirectSound;

        private IWorkUnitBuilder workUnitBuilder;

        private IFingerprintingConfiguration fingerprintingConfiguration;

        [TestInitialize]
        public void SetUp()
        {
            connectionstring = ConnectionString;
            dalManager = new DaoGateway(connectionstring);
            fingerprintingServiceWithBass = new FingerprintService(
                new BassAudioService(), new FingerprintDescriptor(), new HaarWavelet());

            fingerprintingServiceWithDirectSound = new FingerprintService(
                new DirectSoundAudioService(), new FingerprintDescriptor(), new HaarWavelet());

            fingerprintingConfiguration = new DefaultFingerprintingConfiguration();
            workUnitBuilder = new WorkUnitBuilder();
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
            List<bool[]> signatures =
                workUnitBuilder.BuildWorkUnit().On(PathToWav).With(fingerprintingConfiguration).
                    GetFingerprintsUsingService(fingerprintingServiceWithDirectSound).Result;
            List<Fingerprint> fingerprints = Fingerprint.AssociateFingerprintsToTrack(signatures, track.Id);
            dalManager.InsertFingerprint(fingerprints);
            List<Fingerprint> insertedFingerprints = dalManager.ReadFingerprintsByTrackId(track.Id, 0);
            /*Read all fingerprints*/

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

                        Assert.AreEqual(
                            fingerprint.TotalFingerprintsPerTrack, insertedFingerprint.TotalFingerprintsPerTrack);
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

            List<bool[]> signatures =
                workUnitBuilder.BuildWorkUnit().On(PathToMp3).With(fingerprintingConfiguration).
                    GetFingerprintsUsingService(fingerprintingServiceWithBass).Result;

            List<Fingerprint> fingerprints = Fingerprint.AssociateFingerprintsToTrack(signatures, track.Id);
            dalManager.InsertFingerprint(fingerprints);


            List<Fingerprint> insertedFingerprints = dalManager.ReadFingerprintsByTrackId(track.Id, 0);
            Assert.AreEqual(fingerprints.Count, insertedFingerprints.Count);

            foreach (Fingerprint fingerprint in fingerprints)
            {
                int fingerprintId = fingerprint.Id;
                foreach (Fingerprint insertedFingerprint in
                    insertedFingerprints.Where(fingerprintSignature => fingerprintSignature.Id == fingerprintId))
                {
                    Assert.AreEqual(fingerprint.Signature.Length, insertedFingerprint.Signature.Length);
                    for (int i = 0; i < fingerprint.Signature.Length; i++)
                    {
                        Assert.AreEqual(fingerprint.Signature[i], insertedFingerprint.Signature[i]);
                    }

                    Assert.AreEqual(
                        fingerprint.TotalFingerprintsPerTrack, insertedFingerprint.TotalFingerprintsPerTrack);
                    Assert.AreEqual(fingerprint.TrackId, insertedFingerprint.TrackId);
                }
            }
        }

        [TestMethod]
        public void
            CreateFingerprintsFromFileAndInsertInDatabaseUsingDirectSoundProxyCheckCorrectitudeOfFingerprintsTest()
        {
            Album album = new Album(0, "Random");
            dalManager.InsertAlbum(album);
            Track track = new Track(0, "Random", "Random", album.Id);
            dalManager.InsertTrack(track);

            List<bool[]> signatures =
                workUnitBuilder.BuildWorkUnit().On(PathToWav).With(fingerprintingConfiguration).
                    GetFingerprintsUsingService(fingerprintingServiceWithDirectSound).Result;

            List<Fingerprint> fingerprints = Fingerprint.AssociateFingerprintsToTrack(signatures, track.Id);
            dalManager.InsertFingerprint(fingerprints);

            List<Fingerprint> insertedFingerprints = dalManager.ReadFingerprintsByTrackId(track.Id, 0);
            Assert.AreEqual(fingerprints.Count, insertedFingerprints.Count);

            foreach (Fingerprint fingerprint in fingerprints)
            {
                int fingerprintId = fingerprint.Id;
                foreach (Fingerprint insertedFingerprint in
                    insertedFingerprints.Where(fingerprintSignature => fingerprintSignature.Id == fingerprintId))
                {
                    Assert.AreEqual(fingerprint.Signature.Length, insertedFingerprint.Signature.Length);
                    for (int i = 0; i < fingerprint.Signature.Length; i++)
                    {
                        Assert.AreEqual(fingerprint.Signature[i], insertedFingerprint.Signature[i]);
                    }

                    Assert.AreEqual(
                        fingerprint.TotalFingerprintsPerTrack, insertedFingerprint.TotalFingerprintsPerTrack);
                    Assert.AreEqual(fingerprint.TrackId, insertedFingerprint.TrackId);
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

            var signatures =
                workUnitBuilder.BuildWorkUnit().On(PathToMp3).With(fingerprintingConfiguration).
                    GetFingerprintsUsingService(fingerprintingServiceWithBass).Result;

            List<Fingerprint> fingerprints = Fingerprint.AssociateFingerprintsToTrack(signatures, track.Id);
            dalManager.InsertFingerprint(fingerprints);

            List<Fingerprint> insertedFingerprints = Fingerprint.AssociateFingerprintsToTrack(signatures, track.Id);
            Assert.AreEqual(fingerprints.Count, insertedFingerprints.Count);

            foreach (Fingerprint fingerprint in fingerprints)
            {
                foreach (var insertedFingerprint in insertedFingerprints)
                {
                    if (fingerprint.Id == insertedFingerprint.Id)
                    {
                        Assert.AreEqual(fingerprint.Signature.Length, insertedFingerprint.Signature.Length);
                        for (int i = 0; i < fingerprint.Signature.Length; i++)
                        {
                            Assert.AreEqual(fingerprint.Signature[i], insertedFingerprint.Signature[i]);
                        }

                        Assert.AreEqual(
                            fingerprint.TotalFingerprintsPerTrack, insertedFingerprint.TotalFingerprintsPerTrack);
                        Assert.AreEqual(fingerprint.TrackId, insertedFingerprint.TrackId);
                    }
                }
            }
        }


        [TestMethod]
        public void CompareFingerprintsCreatedByDifferentProxiesTest()
        {
            var workUnitForDirectSound = workUnitBuilder.BuildWorkUnit().On(PathToWav).With(fingerprintingConfiguration);
            List<bool[]> directSoundFingerprints =
                workUnitForDirectSound.GetFingerprintsUsingService(fingerprintingServiceWithDirectSound).Result;
            var workUnitForBass = workUnitBuilder.BuildWorkUnit().On(PathToMp3).With(fingerprintingConfiguration);
            List<bool[]> bassFingerprints =
                workUnitForBass.GetFingerprintsUsingService(fingerprintingServiceWithBass).Result;
            int unmatchedItems = 0;
            int totalmatches = 0;

            // Check how many bytes are different while comparing BASS Fingers and DS Fingers (normaly ~1%)
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

            Assert.AreEqual(true, (float)unmatchedItems / totalmatches < 0.02); /*less than 1.5% difference*/
            Assert.AreEqual(bassFingerprints.Count, directSoundFingerprints.Count);
        }

        [TestMethod]
        public void CreateSeveralFingerprintsTest()
        {
            var workUnitForDirectSound = workUnitBuilder.BuildWorkUnit().On(PathToWav).With(fingerprintingConfiguration);
            List<bool[]> directSoundFingerprints =
                workUnitForDirectSound.GetFingerprintsUsingService(fingerprintingServiceWithDirectSound).Result;
            var workUnitForBass = workUnitBuilder.BuildWorkUnit().On(PathToMp3).With(fingerprintingConfiguration);
            List<bool[]> bassFingerprints =
                workUnitForBass.GetFingerprintsUsingService(fingerprintingServiceWithBass).Result;

            Assert.AreEqual(directSoundFingerprints.Count, bassFingerprints.Count);
            int unmatched = 0;
            for (int i = 0, n = directSoundFingerprints.Count; i < n; i++)
            {
                for (int j = 0; j < directSoundFingerprints[i].Length; j++)
                {
                    if (directSoundFingerprints[i][j] != bassFingerprints[i][j])
                    {
                        unmatched++;
                    }
                }
            }

            int totalElements = directSoundFingerprints.Count * directSoundFingerprints[0].Length;
            Assert.AreEqual(true, (float)unmatched / totalElements < 0.02);
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

        [TestMethod]
        public void CheckFingerprintCreationAlgorithmTest()
        {
            using (BassAudioService bassAudioService = new BassAudioService())
            {
                string tempFile = Path.GetTempPath() + 0 + ".wav";
                bassAudioService.RecodeTheFile(PathToMp3, tempFile, 5512);

                long fileSize = new FileInfo(tempFile).Length;
                List<bool[]> list =
                    workUnitBuilder.BuildWorkUnit().On(PathToMp3).With(fingerprintingConfiguration).
                        GetFingerprintsUsingService(fingerprintingServiceWithBass).Result;

                // One fingerprint corresponds to a granularity of 8192 samples which is 16384 bytes
                long expected = fileSize / (fingerprintingConfiguration.SamplesPerFingerprint * 4);
                Assert.AreEqual(expected, list.Count);
                File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void CheckFingerprintCreationAlgorithmTest1()
        {
            long fileSize = new FileInfo(PathToWav).Length;
#pragma warning disable 612,618
            using (IAudioService proxy = new DirectSoundAudioService())
            {
#pragma warning restore 612,618
                var workUnitForDirectSound =
                    workUnitBuilder.BuildWorkUnit().On(PathToWav).With(fingerprintingConfiguration);
                List<bool[]> list =
                    workUnitForDirectSound.GetFingerprintsUsingService(fingerprintingServiceWithDirectSound).Result;

                // One fingerprint corresponds to a granularity of 8192 samples which is 16384 bytes
                long expected = fileSize / (8192 * 4);
                Assert.AreEqual(expected, list.Count);
                proxy.Dispose();
            }
        }

        [TestMethod]
        public void CheckFingerprintCreationAlgorithmTest2()
        {
            var workUnitForBass = workUnitBuilder.BuildWorkUnit().On(PathToMp3).With(fingerprintingConfiguration);
            List<bool[]> bassFingerprints =
                workUnitForBass.GetFingerprintsUsingService(fingerprintingServiceWithBass).Result;
            var workUnitForDirectSound = workUnitBuilder.BuildWorkUnit().On(PathToWav).With(fingerprintingConfiguration);
            List<bool[]> directSoundFingerprints =
                workUnitForDirectSound.GetFingerprintsUsingService(fingerprintingServiceWithDirectSound).Result;

            Assert.AreEqual(bassFingerprints.Count, directSoundFingerprints.Count);
        }
    }
}