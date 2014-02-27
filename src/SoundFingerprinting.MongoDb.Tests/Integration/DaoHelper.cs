namespace SoundFingerprinting.MongoDb.Tests.Integration
{
    using MongoDB.Driver;

    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.MongoDb.Connection;
    using SoundFingerprinting.MongoDb.Data;

    internal class DaoHelper : AbstractDao
    {
        public DaoHelper()
            : base(DependencyResolver.Current.Get<IMongoDatabaseProviderFactory>())
        {
            // no op
        }

        public new MongoCollection<Fingerprint> GetCollection(string collectionName)
        {
            return base.GetCollection(collectionName);
        } 
    }
}
