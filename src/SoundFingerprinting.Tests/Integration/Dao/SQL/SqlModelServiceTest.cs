namespace SoundFingerprinting.Tests.Integration.Dao.SQL
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.SQL;

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
