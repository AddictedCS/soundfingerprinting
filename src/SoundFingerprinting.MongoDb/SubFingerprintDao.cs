namespace SoundFingerprinting.MongoDb
{
    using System.Linq;

    using MongoDB.Bson;
    using MongoDB.Driver.Linq;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.MongoDb.Connection;
    using SoundFingerprinting.MongoDb.DAO;
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
            return GetCollection<SubFingerprint>(SubFingerprints).AsQueryable()
                             .Where(s => s.Id.Equals(subFingerprintReference.Id))
                             .Select(s => new SubFingerprintData(s.Signature, s.SequenceNumber, s.SequenceAt, new MongoModelReference(s.Id), new MongoModelReference(s.TrackId)))
                             .FirstOrDefault();
        }

        public IModelReference InsertSubFingerprint(byte[] signature, int sequenceNumber, double sequenceAt, IModelReference trackReference)
        {
            var subFingerprint = new SubFingerprint
                { 
                    Signature = signature, TrackId = (ObjectId)trackReference.Id, SequenceNumber = sequenceNumber, SequenceAt = sequenceAt
                };
            GetCollection<SubFingerprint>(SubFingerprints).Insert(subFingerprint);
            return new MongoModelReference(subFingerprint.Id);
        }
    }
}
