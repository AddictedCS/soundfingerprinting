namespace SoundFingerprinting.Tests.Integration.Dao.SQL
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Dao;
    using SoundFingerprinting.Dao.SQL;

    [TestClass]
    public class SubFingerprintDaoTest : AbstractSubFingerprintDaoTest
    {
        public SubFingerprintDaoTest()
        {
            SubFingerprintDao = new SubFingerprintDao();
            TrackDao = new TrackDao();
        }

        public override sealed ISubFingerprintDao SubFingerprintDao { get; set; }

        public override sealed ITrackDao TrackDao { get; set; }
    }
}
