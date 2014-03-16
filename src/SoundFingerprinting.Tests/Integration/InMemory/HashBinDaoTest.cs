namespace SoundFingerprinting.Tests.Integration.InMemory
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.InMemory;
    using SoundFingerprinting.Tests.Integration.Dao;

    [TestClass]
    public class HashBinDaoTest : AbstractHashBinDaoTest
    {
        private const int NumberOfHashTables = 25;

        public override sealed IHashBinDao HashBinDao { get; set; }

        public override sealed ITrackDao TrackDao { get; set; }

        public override sealed ISubFingerprintDao SubFingerprintDao { get; set; }

        [TestInitialize]
        public void SetUp()
        {
            var ramStorage = new RAMStorage(NumberOfHashTables);
            HashBinDao = new HashBinDao(ramStorage);
            TrackDao = new TrackDao(ramStorage);
            SubFingerprintDao = new SubFingerprintDao(ramStorage);
        }
    }
}
