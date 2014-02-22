namespace SoundFingerprinting.Dao.MongoDb.Connection
{
    public interface IMongoDbConnectionStringFactory
    {
        string GetConnectionString();

        string GetDatabaseName();
    }
}
