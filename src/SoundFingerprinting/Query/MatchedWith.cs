// ReSharper disable UnusedMember.Local
namespace SoundFingerprinting.Query
{
    using System;
    using ProtoBuf;
    using SoundFingerprinting.LCS;

    /// <summary>
    ///  Match pair containing information track/query match tuple.
    /// </summary>
    [ProtoContract]
    public class MatchedWith : IEquatable<MatchedWith>
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

        /// <inheritdoc cref="object.ToString"/>
        public override string ToString()
        {
            return $"MatchedWith[QuerySequenceNumber: {QuerySequenceNumber}, QueryMatchAt: {QueryMatchAt}, TrackSequenceNumber: {TrackSequenceNumber}, TrackMatchAt: {TrackMatchAt}, Score: {Score}]";
        }

        /// <inheritdoc />
        public bool Equals(MatchedWith? other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return QuerySequenceNumber == other.QuerySequenceNumber && TrackSequenceNumber == other.TrackSequenceNumber;
        }

        /// <inheritdoc cref="object.Equals(object)"/>
        public override bool Equals(object? obj)
        {
            return Equals(obj as MatchedWith);
        }

        /// <inheritdoc cref="object.GetHashCode"/>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)QuerySequenceNumber * 397) ^ (int)TrackSequenceNumber;
            }
        }

        /// <summary>
        ///  Equality operator.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if equal, false otherwise.</returns>
        public static bool operator ==(MatchedWith? left, MatchedWith? right)
        {
            return Equals(left, right);
        }

        /// <summary>
        ///  Inequality operator.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if not equal, false otherwise.</returns>
        public static bool operator !=(MatchedWith? left, MatchedWith? right)
        {
            return !Equals(left, right);
        }
    }
}