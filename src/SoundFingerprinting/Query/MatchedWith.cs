// ReSharper disable UnusedMember.Local
namespace SoundFingerprinting.Query
{
    using ProtoBuf;
    using SoundFingerprinting.LCS;

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
        ///   Score as calculated by score algorithm set by <see cref="QueryPathReconstructionStrategyType"/> strategy. 
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

        /// <summary>
        ///  Gets query sequence number.
        /// </summary>
        [ProtoMember(1)]
        public uint QuerySequenceNumber { get; }

        /// <summary>
        ///  Gets query matched at second.
        /// </summary>
        [ProtoMember(2)]
        public float QueryMatchAt { get; }

        /// <summary>
        ///  Gets track sequence number.
        /// </summary>
        [ProtoMember(3)]
        public uint TrackSequenceNumber { get; }

        /// <summary>
        ///  Gets track matched at second.
        /// </summary>
        [ProtoMember(4)]
        public float TrackMatchAt { get; }

        /// <summary>
        ///  Gets score.
        /// </summary>
        /// <remarks>
        ///  Only valid when <see cref="QueryPathReconstructionStrategyType"/> is set to Legacy, otherwise score will be equal to 1.
        /// </remarks>
        [ProtoMember(5)]
        public double Score { get; }
    }
}