namespace SoundFingerprinting.DAO.Data
{
    using System;
    using System.Collections.Generic;
    using DAO;

    using ProtoBuf;
    using SoundFingerprinting.Data;

    [Serializable]
    [ProtoContract(SkipConstructor = true)]
    public class TrackData
    {
        public TrackData(string id, string artist, string title, double length, IModelReference trackReference, IDictionary<string, string> metaFields, MediaType mediaType)
        {
            Id = id;
            Artist = artist;
            Title = title;
            Length = length;
            TrackReference = trackReference;
            MetaFields = metaFields;
            MediaType = mediaType;
        }

        public TrackData(string id, string artist, string title, double length, IModelReference trackReference) : this(id, artist, title, length, trackReference, new Dictionary<string, string>(), MediaType.Audio)
        {
        }

        [ProtoMember(3)]
        public string Id { get; }

        [ProtoMember(1)]
        public string Artist { get; }

        [ProtoMember(2)]
        public string Title { get; }

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
