namespace SoundFingerprinting.Tests.Integration.Dao.SQL
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Dao;
    using SoundFingerprinting.Dao.SQL;

    [TestClass]
    public class SqlModelServiceTest : ModelServiceTest
    {
        public SqlModelServiceTest()
            : base(new SqlModelService())
        {
        }

        protected SqlModelServiceTest(IModelService modelService)
            : base(modelService)
        {
        }
    }
}
