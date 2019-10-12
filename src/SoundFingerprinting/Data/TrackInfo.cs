namespace SoundFingerprinting.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ProtoBuf;

    [Serializable]
    [ProtoContract]
    public class TrackInfo
    {
        public TrackInfo(string id, string title, string artist, double durationInSeconds) : this(id, title, artist, durationInSeconds, new Dictionary<string, string>(), MediaType.Audio)
        {
            // no op
        }
        
        public TrackInfo(string id, string title, string artist, double durationInSeconds, IDictionary<string, string> metaFields, MediaType mediaType)
        {
            Id = id;
            Title = title;
            Artist = artist;
            DurationInSeconds = durationInSeconds;
            MetaFields = metaFields;
            MediaType = mediaType;
        }

        private TrackInfo()
        {
            // left for proto-buf
            MetaFields = new Dictionary<string, string>();
            MediaType = MediaType.Audio;
        }

        [ProtoMember(1)]
        public string Id { get; }

        [ProtoMember(2)]
        public string Title { get; }

        [ProtoMember(3)]
        public string Artist { get; }

        [ProtoMember(4)]
        public double DurationInSeconds { get; }

        [ProtoMember(5)]
        public IDictionary<string, string> MetaFields { get; }

        [ProtoMember(6)]
        public MediaType MediaType { get; }

        public override string ToString()
        {
            return $"TrackInfo Id: {Id}, " +
                   $"Title {Title}, " +
                   $"Artist {Artist}, " +
                   $"DurationInSeconds {DurationInSeconds}, " +
                   $"MediaType {MediaType}, " +
                   $"MetaFields {string.Join(",", MetaFields.Select(pair => $"{pair.Key}:{pair.Value}"))}";
        }
    }
}
