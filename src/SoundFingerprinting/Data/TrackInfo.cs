namespace SoundFingerprinting.Data
{
    using System;

    using ProtoBuf;

    [Serializable]
    [ProtoContract]
    public class TrackInfo
    {
        public TrackInfo(string id, string title, string artist, double durationInSeconds)
        {
            Id = id;
            Title = title;
            Artist = artist;
            DurationInSeconds = durationInSeconds;
        }

        private TrackInfo()
        {
            // left for protobuf
        }

        [ProtoMember(1)]
        public string Id { get; }

        [ProtoMember(2)]
        public string Title { get; }

        [ProtoMember(3)]
        public string Artist { get; }

        [ProtoMember(4)]
        public double DurationInSeconds { get; }
    }
}
