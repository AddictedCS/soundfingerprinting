namespace SoundFingerprinting.Tests.Integration.Dao
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    [TestClass]
    public abstract class AbstractFingerprintDaoTest : AbstractIntegrationTest
    {
        public abstract IFingerprintDao FingerprintDao { get; set; }

        public abstract ITrackDao TrackDao { get; set; }

        [TestMethod]
        public void InsertTest()
        {
            TrackData track = new TrackData("isrc", "artist", "title", "album", 1986, 200);
            var trackReference = TrackDao.InsertTrack(track);

            var fingerprintReference = FingerprintDao.InsertFingerprint(new FingerprintData(GenericFingerprint, trackReference));

            AssertModelReferenceIsInitialized(fingerprintReference);
        }

        [TestMethod]
        public void MultipleFingerprintsInsertTest()
        {
            const int NumberOfFingerprints = 100;
            for (int i = 0; i < NumberOfFingerprints; i++)
            {
                var trackData = new TrackData("isrc" + i, "artist", "title", "album", 2012, 200);
                var trackReference = TrackDao.InsertTrack(trackData);
                var fingerprintReference = FingerprintDao.InsertFingerprint(new FingerprintData(GenericFingerprint, trackReference));

                AssertModelReferenceIsInitialized(fingerprintReference);
            }
        }

        [TestMethod]
        public void ReadTest()
        {
            const int NumberOfFingerprints = 100;
            TrackData track = new TrackData("isrc", "artist", "title", "album", 1986, 200);
            var trackReference = TrackDao.InsertTrack(track);

            for (int i = 0; i < NumberOfFingerprints; i++)
            {
                FingerprintDao.InsertFingerprint(new FingerprintData(GenericFingerprint, trackReference));
            }

            var fingerprints = FingerprintDao.ReadFingerprintsByTrackReference(trackReference);

            Assert.IsTrue(fingerprints.Count == NumberOfFingerprints);

            foreach (var fingerprint in fingerprints)
            {
                Assert.IsTrue(GenericFingerprint.Length == fingerprint.Signature.Length);
                for (var i = 0; i < GenericFingerprint.Length; i++)
                {
                    Assert.AreEqual(GenericFingerprint[i], fingerprint.Signature[i]);
                }
            }
        }
    }
}
