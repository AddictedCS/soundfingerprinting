// ReSharper disable UnusedMember.Local
namespace SoundFingerprinting.Query
{
    using ProtoBuf;

    /// <summary>
    ///  Match pair containing information track/query match tuple.
    /// </summary>
    [ProtoContract]
    public class MatchedWith
    {
        /// <summary>
        ///  Initializes a new instance of the <see cref="MatchedWith"/> class.
        /// </summary>
        /// <param name="querySequenceNumber">Query sequence number.</param>
        /// <param name="queryMatchAt">Query match at.</param>
        /// <param name="trackSequenceNumber">Track sequence number.</param>
        /// <param name="trackMatchAt">Track match at.</param>
        /// <param name="score">
        ///   Score as calculated by <see cref="IScoreAlgorithm"/>.
        /// </param>
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