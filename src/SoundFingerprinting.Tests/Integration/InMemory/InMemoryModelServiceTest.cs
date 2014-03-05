namespace SoundFingerprinting.Tests.Integration.InMemory
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

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
