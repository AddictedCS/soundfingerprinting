namespace SoundFingerprinting.Tests.Unit.Dao.MongoDb
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SoundFingerprinting.Dao.MongoDb.Connection;

    [TestClass]
    public class MongoDatabaseProviderFactoryTest : AbstractTest
    {
        private IMongoDatabaseProviderFactory mongoDatabaseProviderFactory;

        private Mock<IMongoDbConnectionStringFactory> connectionStringFactory;

        [TestInitialize]
        public void SetUp()
        {
            connectionStringFactory = new Mock<IMongoDbConnectionStringFactory>(MockBehavior.Strict);

            mongoDatabaseProviderFactory = new MongoDatabaseProviderFactory(connectionStringFactory.Object);
        }

        [TestCleanup]
        public void TearDown()
        {
            connectionStringFactory.VerifyAll();
        }

        [TestMethod]
        public void MongoDatabaseIsRetrievedCorrectly()
        {
            connectionStringFactory.Setup(f => f.GetConnectionString()).Returns("mongodb://localhost");
            connectionStringFactory.Setup(f => f.GetDatabaseName()).Returns("FingerprintsDb");

            var database = mongoDatabaseProviderFactory.GetDatabase();

            Assert.IsNotNull(database);
            Assert.AreEqual("localhost:27017", database.Server.Instance.Address.ToString());
            Assert.AreEqual("FingerprintsDb", database.Name);
        }
    }
}
