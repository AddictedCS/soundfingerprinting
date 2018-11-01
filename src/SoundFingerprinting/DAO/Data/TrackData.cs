namespace SoundFingerprinting.DAO.Data
{
    using System;

    using DAO;

    using ProtoBuf;

    [Serializable]
    [ProtoContract]
    public class TrackData
    {
        public TrackData(string isrc, string artist, string title, string album, int releaseYear, double length, IModelReference trackReference)
        {
            ISRC = isrc;
            Artist = artist;
            Title = title;
            Album = album;
            ReleaseYear = releaseYear;
            Length = length;
            TrackReference = trackReference;
        }

        private TrackData()
        {
            // left for protobuf
        }

        [ProtoMember(1)]
        public string Artist { get; }

        [ProtoMember(2)]
        public string Title { get; }

        [ProtoMember(3)]
        public string ISRC { get; }

        [Obsolete("Will be removed in upcoming versions")]
        [ProtoMember(4)]
        public string Album { get; }

        [Obsolete("Will be removed in upcoming versions")]
        [ProtoMember(5)]
        public int ReleaseYear { get; }

        [ProtoMember(6)]
        public double Length { get; }

        [IgnoreBinding]
        [ProtoMember(7)]
        public IModelReference TrackReference { get; }

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
