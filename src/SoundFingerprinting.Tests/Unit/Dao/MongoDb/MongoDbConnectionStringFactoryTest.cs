namespace SoundFingerprinting.Tests.Unit.Dao.MongoDb
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Dao.MongoDb.Connection;

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
            Assert.AreEqual("mongo://localhost", connectionString);
        }

        [TestMethod]
        public void ReadMongoDbDatabaseNameStringValue()
        {
            var databaseName = connectionStringFactory.GetDatabaseName();

            Assert.AreEqual("FingerprintsDb", databaseName);
        }
    }
}
