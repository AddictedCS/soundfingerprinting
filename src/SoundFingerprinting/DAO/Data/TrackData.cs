namespace SoundFingerprinting.DAO.Data
{
    using System;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.DAO;

    [Serializable]
    public class TrackData
    {
        public TrackData()
        {
            // no op
        }

        public TrackData(string isrc, string artist, string title, string album, int releaseYear, double length)
        {
            ISRC = isrc;
            Artist = artist;
            Title = title;
            Album = album;
            ReleaseYear = releaseYear;
            Length = length;
        }

        public TrackData(TagInfo tags) : this(tags.ISRC, tags.Artist, tags.Title, tags.Album, tags.Year, tags.Duration)
        {
        }

        public TrackData(
            string isrc,
            string artist,
            string title,
            string album,
            int releaseYear,
            double length,
            IModelReference trackReference)
            : this(isrc, artist, title, album, releaseYear, length)
        {
            TrackReference = trackReference;
        }

        public string Artist { get; set; }

        public string Title { get; set; }

        public string ISRC { get; set; }

        public string Album { get; set; }

        public int ReleaseYear { get; set; }

        public double Length { get; set; }

        [IgnoreBinding]
        public IModelReference TrackReference { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (!(obj is TrackData))
            {
                return false;
            }

            return ((TrackData)obj).TrackReference.Equals(TrackReference);
        }

        public override int GetHashCode()
        {
            return TrackReference.GetHashCode();
        }
    }
}
