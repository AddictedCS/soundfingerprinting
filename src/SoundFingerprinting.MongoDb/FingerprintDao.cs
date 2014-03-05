namespace SoundFingerprinting.MongoDb
{
    using System.Collections.Generic;
    using System.Linq;

    using MongoDB.Bson;
    using MongoDB.Driver.Linq;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.MongoDb.Connection;
    using SoundFingerprinting.MongoDb.DAO;
    using SoundFingerprinting.MongoDb.Entity;

    internal class FingerprintDao : AbstractDao, IFingerprintDao
    {
        public const string Fingerprints = "Fingerprints";

        public FingerprintDao()
            : base(DependencyResolver.Current.Get<IMongoDatabaseProviderFactory>())
        {
            // no op
        }

        public IModelReference InsertFingerprint(FingerprintData fingerprintData)
        {
            var fingerprint = new Fingerprint { Signature = fingerprintData.Signature, TrackId = (ObjectId)fingerprintData.TrackReference.Id };
            GetCollection<Fingerprint>(Fingerprints).Insert(fingerprint);
            return fingerprintData.FingerprintReference = new MongoModelReference(fingerprint.Id);
        }

        public IList<FingerprintData> ReadFingerprintsByTrackReference(IModelReference trackReference)
        {
            return GetCollection<Fingerprint>(Fingerprints).AsQueryable()
                                              .Where(f => f.TrackId.Equals(trackReference.Id))
                                              .Select(fingerprint => new FingerprintData(fingerprint.Signature, new MongoModelReference(fingerprint.TrackId))
                                                  {
                                                      FingerprintReference = new MongoModelReference(fingerprint.Id)
                                                  })
                                              .ToList();
        }
    }
}
