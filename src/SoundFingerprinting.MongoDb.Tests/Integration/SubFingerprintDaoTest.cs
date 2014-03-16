namespace SoundFingerprinting.MongoDb.Tests.Integration
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.MongoDb.Entity;
    using SoundFingerprinting.Tests.Integration.Dao;

    [TestClass]
    public class SubFingerprintDaoTest : AbstractSubFingerprintDaoTest
    {
        private readonly DaoTestHelper daoTestHelper = new DaoTestHelper();

        public SubFingerprintDaoTest()
        {
            SubFingerprintDao = new SubFingerprintDao();
            TrackDao = new TrackDao();
        }

        public override sealed ISubFingerprintDao SubFingerprintDao { get; set; }

        public override sealed ITrackDao TrackDao { get; set; }

        [TestInitialize]
        public void SetUp()
        {
            daoTestHelper.GetCollection<SubFingerprint>(MongoDb.SubFingerprintDao.SubFingerprints);
            daoTestHelper.GetCollection<Track>(MongoDb.TrackDao.Tracks);
        }
    }
}
