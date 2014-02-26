namespace SoundFingerprinting.MongoDb
{
    using System.Collections.Generic;
    using System.Linq;

    using MongoDB.Bson;
    using MongoDB.Driver;
    using MongoDB.Driver.Linq;

    using SoundFingerprinting.Data;
    using SoundFingerprinting.MongoDb.Connection;
    using SoundFingerprinting.MongoDb.Data;

    public class FingerprintDao : AbstractDao
    {
        private const string FingerprintsCollection = "Fingerprints";

        public FingerprintDao(IMongoDatabaseProviderFactory databaseProvider)
            : base(databaseProvider)
        {
        }

        public ObjectId Insert(bool[] signature, ObjectId trackId)
        {
            var collection = GetFingerprintsCollection();
            var fingerprint = new Fingerprint { Signature = signature, TrackId = trackId };
            collection.Insert(fingerprint);
            return fingerprint.Id;
        }

        public IList<FingerprintData> ReadFingerprintsByTrackId(ObjectId trackId)
        {
            return GetFingerprintsCollection().AsQueryable()
                                              .Where(f => f.TrackId.Equals(trackId))
                                              .Select(fingerprint => new FingerprintData(fingerprint.Signature, new MongoModelReference(fingerprint.TrackId)))
                                              .ToList();
        }

        private MongoCollection<Fingerprint> GetFingerprintsCollection()
        {
            var collection = Database.GetCollection<Fingerprint>(FingerprintsCollection);
            return collection;
        }
    }
}
