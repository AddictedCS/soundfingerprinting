namespace SoundFingerprinting.MongoDb.Tests.Integration
{
    using MongoDB.Driver;

    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.MongoDb.Connection;

    internal class DaoTestHelper : AbstractDao
    {
        public DaoTestHelper()
            : base(DependencyResolver.Current.Get<MongoDatabaseProviderFactory>())
        {
            // no op
        }

        public new MongoCollection<T> GetCollection<T>(string collectionName)
        {
            return base.GetCollection<T>(collectionName);
        } 
    }
}
