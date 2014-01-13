namespace SoundFingerprinting.Tests.Unit.Dao
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Dao.InMemory;
    using SoundFingerprinting.Data;

    [TestClass]
    public class FingerprintStorageDaoTest : AbstractTest
    {
        private FingerprintStorageDao fingerprintStorageDao;

        private TrackStorageDao trackStorageDao;

        [TestInitialize]
        public void SetUp()
        {
            fingerprintStorageDao = new FingerprintStorageDao();
            trackStorageDao = new TrackStorageDao();
        }

        [TestMethod]
        public void InsertTest()
        {
            var trackData = new TrackData("isrc", "artist", "title", "album", 2012, 200);
            int trackId = trackStorageDao.Insert(trackData);

            for (int i = 0; i < 100; i++)
            {
                int id = fingerprintStorageDao.Insert(GenericFingerprint, trackId);
                Assert.IsFalse(id == 0);
            }
        }

        [TestMethod]
        public void ReadTest()
        {
            var trackData = new TrackData("isrc", "artist", "title", "album", 2012, 200);
            int trackId = trackStorageDao.Insert(trackData);

            for (int i = 0; i < 100; i++)
            {
                fingerprintStorageDao.Insert(GenericFingerprint, trackId);
            }

            var fingerprints = fingerprintStorageDao.ReadFingerprintsByTrackId(trackId);
            Assert.AreEqual(100, fingerprints.Count);
        }
    }
}
