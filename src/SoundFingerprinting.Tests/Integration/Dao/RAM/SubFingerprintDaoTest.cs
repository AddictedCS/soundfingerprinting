namespace SoundFingerprinting.Tests.Integration.Dao.RAM
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.InMemory;

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
