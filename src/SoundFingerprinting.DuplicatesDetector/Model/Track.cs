namespace SoundFingerprinting.DuplicatesDetector.Model
{
    using System;

    using SoundFingerprinting.Data;

    [Serializable]
    public class Track : TrackData
    {
        private const int MaxPathLength = 255;

        private string path;

        public Track(
            string path, string isrc, string artist, string title, string album, int releaseYear, int trackLength)
            : base(isrc, artist, title, album, releaseYear, trackLength)
        {
            Path = path;
        }

        public string Path
        {
            get
            {
                return path;
            }

            set
            {
                path = value.Length > MaxPathLength ? value.Substring(0, MaxPathLength) : value;
            }
        }
    }
}