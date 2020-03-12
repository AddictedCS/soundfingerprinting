// ReSharper disable UnusedMember.Local
namespace SoundFingerprinting.Query
{
    using ProtoBuf;

    [ProtoContract]
    public class MatchedWith
    {
        public MatchedWith(uint querySequenceNumber, float queryMatchAt, uint trackSequenceNumber, float trackMatchAt, double score)
        {
            QuerySequenceNumber = querySequenceNumber;
            QueryMatchAt = queryMatchAt;
            TrackMatchAt = trackMatchAt;
            TrackSequenceNumber = trackSequenceNumber;
            Score = score;
        }

        private MatchedWith()
        {
            // left for proto-buf
        }

        [ProtoMember(1)]
        public uint QuerySequenceNumber { get; }

        [ProtoMember(2)]
        public float QueryMatchAt { get; }

        [ProtoMember(3)]
        public uint TrackSequenceNumber { get; }

        [ProtoMember(4)]
        public float TrackMatchAt { get; }

        [ProtoMember(5)]
        public double Score { get; }
    }
}