namespace SoundFingerprinting.MongoDb
{
    using MongoDB.Driver;

    using SoundFingerprinting.MongoDb.Connection;

    internal abstract class AbstractDao
    {
        private readonly IMongoDatabaseProviderFactory databaseProvider;

        protected AbstractDao(IMongoDatabaseProviderFactory databaseProvider)
        {
            this.databaseProvider = databaseProvider;
        }

        protected MongoDatabase Database
        {
            get
            {
                return databaseProvider.GetDatabase();
            }
        }

        protected MongoCollection<T> GetCollection<T>(string collectionName)
        {
            return Database.GetCollection<T>(collectionName);
        }
    }
}