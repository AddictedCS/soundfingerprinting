namespace SoundFingerprinting.MongoDb
{
    using MongoDB.Driver;

    using SoundFingerprinting.MongoDb.Connection;

    public abstract class AbstractDao
    {
        private readonly IMongoDatabaseProviderFactory databaseProvider;

        protected AbstractDao(IMongoDatabaseProviderFactory databaseProvider)
        {
            this.databaseProvider = databaseProvider;
        }

        public MongoDatabase Database
        {
            get
            {
                return databaseProvider.GetDatabase();
            }
        }
    }
}