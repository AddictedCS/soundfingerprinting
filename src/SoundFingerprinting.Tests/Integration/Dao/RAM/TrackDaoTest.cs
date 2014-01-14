namespace SoundFingerprinting.Tests.Integration.Dao.RAM
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Dao;
    using SoundFingerprinting.Dao.RAM;

    [TestClass]
    public class TrackDaoTest : AbstractTrackDaoTest
    {
        public TrackDaoTest()
        {
            TrackDao = new TrackDao();
            SubFingerprintDao = new SubFingerprintDao();
            HashBinDao = new HashBinDao();
        }
        
        public override sealed ITrackDao TrackDao { get; set; }

        public override sealed ISubFingerprintDao SubFingerprintDao { get; set; }

        public override sealed IHashBinDao HashBinDao { get; set; }
    }
}
