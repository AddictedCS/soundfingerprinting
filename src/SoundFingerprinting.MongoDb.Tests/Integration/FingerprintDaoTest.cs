namespace SoundFingerprinting.MongoDb.Tests.Integration
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.MongoDb.Entity;
    using SoundFingerprinting.Tests.Integration.Dao;

    [TestClass]
    public class FingerprintDaoTest : AbstractFingerprintDaoTest
    {
        private readonly DaoTestHelper daoTestHelper;

        public FingerprintDaoTest()
        {
            FingerprintDao = new FingerprintDao();
            TrackDao = new TrackDao();
            daoTestHelper = new DaoTestHelper();
        }

        public override sealed IFingerprintDao FingerprintDao { get; set; }

        public override sealed ITrackDao TrackDao { get; set; }

        [TestInitialize]
        public void SetUp()
        {
            daoTestHelper.GetCollection<Fingerprint>(MongoDb.FingerprintDao.Fingerprints).RemoveAll();
            daoTestHelper.GetCollection<Track>(MongoDb.TrackDao.Tracks).RemoveAll();
        }
    }
}
