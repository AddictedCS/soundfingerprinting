namespace SoundFingerprinting.Tests.Integration.Dao.RAM
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Dao;
    using SoundFingerprinting.Dao.RAM;
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
