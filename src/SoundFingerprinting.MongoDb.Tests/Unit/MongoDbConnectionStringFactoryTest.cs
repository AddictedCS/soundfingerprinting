namespace SoundFingerprinting.MongoDb.Tests.Unit
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.MongoDb.Connection;
    using SoundFingerprinting.Tests;

    [TestClass]
    public class MongoDbConnectionStringFactoryTest : AbstractTest
    {
        private MongoDbConnectionStringFactory connectionStringFactory;

        [TestInitialize]
        public void SetUp()
        {
            connectionStringFactory = new MongoDbConnectionStringFactory();
        }

        [TestMethod]
        public void ReadMongoDbConnectionStringValue()
        {
            var connectionString = connectionStringFactory.GetConnectionString();
            Assert.AreEqual("mongodb://localhost", connectionString);
        }

        [TestMethod]
        public void ReadMongoDbDatabaseNameStringValue()
        {
            var databaseName = connectionStringFactory.GetDatabaseName();

            Assert.AreEqual("FingerprintsDbTest", databaseName);
        }
    }
}
