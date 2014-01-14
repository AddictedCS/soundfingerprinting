namespace SoundFingerprinting.Tests.Integration.Dao.RAM
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Dao;
    using SoundFingerprinting.Dao.RAM;

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
