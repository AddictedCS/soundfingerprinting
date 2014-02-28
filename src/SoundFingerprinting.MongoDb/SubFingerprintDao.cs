namespace SoundFingerprinting.MongoDb
{
    using System;
    using System.Linq;

    using MongoDB.Bson;
    using MongoDB.Driver.Linq;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.MongoDb.Connection;
    using SoundFingerprinting.MongoDb.Data;
    using SoundFingerprinting.MongoDb.Entity;

    internal class SubFingerprintDao : AbstractDao, ISubFingerprintDao
    {
        public const string SubFingerprints = "SubFingerprints";

        public SubFingerprintDao() : base(DependencyResolver.Current.Get<IMongoDatabaseProviderFactory>())
        {
            // no op
        }

        public SubFingerprintData ReadSubFingerprint(IModelReference subFingerprintReference)
        {
            var collection = GetCollection<SubFingerprint>(SubFingerprints);
            return collection.AsQueryable()
                             .Where(s => s.Id.Equals(subFingerprintReference.Id))
                             .Select(s => new SubFingerprintData(s.Signature, new MongoModelReference(s.Id), new MongoModelReference(s.TrackId)))
                             .FirstOrDefault();
        }

        public IModelReference InsertSubFingerprint(byte[] signature, IModelReference trackReference)
        {
            var collection = GetCollection<SubFingerprint>(SubFingerprints);
            var subFingerprint = new SubFingerprint
                { 
                    Signature = signature, TrackId = (ObjectId)trackReference.Id 
                };
            collection.Insert(subFingerprint);
            return new MongoModelReference(subFingerprint.Id);
        }
    }
}
