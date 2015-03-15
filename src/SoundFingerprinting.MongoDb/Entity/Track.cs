namespace SoundFingerprinting.MongoDb.Entity
{
    using MongoDB.Bson;

    public class Track
    {
        public ObjectId Id { get; set; }

        public string Artist { get; set; }

        public string Title { get; set; }

        public string ISRC { get; set; }

        public string Album { get; set; }

        public int ReleaseYear { get; set; }

        public int TrackLengthSec { get; set; }

        public string GroupId { get; set; }
    }
}
