namespace SoundFingerprinting.Tests.Integration.Dao.MongoDb
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Dao;

    [TestClass]
    public class MongoDbModelServiceTest : ModelServiceTest
    {
        public MongoDbModelServiceTest(IModelService modelService)
            : base(modelService)
        {
        }
    }
}
