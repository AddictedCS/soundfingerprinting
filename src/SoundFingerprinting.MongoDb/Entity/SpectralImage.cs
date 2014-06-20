namespace SoundFingerprinting.MongoDb.Entity
{
    using MongoDB.Bson;

    internal class SpectralImage
    {
        public ObjectId Id { get; set; }

        public ObjectId TrackId { get; set; }

        public float[] Image { get; set; }

        public int OrderNumber { get; set; }
    }
}
