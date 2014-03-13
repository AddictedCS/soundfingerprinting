namespace SoundFingerprinting.Tests.Integration.InMemory
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.InMemory;
    using SoundFingerprinting.Tests.Integration.Dao;

    [TestClass]
    public class SubFingerprintDaoTest : AbstractSubFingerprintDaoTest
    {
        private const int NumberOfFingerprints = 25;

        public override sealed ISubFingerprintDao SubFingerprintDao { get; set; }

        public override sealed ITrackDao TrackDao { get; set; }

        [TestInitialize]
        public override void SetUp()
        {
            base.SetUp();
            var ramStorage = new RAMStorage(NumberOfFingerprints);
            SubFingerprintDao = new SubFingerprintDao(ramStorage);
            TrackDao = new TrackDao(ramStorage);
        }
    }
}
