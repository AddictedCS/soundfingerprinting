namespace SoundFingerprinting.Dao.MongoDb
{
    using MongoDB.Driver;

    using SoundFingerprinting.Dao.MongoDb.Connection;

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