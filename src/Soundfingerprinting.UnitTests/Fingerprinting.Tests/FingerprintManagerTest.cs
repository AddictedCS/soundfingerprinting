namespace Soundfingerprinting.UnitTests.Fingerprinting.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Soundfingerprinting.AudioProxies;
    using Soundfingerprinting.AudioProxies.Strides;
    using Soundfingerprinting.DbStorage;
    using Soundfingerprinting.DbStorage.Entities;
    using Soundfingerprinting.DbStorage.Utils;
    using Soundfingerprinting.Fingerprinting;


    [TestClass]
    public class FingerprintManagerTest : BaseTest
    {
        private string connectionstring;

        private DaoGateway dalManager;
       
        private FingerprintManager fingerManager;

        private IFingerprintConfig fingerprintConfig;

        [TestInitialize]
        public void SetUp()
        {
            connectionstring = ConnectionString;
            dalManager = new DaoGateway(connectionstring);
            fingerManager = new FingerprintManager();
            fingerprintConfig = new DefaultFingerpringConfig();
        }

        [TestCleanup]
        public void TearDown()
        {
        }

        [TestMethod]
        public void CreateFingerprintsFromFileAndInsertInDatabaseUsingDirectSoundProxyTest()
        {

            List<Track> tracks = dalManager.ReadTracks();
            if (tracks != null)
            {
                dalManager.DeleteTrack(tracks); /*Delete the information from the database*/
            }

            Album album = new Album(0, "Random");

            dalManager.InsertAlbum(album);
            Track track = new Track(0, "Track", "Track", album.Id);

            dalManager.InsertTrack(track);
            List<Fingerprint> fingerprints = null;
#pragma warning disable 612,618
            using (DirectSoundProxy proxy = new DirectSoundProxy())
#pragma warning restore 612,618
            {
                List<bool[]> signatures = fingerManager.CreateFingerprints(proxy, PATH_TO_WAV, StaticStride);
                fingerprints = Fingerprint.AssociateFingerprintsToTrack(signatures, track.Id);
                dalManager.InsertFingerprint(fingerprints);
            }

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

                        Assert.AreEqual(fingerprint.TotalFingerprintsPerTrack, insertedFingerprint.TotalFingerprintsPerTrack);
                        Assert.AreEqual(fingerprint.TrackId, insertedFingerprint.TrackId);
                    }
                }
            }
        }

        [TestMethod]
        public void CreateFingerprintsFromFileAndInsertInDatabaseUsingBassProxyTest()
        {
            List<Track> tracks = dalManager.ReadTracks();
            if (tracks != null)
            {
                dalManager.DeleteTrack(tracks); /*Delete the information from the database*/
            }

            Album album = new Album(0, "Track");

            dalManager.InsertAlbum(album);
            Track track = new Track(0, "Random", "Random", album.Id);

            dalManager.InsertTrack(track);
            List<Fingerprint> fingerprints = null;
            using (BassProxy proxy = new BassProxy())
            {
                List<bool[]> signatures = fingerManager.CreateFingerprints(proxy, PATH_TO_MP3, StaticStride);
                fingerprints = Fingerprint.AssociateFingerprintsToTrack(signatures, track.Id);
                dalManager.InsertFingerprint(fingerprints);
            }

            List<Fingerprint> insertedFingerprints = dalManager.ReadFingerprintsByTrackId(track.Id, 0);
                /*Read all fingerprints*/
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

        /// <summary>
        ///   Check whether CreateFingerprintsFromFileAndInsertInDatabase and CreateFingerprintsFromFile
        ///   generate the same fingerprints [Direct Sound Proxy]
        /// </summary>
        [TestMethod]
        public void CreateFingerprintsFromFileAndInsertInDatabaseUsingDirectSoundProxyCheckCorrectitudeOfFingerprintsTest()
        {
            List<Track> tracks = dalManager.ReadTracks();
            if (tracks != null)
            {
                dalManager.DeleteTrack(tracks); /*Delete the information from the database*/
            }

            Album album = new Album(0, "Random");

            dalManager.InsertAlbum(album);
            Track track = new Track(0, "Random",
                "Random",
                album.Id);

            dalManager.InsertTrack(track);
            List<Fingerprint> fingerprints = null;
            List<Fingerprint> insertedFingerprints = null;
#pragma warning disable 612,618
            using (DirectSoundProxy proxy = new DirectSoundProxy())
#pragma warning restore 612,618
            {
                List<bool[]> signatures = fingerManager.CreateFingerprints(proxy, PATH_TO_WAV, StaticStride);
                fingerprints = Fingerprint.AssociateFingerprintsToTrack(signatures, track.Id);
                dalManager.InsertFingerprint(fingerprints);
                signatures = fingerManager.CreateFingerprints(proxy, PATH_TO_WAV, StaticStride);
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

        /// <summary>
        ///   Check whether CreateFingerprintsFromFileAndInsertInDatabase and CreateFingerprintsFromFile
        ///   generate the same fingerprints [Bass Proxy]
        /// </summary>
        [TestMethod]
        public void CreateFingerprintsFromFileAndInsertInDatabaseUsingBassProxyCheckCorrectitudeOfFingerprintsTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            List<Track> tracks = dalManager.ReadTracks();
            if (tracks != null)
                dalManager.DeleteTrack(tracks); /*Delete the information from the database*/

            Album album = new Album(0, name);

            dalManager.InsertAlbum(album);
            Track track = new Track(0, name,
                name,
                album.Id);

            dalManager.InsertTrack(track);
            List<Fingerprint> fingerprints = null;
            List<Fingerprint> insertedFingerprints = null;
            using (BassProxy proxy = new BassProxy())
            {
                List<bool[]> signatures = fingerManager.CreateFingerprints(proxy, PATH_TO_MP3, StaticStride);
                fingerprints = Fingerprint.AssociateFingerprintsToTrack(signatures, track.Id);
                dalManager.InsertFingerprint(fingerprints);
                signatures = fingerManager.CreateFingerprints(proxy, PATH_TO_MP3, StaticStride);
                insertedFingerprints = Fingerprint.AssociateFingerprintsToTrack(signatures, track.Id);
            }

            Assert.AreEqual(fingerprints.Count, insertedFingerprints.Count);

            foreach (Fingerprint f in fingerprints)
                foreach (Fingerprint iFingerprint in insertedFingerprints)
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
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Compare fingerprints created by different proxies
        /// </summary>
        [TestMethod]
        public void CompareFingerprintsCreatedByDifferentProxiesTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

#pragma warning disable 612,618
            DirectSoundProxy dsProxy = new DirectSoundProxy();
#pragma warning restore 612,618
            BassProxy bsProxy = new BassProxy();
            List<bool[]> dsFingers = fingerManager.CreateFingerprints(dsProxy, PATH_TO_WAV, StaticStride);
            List<bool[]> bFingers = fingerManager.CreateFingerprints(bsProxy, PATH_TO_MP3, StaticStride);
            int unmatchedItems = 0;
            int totalmatches = 0;
            //Check how many bytes are different while comparing BASS Fingers and DS Fingers (normaly ~1%)
            for (int i = 0, n = dsFingers.Count > bFingers.Count ? bFingers.Count : dsFingers.Count; i < n; i++)
                for (int j = 0; j < dsFingers[i].Length; j++)
                {
                    if (dsFingers[i][j] != bFingers[i][j])
                        unmatchedItems++;
                    totalmatches++;
                }
            Assert.AreEqual(true, (float) unmatchedItems/totalmatches < 0.02); /*less than 1.5% difference*/
            Assert.AreEqual(bFingers.Count, dsFingers.Count);
            dsProxy.Dispose();
            bsProxy.Dispose();
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }


        /// <summary>
        ///   Create just several fingerprints
        /// </summary>
        [TestMethod]
        public void CreateSeveralFingerprintsTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

#pragma warning disable 612,618
            using (DirectSoundProxy dsProxy = new DirectSoundProxy())
            {
#pragma warning restore 612,618
                using (BassProxy bsProxy = new BassProxy())
                {
                    const int count = 0;
                    List<bool[]> dsFingers = fingerManager.CreateFingerprints(dsProxy, PATH_TO_WAV, StaticStride);
                    List<bool[]> bFingers = fingerManager.CreateFingerprints(bsProxy, PATH_TO_MP3, StaticStride);
                    Assert.AreEqual(dsFingers.Count, bFingers.Count);
                    int unmatched = 0;
                    for (int i = 0, n = dsFingers.Count; i < n; i++)
                        for (int j = 0; j < dsFingers[i].Length; j++)
                            if (dsFingers[i][j] != bFingers[i][j]) unmatched++;
                    int totalElements = dsFingers.Count*dsFingers[0].Length;
                    Assert.AreEqual(true, (float) unmatched/totalElements < 0.02);
                }
            }
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }


        /// <summary>
        ///   Get float array from byte
        /// </summary>
        [TestMethod]
        public void GetDoubleArrayFromByteTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            byte[] byteArray = TestUtilities.GenerateRandomInputByteArray(128*64);
            bool silence = false;
            float[] array = ArrayUtils.GetDoubleArrayFromSamples(byteArray, 128*64, ref silence);
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

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Check whether the # of fingerprints returned from the creation process is Ok
        /// </summary>
        [TestMethod]
        public void CheckFingerprintCreationAlgorithmTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            using (BassProxy bassProxy = new BassProxy())
            {
                string tempFile = Path.GetTempPath() + 0 + ".wav";
                bassProxy.RecodeTheFile(PATH_TO_MP3, tempFile, 5512);

                long fileSize = new FileInfo(tempFile).Length;
                BassProxy proxy = new BassProxy();
                List<bool[]> list = fingerManager.CreateFingerprints(proxy, PATH_TO_MP3, new StaticStride(0));
                //One fingerprint corresponds to a granularity of 8192 samples which is 16384 bytes
                long expected = fileSize/(fingerprintConfig.SamplesPerFingerprint*4);
                Assert.AreEqual(expected, list.Count);
                proxy.Dispose();
                File.Delete(tempFile);
            }

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Check whether the # of fingerprints returned from the creation process is Ok
        /// </summary>
        [TestMethod]
        public void CheckFingerprintCreationAlgorithmTest1()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            long fileSize = new FileInfo(PATH_TO_WAV).Length;
            int startIndex = 0;
#pragma warning disable 612,618
            using (IAudio proxy = new DirectSoundProxy())
            {
#pragma warning restore 612,618
                List<bool[]> list = fingerManager.CreateFingerprints(proxy, PATH_TO_WAV, new StaticStride(0));
                //One fingerprint corresponds to a granularity of 8192 samples which is 16384 bytes
                long expected = fileSize/(8192*4);
                Assert.AreEqual(expected, list.Count);
                proxy.Dispose();
            }
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Check whether the # of fingerprints returned from the creation process is Ok
        /// </summary>
        [TestMethod]
        public void CheckFingerprintCreationAlgorithmTest2()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            long fileSize = new FileInfo(PATH_TO_WAV).Length;
            int startIndex = 0;
            using (IAudio proxy = new BassProxy())
            {
                List<bool[]> listBs = fingerManager.CreateFingerprints(proxy, PATH_TO_MP3, new StaticStride(0));
                //One fingerprint corresponds to a granularity of 8192 samples which is 16384 bytes
#pragma warning disable 612,618
                using (IAudio dsProxy = new DirectSoundProxy())
                {
#pragma warning restore 612,618
                    List<bool[]> listDs = fingerManager.CreateFingerprints(dsProxy, PATH_TO_WAV, new StaticStride(0));
                    Assert.AreEqual(listBs.Count, listDs.Count);
                    proxy.Dispose();
                    dsProxy.Dispose();
                }
            }
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }
    }
}