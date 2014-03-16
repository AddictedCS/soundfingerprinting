namespace SoundFingerprinting.Tests.Integration.InMemory
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.InMemory;
    using SoundFingerprinting.Tests.Integration.Dao;

    [TestClass]
    public class FingerprintDaoTest : AbstractFingerprintDaoTest
    {
        private const int NumberOfHashTables = 25;

        public override sealed IFingerprintDao FingerprintDao { get; set; }

        public override sealed ITrackDao TrackDao { get; set; }

        [TestInitialize]
        public void SetUp()
        {
            var ramStorage = new RAMStorage(NumberOfHashTables);
            FingerprintDao = new FingerprintDao(ramStorage);
            TrackDao = new TrackDao(ramStorage);
        }
    }
}
