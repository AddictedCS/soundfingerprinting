namespace SoundFingerprinting.MongoDb.Entity
{
    using MongoDB.Bson;

    using SoundFingerprinting.Data;

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

        public static Track FromTrackData(TrackData trackData)
        {
            Track track = new Track
                {
                    Artist = trackData.Artist,
                    Title = trackData.Title,
                    ISRC = trackData.ISRC,
                    Album = trackData.Album,
                    ReleaseYear = trackData.ReleaseYear,
                    TrackLengthSec = trackData.TrackLengthSec,
                    GroupId = trackData.GroupId
                };

            if (trackData.TrackReference != null)
            {
                track.Id = (ObjectId)trackData.TrackReference.Id;
            }

            return track;
        }
    }
}
