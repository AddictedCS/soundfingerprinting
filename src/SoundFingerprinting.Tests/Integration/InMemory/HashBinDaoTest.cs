namespace SoundFingerprinting.Tests.Integration.InMemory
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.InMemory;
    using SoundFingerprinting.Tests.Integration.Dao;

    [TestClass]
    public class HashBinDaoTest : AbstractHashBinDaoTest
    {
        public HashBinDaoTest()
        {
            HashBinDao = new HashBinDao();
            TrackDao = new TrackDao();
            SubFingerprintDao = new SubFingerprintDao();
        }

        public override sealed IHashBinDao HashBinDao { get; set; }

        public override sealed ITrackDao TrackDao { get; set; }

        public override sealed ISubFingerprintDao SubFingerprintDao { get; set; }
    }
}
