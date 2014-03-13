namespace SoundFingerprinting.Tests.Integration.InMemory
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.InMemory;

    [TestClass]
    public class InMemoryModelServiceTest : ModelServiceTest
    {
        private const int NumberOfHashTables = 25;

        public override IModelService ModelService { get; set; }

        [TestInitialize]
        public override void SetUp()
        {
            base.SetUp();
            var ramStorage = new RAMStorage(NumberOfHashTables);
            ModelService = new InMemoryModelService(
                new TrackDao(ramStorage), new HashBinDao(ramStorage), new SubFingerprintDao(ramStorage), new FingerprintDao(ramStorage));
        }
    }
}
