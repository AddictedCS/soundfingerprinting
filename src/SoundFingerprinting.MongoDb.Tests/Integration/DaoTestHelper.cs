namespace SoundFingerprinting.MongoDb.Tests.Integration
{
    using MongoDB.Driver;

    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.MongoDb.Connection;
    using SoundFingerprinting.MongoDb.Data;

    internal class DaoTestHelper : AbstractDao
    {
        public DaoTestHelper()
            : base(DependencyResolver.Current.Get<IMongoDatabaseProviderFactory>())
        {
            // no op
        }

        public new MongoCollection<T> GetCollection<T>(string collectionName)
        {
            return base.GetCollection<T>(collectionName);
        } 
    }
}
