namespace SoundFingerprinting.Dao.MongoDb
{
    public interface IMongoDbConnectionStringFactory
    {
        string GetConnectionString();

        string GetDatabaseName();
    }
}
