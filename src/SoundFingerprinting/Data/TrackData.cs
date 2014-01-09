namespace SoundFingerprinting.Data
{
    public class TrackData
    {
        public TrackData(string isrc, string artist, string title, string album, int releaseYear, int trackLength)
        {
            ISRC = isrc;
            Artist = artist;
            Title = title;
            Album = album;
            ReleaseYear = releaseYear;
            TrackLengthSec = trackLength;
        }

        public int Id { get; set; }

        public string Artist { get; set; }

        public string Title { get; set; }

        public string ISRC { get; set; }

        public string Album { get; set; }

        public int ReleaseYear { get; set; }

        public int TrackLengthSec { get; set; }
    }
}
