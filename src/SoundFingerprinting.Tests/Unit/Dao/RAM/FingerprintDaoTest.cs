namespace SoundFingerprinting.Tests.Unit.Dao
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Dao.RAM;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;

    [TestClass]
    public class FingerprintDaoTest : AbstractTest
    {
        private FingerprintDao fingerprintDao;

        private TrackDao trackDao;

        [TestInitialize]
        public void SetUp()
        {
            fingerprintDao = new FingerprintDao(DependencyResolver.Current.Get<IRAMStorage>());
            trackDao = new TrackDao(DependencyResolver.Current.Get<IRAMStorage>());
        }

        [TestCleanup]
        public void TearDown()
        {
            DependencyResolver.Current.Get<IRAMStorage>().Reset(new DefaultFingerprintConfiguration().NumberOfLSHTables);
        }

        [TestMethod]
        public void DependencyResolverTest()
        {
            var instance = new FingerprintDao();

            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void InsertTest()
        {
            var trackData = new TrackData("isrc", "artist", "title", "album", 2012, 200);
            int trackId = trackDao.Insert(trackData);

            for (int i = 0; i < 100; i++)
            {
                int id = fingerprintDao.Insert(GenericFingerprint, trackId);
                Assert.IsFalse(id == 0);
            }
        }

        [TestMethod]
        public void ReadTest()
        {
            var trackData = new TrackData("isrc", "artist", "title", "album", 2012, 200);
            int trackId = trackDao.Insert(trackData);

            for (int i = 0; i < 100; i++)
            {
                fingerprintDao.Insert(GenericFingerprint, trackId);
            }

            var fingerprints = fingerprintDao.ReadFingerprintsByTrackId(trackId);
            Assert.AreEqual(100, fingerprints.Count);
        }
    }
}
