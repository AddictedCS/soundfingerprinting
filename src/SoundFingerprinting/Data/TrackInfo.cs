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
        public TrackInfo(string id, string title, string artist, double durationInSeconds) : this(id, title, artist, durationInSeconds, new Dictionary<string, string>())
        {
            // no op
        }
        
        public TrackInfo(string id, string title, string artist, double durationInSeconds, IDictionary<string, string> metaFields)
        {
            Id = id;
            Title = title;
            Artist = artist;
            DurationInSeconds = durationInSeconds;
            MetaFields = metaFields;
        }

        private TrackInfo()
        {
            // left for proto-buf
            MetaFields = new Dictionary<string, string>();
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

        public override string ToString()
        {
            return $"TrackInfo Id: {Id}, " +
                   $"Title {Title}, " +
                   $"Artist {Artist}, " +
                   $"DurationInSeconds {DurationInSeconds}, " +
                   $"MetaFields {string.Join(",", MetaFields.Select(pair => $"{pair.Key}:{pair.Value}"))}";
        }
    }
}
