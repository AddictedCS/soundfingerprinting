namespace SoundFingerprinting.Tests.Integration
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.InMemory;

    [TestClass]
    public class FingerprintDaoTest : IntegrationWithSampleFilesTest
    {
        private IFingerprintDao fingerprintDao;
        private ITrackDao trackDao;

        [TestInitialize]
        public void SetUp()
        {
            var ramStorage = new RAMStorage(NumberOfHashTables);
            fingerprintDao = new FingerprintDao(ramStorage);
            trackDao = new TrackDao(ramStorage);
        }

        [TestMethod]
        public void InsertTest()
        {
            TrackData track = new TrackData("isrc", "artist", "title", "album", 1986, 200);
            var trackReference = trackDao.InsertTrack(track);

            var fingerprintReference = fingerprintDao.InsertFingerprint(new FingerprintData(GenericFingerprint, trackReference));

            AssertModelReferenceIsInitialized(fingerprintReference);
        }

        [TestMethod]
        public void MultipleFingerprintsInsertTest()
        {
            const int NumberOfFingerprints = 100;
            for (int i = 0; i < NumberOfFingerprints; i++)
            {
                var trackData = new TrackData("isrc" + i, "artist", "title", "album", 2012, 200);
                var trackReference = trackDao.InsertTrack(trackData);
                var fingerprintReference = fingerprintDao.InsertFingerprint(new FingerprintData(GenericFingerprint, trackReference));

                AssertModelReferenceIsInitialized(fingerprintReference);
            }
        }

        [TestMethod]
        public void ReadTest()
        {
            const int NumberOfFingerprints = 100;
            TrackData track = new TrackData("isrc", "artist", "title", "album", 1986, 200);
            var trackReference = trackDao.InsertTrack(track);

            for (int i = 0; i < NumberOfFingerprints; i++)
            {
                fingerprintDao.InsertFingerprint(new FingerprintData(GenericFingerprint, trackReference));
            }

            var fingerprints = fingerprintDao.ReadFingerprintsByTrackReference(trackReference);

            Assert.IsTrue(fingerprints.Count == NumberOfFingerprints);

            foreach (var fingerprint in fingerprints)
            {
                CollectionAssert.AreEqual(GenericFingerprint, fingerprint.Signature);
            }
        }
    }
}
