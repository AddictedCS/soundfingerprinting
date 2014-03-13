namespace SoundFingerprinting.Tests.Integration.SQL
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.SQL;

    [TestClass]
    public class SqlModelServiceTest : ModelServiceTest
    {
        public SqlModelServiceTest()
        {
            ModelService = new SqlModelService();
        }

        public override sealed IModelService ModelService { get; set; }
    }
}
