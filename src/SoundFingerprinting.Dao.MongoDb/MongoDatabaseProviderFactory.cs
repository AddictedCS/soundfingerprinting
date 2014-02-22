namespace SoundFingerprinting.Dao.MongoDb
{
    using MongoDB.Driver;

    public class MongoDatabaseProviderFactory : IMongoDatabaseProviderFactory
    {
        private readonly IMongoDbConnectionStringFactory connectionStringFactory;

        public MongoDatabaseProviderFactory(IMongoDbConnectionStringFactory connectionStringFactory)
        {
            this.connectionStringFactory = connectionStringFactory;
        }

        public MongoDatabase GetDatabase()
        {
            string connectionString = connectionStringFactory.GetConnectionString();
            return GetMongoDatabase(connectionString, connectionStringFactory.GetDatabaseName());
        }

        private MongoDatabase GetMongoDatabase(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            var server = client.GetServer();
            return server.GetDatabase(databaseName);
        }
    }
}