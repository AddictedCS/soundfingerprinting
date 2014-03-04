namespace SoundFingerprinting.MongoDb.Entity
{
    using MongoDB.Bson;

    public class Hash
    {
        public ObjectId Id { get; set; }

        public long[] HashBins { get; set; }

        public ObjectId SubFingerprintId { get; set; }

        public ObjectId TrackId { get; set; }
    }
}
