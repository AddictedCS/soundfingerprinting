namespace SoundFingerprinting.SQL.Tests.Integration
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.SQL;
    using SoundFingerprinting.Tests.Integration;

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
