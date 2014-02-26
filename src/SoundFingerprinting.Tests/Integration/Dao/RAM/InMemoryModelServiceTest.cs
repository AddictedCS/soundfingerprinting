namespace SoundFingerprinting.Tests.Integration.Dao.RAM
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.InMemory;

    [TestClass]
    public class InMemoryModelServiceTest : ModelServiceTest
    {
        public InMemoryModelServiceTest()
            : base(new InMemoryModelService())
        {
        }

        protected InMemoryModelServiceTest(IModelService modelService)
            : base(modelService)
        {
        }
    }
}
