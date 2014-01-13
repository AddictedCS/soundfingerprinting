namespace SoundFingerprinting.Tests.Unit.Dao
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Dao.InMemory;
    using SoundFingerprinting.Data;

    [TestClass]
    public class SubFingerprintStorageDaoTest : AbstractTest
    {
        private SubFingerprintStorageDao subFingerprintStorageDao;

        private TrackStorageDao trackStorageDao;

        [TestInitialize]
        public void SetUp()
        {
            subFingerprintStorageDao = new SubFingerprintStorageDao();
            trackStorageDao = new TrackStorageDao();
        }

        [TestMethod]
        public void InsertSubFingerprintSignatureTest()
        {
            TrackData track = new TrackData("isrc", "artist", "title", "album", 2012, 200);
            int trackId = trackStorageDao.Insert(track);

            long id = subFingerprintStorageDao.Insert(GenericSignature, trackId);

            Assert.IsFalse(id == 0);
        }

        [TestMethod]
        public void ReadBySubFingerprintIdTest()
        {
            TrackData track = new TrackData("isrc", "artist", "title", "album", 2012, 200);
            int trackId = trackStorageDao.Insert(track);
            long id = subFingerprintStorageDao.Insert(GenericSignature, trackId);

            SubFingerprintData subFingerprint = subFingerprintStorageDao.ReadById(id);

            Assert.IsNotNull(subFingerprint);
            Assert.AreEqual(GenericSignature, subFingerprint.Signature);
            Assert.IsNotNull(subFingerprint.SubFingerprintReference);
            Assert.IsNotNull(subFingerprint.TrackReference);
        }
    }
}
