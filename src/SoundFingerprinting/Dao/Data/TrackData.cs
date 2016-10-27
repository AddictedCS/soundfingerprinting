﻿namespace SoundFingerprinting.DAO.Data
{
    using System;

    using SoundFingerprinting.DAO;

    [Serializable]
    public class TrackData
    {
        public TrackData()
        {
            // no op
        }

        public TrackData(string isrc, string artist, string title, string album, int releaseYear, double trackLength)
        {
            ISRC = isrc;
            Artist = artist;
            Title = title;
            Album = album;
            ReleaseYear = releaseYear;
            TrackLengthSec = trackLength;
        }

        public TrackData(string isrc, string artist, string title, string album, int releaseYear, double trackLength, IModelReference trackReference)
            : this(isrc, artist, title, album, releaseYear, trackLength)
        {
            TrackReference = trackReference;
        }

        public string Artist { get; set; }

        public string Title { get; set; }

        public string ISRC { get; set; }

        public string Album { get; set; }

        public int ReleaseYear { get; set; }

        public double TrackLengthSec { get; set; }

        public string GroupId { get; set; }

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
