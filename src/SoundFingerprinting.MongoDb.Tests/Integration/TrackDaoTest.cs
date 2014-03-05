namespace SoundFingerprinting.MongoDb.Tests.Integration
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.MongoDb.Entity;
    using SoundFingerprinting.Tests.Integration.Dao;

    [TestClass]
    public class TrackDaoTest : AbstractTrackDaoTest
    {
        private readonly DaoTestHelper daoTestHelper = new DaoTestHelper();

        public TrackDaoTest()
        {
            TrackDao = new TrackDao();
            SubFingerprintDao = new SubFingerprintDao();
            HashBinDao = new HashBinDao();
        }

        public override sealed ITrackDao TrackDao { get; set; }

        public override sealed ISubFingerprintDao SubFingerprintDao { get; set; }

        public override sealed IHashBinDao HashBinDao { get; set; }

        [TestInitialize]
        public override void SetUp()
        {
            base.SetUp();
            daoTestHelper.GetCollection<SubFingerprint>(MongoDb.SubFingerprintDao.SubFingerprints).RemoveAll();
            daoTestHelper.GetCollection<Track>(MongoDb.TrackDao.Tracks).RemoveAll();
            daoTestHelper.GetCollection<Hash>(MongoDb.HashBinDao.HashBins).RemoveAll();
        }
    }
}
