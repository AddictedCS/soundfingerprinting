namespace SoundFingerprinting.MongoDb.Tests.Integration
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.Tests.Integration.Dao;

    [TestClass]
    public class TrackDaoTest : AbstractTrackDaoTest
    {
        public override ITrackDao TrackDao { get; set; }

        public override ISubFingerprintDao SubFingerprintDao { get; set; }

        public override IHashBinDao HashBinDao { get; set; }
    }
}
