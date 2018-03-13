namespace SoundFingerprinting.Audio
{
    public class TagInfo
    {
        public bool IsEmpty { get; set; }

        public double Duration { get; set; }

        public string Album { get; set; }

        public string Artist { get; set; }

        public string Title { get; set; }

        public string AlbumArtist { get; set; }

        public string Genre { get; set; }

        public int Year { get; set; }

        public string Composer { get; set; }

        public string ISRC { get; set; }

        public bool IsTrackUniquelyIdentifiable()
        {
            var isNot = IsEmpty || (string.IsNullOrEmpty(ISRC) && (string.IsNullOrEmpty(Artist) || string.IsNullOrEmpty(Title)));
            return !isNot;
        }
    }
}
