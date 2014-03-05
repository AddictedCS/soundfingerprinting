namespace SoundFingerprinting.MongoDb.Connection
{
    public interface IMongoDbConnectionStringFactory
    {
        string GetConnectionString();

        string GetDatabaseName();
    }
}
