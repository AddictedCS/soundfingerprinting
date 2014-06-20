namespace SoundFingerprinting.MongoDb.Tests.Integration
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.MongoDb.Entity;
    using SoundFingerprinting.Tests.Integration.DAO;

    [TestClass]
    public class SpectralImageDaoTest : AbstractSpectralImageDaoTest
    {
        private readonly DaoTestHelper daoTestHelper = new DaoTestHelper();
        
        public SpectralImageDaoTest()
        {
            SpectralImageDao = new SpectralImageDao();
            TrackDao = new TrackDao();
        }

        public override sealed ISpectralImageDao SpectralImageDao { get; set; }

        public override sealed ITrackDao TrackDao { get; set; }

        [TestInitialize]
        public void SetUp()
        {
            daoTestHelper.GetCollection<SubFingerprint>(SubFingerprintDao.SubFingerprints).RemoveAll();
            daoTestHelper.GetCollection<Track>(MongoDb.TrackDao.Tracks).RemoveAll();
            daoTestHelper.GetCollection<Hash>(HashBinDao.HashBins).RemoveAll();
        }
    }
}
