namespace SoundFingerprinting.Dao.MongoDb
{
    public class MongoDbConnectionStringFactory : IMongoDbConnectionStringFactory
    {
        public string GetConnectionString()
        {
            return "mongodb://localhost";
        }

        public string GetDatabaseName()
        {
            return "FingerprintsDb";
        }
    }
}