namespace SoundFingerprinting.Tests.Integration.SQL
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.SQL;
    using SoundFingerprinting.Tests.Integration.Dao;

    [TestClass]
    public class FingerprintDaoTest : AbstractFingerprintDaoTest
    {
        public FingerprintDaoTest()
        {
            FingerprintDao = new FingerprintDao();
            TrackDao = new TrackDao();
        }

        public override sealed IFingerprintDao FingerprintDao { get; set; }

        public override sealed ITrackDao TrackDao { get; set; }
    }
}
