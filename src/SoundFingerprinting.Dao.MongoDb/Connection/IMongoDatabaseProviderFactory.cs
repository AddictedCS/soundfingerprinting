namespace SoundFingerprinting.Dao.MongoDb.Connection
{
    using MongoDB.Driver;

    public interface IMongoDatabaseProviderFactory
    {
        MongoDatabase GetDatabase();
    }
}