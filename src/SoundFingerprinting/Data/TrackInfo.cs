namespace SoundFingerprinting.Data
{
    public class TrackInfo
    {
        public TrackInfo(string isrc, string title, string artist, double durationInSeconds)
        {
            Isrc = isrc;
            Title = title;
            Artist = artist;
            DurationInSeconds = durationInSeconds;
        }

        public string Isrc { get; }

        public string Title { get; }

        public string Artist { get; }

        public double DurationInSeconds { get; }
    }
}
