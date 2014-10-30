namespace SoundFingerprinting.MongoDb.Entity
{
    using MongoDB.Bson;

    public class SubFingerprint
    {
        public ObjectId Id { get; set; }

        public byte[] Signature { get; set; }

        public int SequenceNumber { get; set; }

        public ObjectId TrackId { get; set; }
    }
}
