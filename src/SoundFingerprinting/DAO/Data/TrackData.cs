namespace SoundFingerprinting.DAO.Data
{
    using System;

    using Audio;
    using DAO;

    using ProtoBuf;

    [Serializable]
    [ProtoContract]
    public class TrackData
    {
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

        public TrackData()
        {
            // this internal parameterless constructor is left here to allow datastorages that leverage reflection to instantiate objects
            // nontheless it is going to be removed in future versions
        }

        [ProtoMember(1)]
        public string Artist { get; internal set; }

        [ProtoMember(2)]
        public string Title { get; internal set; }

        [ProtoMember(3)]
        public string ISRC { get; internal set; }

        [ProtoMember(4)]
        public string Album { get; internal set; }

        [ProtoMember(5)]
        public int ReleaseYear { get; internal set; }

        [ProtoMember(6)]
        public double Length { get; internal set; }

        [IgnoreBinding]
        [ProtoMember(7)]
        public IModelReference TrackReference { get; internal set; }

        public override bool Equals(object obj)
        {
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
