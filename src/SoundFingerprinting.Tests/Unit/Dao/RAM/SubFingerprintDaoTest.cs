namespace SoundFingerprinting.Tests.Unit.Dao.RAM
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Dao.RAM;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;

    [TestClass]
    public class SubFingerprintDaoTest : AbstractTest
    {
        private SubFingerprintDao subFingerprintDao;

        private TrackDao trackDao;

        [TestInitialize]
        public void SetUp()
        {
            subFingerprintDao = new SubFingerprintDao(DependencyResolver.Current.Get<IRAMStorage>());
            trackDao = new TrackDao(DependencyResolver.Current.Get<IRAMStorage>());
        }

        [TestCleanup]
        public void TearDown()
        {
            DependencyResolver.Current.Get<IRAMStorage>().Reset(new DefaultFingerprintConfiguration().NumberOfLSHTables);
        }

        [TestMethod]
        public void DependencyResoverTest()
        {
            var instance = new SubFingerprintDao();

            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void InsertSubFingerprintSignatureTest()
        {
            TrackData track = new TrackData("isrc", "artist", "title", "album", 2012, 200);
            int trackId = trackDao.Insert(track);

            long id = subFingerprintDao.Insert(GenericSignature, trackId);

            Assert.IsFalse(id == 0);
        }

        [TestMethod]
        public void ReadBySubFingerprintIdTest()
        {
            TrackData track = new TrackData("isrc", "artist", "title", "album", 2012, 200);
            int trackId = trackDao.Insert(track);
            long id = subFingerprintDao.Insert(GenericSignature, trackId);

            SubFingerprintData subFingerprint = subFingerprintDao.ReadById(id);

            Assert.IsNotNull(subFingerprint);
            Assert.AreEqual(GenericSignature, subFingerprint.Signature);
            Assert.IsNotNull(subFingerprint.SubFingerprintReference);
            Assert.IsNotNull(subFingerprint.TrackReference);
        }
    }
}
