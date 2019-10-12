namespace SoundFingerprinting.DAO.Data
{
    using System;
    using System.Collections.Generic;
    using DAO;

    using ProtoBuf;
    using SoundFingerprinting.Data;

    [Serializable]
    [ProtoContract]
    public class TrackData
    {
        public TrackData(string isrc, string artist, string title, string album, int releaseYear, double length, IModelReference trackReference, IDictionary<string, string> metaFields, MediaType mediaType)
        {
            ISRC = isrc;
            Artist = artist;
            Title = title;
            Album = album;
            ReleaseYear = releaseYear;
            Length = length;
            TrackReference = trackReference;
            MetaFields = metaFields;
            MediaType = mediaType;
        }

        public TrackData(string isrc, string artist, string title, string album, int releaseYear, double length, IModelReference trackReference) : 
            this(isrc, artist, title, album, releaseYear, length, trackReference, new Dictionary<string, string>(), MediaType.Audio)
        {
        }

        public TrackData()
        {
            // left for proto-buf
            MetaFields = new Dictionary<string, string>();
            MediaType = MediaType.Audio;
        }

        public string Id => ISRC;

        [ProtoMember(1)]
        public string Artist { get; }

        [ProtoMember(2)]
        public string Title { get; }

        [ProtoMember(3)]
        [Obsolete("Will be renamed to `Id` in upcoming versions.")]
        private string ISRC { get; }

        [Obsolete("Will be removed in upcoming versions. Use MetaFields instead.")]
        [ProtoMember(4)]
        private string Album { get; }

        [Obsolete("Will be removed in upcoming versions. Use MetaFields instead.")]
        [ProtoMember(5)]
        private int ReleaseYear { get; }

        [ProtoMember(6)]
        public double Length { get; }

        [IgnoreBinding]
        [ProtoMember(7)]
        public IModelReference TrackReference { get; }

        [IgnoreBinding]
        [ProtoMember(8)]
        public IDictionary<string, string> MetaFields { get; }

        [ProtoMember(9)]
        public MediaType MediaType { get; }

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
