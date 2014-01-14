namespace SoundFingerprinting.Tests.Integration.Dao.SQL
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Dao;
    using SoundFingerprinting.Dao.SQL;

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
