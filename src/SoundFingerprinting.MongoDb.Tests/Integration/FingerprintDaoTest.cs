namespace SoundFingerprinting.MongoDb.Tests.Integration
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.Tests.Integration.Dao;

    [TestClass]
    public class FingerprintDaoTest : AbstractFingerprintDaoTest
    {
        private readonly DaoHelper daoHelper;

        public FingerprintDaoTest()
        {
            FingerprintDao = new FingerprintDao();
            TrackDao = new TrackDao();
            daoHelper = new DaoHelper();
        }

        public override sealed IFingerprintDao FingerprintDao { get; set; }

        public override sealed ITrackDao TrackDao { get; set; }

        [TestInitialize]
        public override void SetUp()
        {
            base.SetUp();
            daoHelper.GetCollection(AbstractDao.FingerprintsCollection).RemoveAll();
            daoHelper.GetCollection(AbstractDao.TracksCollection).RemoveAll();
        }
    }
}
