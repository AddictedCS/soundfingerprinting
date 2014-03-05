namespace SoundFingerprinting.MongoDb.Entity
{
    using MongoDB.Bson;

    public class Hash
    {
        public ObjectId Id { get; set; }

        public int HashTable { get; set; }

        public long HashBin { get; set; }

        public ObjectId SubFingerprintId { get; set; }

        public ObjectId TrackId { get; set; }
    }
}
