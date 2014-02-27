namespace SoundFingerprinting.MongoDb
{
    using MongoDB.Driver;

    using SoundFingerprinting.MongoDb.Connection;
    using SoundFingerprinting.MongoDb.Data;

    internal abstract class AbstractDao
    {
        public const string FingerprintsCollection = "Fingerprints";

        public const string TracksCollection = "Tracks";

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

        protected MongoCollection<Fingerprint> GetCollection(string collectionName)
        {
            return Database.GetCollection<Fingerprint>(collectionName);
        }
    }
}