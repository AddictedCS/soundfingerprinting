namespace SoundFingerprinting.Data
{
    using System;

    [Serializable]
    public class TrackData
    {
        public TrackData()
        {
            // no op
        }

        public TrackData(string isrc, string artist, string title, string album, int releaseYear, int trackLength)
        {
            ISRC = isrc;
            Artist = artist;
            Title = title;
            Album = album;
            ReleaseYear = releaseYear;
            TrackLengthSec = trackLength;
        }

        public TrackData(string isrc, string artist, string title, string album, int releaseYear, int trackLength, ITrackReference trackReference)
            : this(isrc, artist, title, album, releaseYear, trackLength)
        {
            TrackReference = trackReference;
        }

        public string Artist { get; private set; }

        public string Title { get; private set; }

        public string ISRC { get; private set; }

        public string Album { get; private set; }

        public int ReleaseYear { get; private set; }

        public int TrackLengthSec { get; private set; }

        public ITrackReference TrackReference { get; private set; }

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

            return ((TrackData)obj).TrackReference.HashCode == TrackReference.HashCode;
        }

        public override int GetHashCode()
        {
            return TrackReference.HashCode;
        }
    }
}
