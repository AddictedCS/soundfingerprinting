namespace SoundFingerprinting.MongoDb.Data
{
    using MongoDB.Bson;

    internal class Fingerprint
    {
        public ObjectId Id { get; set; }

        public bool[] Signature { get; set; }

        public ObjectId TrackId { get; set; }
    }
}
