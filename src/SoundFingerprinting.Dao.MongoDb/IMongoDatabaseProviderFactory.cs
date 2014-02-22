namespace SoundFingerprinting.Dao.MongoDb
{
    using MongoDB.Driver;

    public interface IMongoDatabaseProviderFactory
    {
        MongoDatabase GetDatabase();
    }
}