namespace SoundFingerprinting.Tests.Integration.Dao.SQL
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.SQL;

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
