namespace SoundFingerprinting.Dao.MongoDb.Entities
{
    using MongoDB.Bson;

    public class Fingerprint
    {
        public ObjectId Id { get; set; }

        public bool[] Signature { get; set; }

        public ObjectId TrackId { get; set; }
    }
}
