namespace SoundFingerprinting.Data
{
    using System;
    using ProtoBuf;
    using SoundFingerprinting.LCS;

    /// <summary>
    ///  Class that describes a successful query match.
    /// </summary>
    [ProtoContract]
    public class QueryMatch
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryMatch"/> class.
        /// </summary>
        /// <param name="queryMatchId">Unique query match identifier.</param>
        /// <param name="track">Track corresponding to the query match.</param>
        /// <param name="coverage">An instance of the <see cref="Coverage"/> class, describing the match.</param>
        /// <param name="matchedAt">Matched at (calculated relative to <see cref="Hashes.RelativeTo"/> property).</param>
        public QueryMatch(string queryMatchId, TrackInfo track, Coverage coverage, DateTime matchedAt)
        {
            QueryMatchId = queryMatchId;
            Track = track;
            Coverage = coverage;
            MatchedAt = matchedAt;
        }

        private QueryMatch()
        {
            // left for proto-buf
        }

        /// <summary>
        ///  Gets an instance of the <see cref="TrackInfo"/> that matched the query.
        /// </summary>
        [ProtoMember(1)] 
        public TrackInfo Track { get; }

        /// <summary>
        ///  Gets coverage information about the match.
        /// </summary>
        [ProtoMember(2)] 
        public Coverage Coverage { get; }

        /// <summary>
        ///  Gets matched at reference (calculated relative to <see cref="Hashes.RelativeTo"/> property).
        /// </summary>
        [ProtoMember(3)] 
        public DateTime MatchedAt { get; }

        /// <summary>
        ///  Gets query matches unique identifier.
        /// </summary>
        [ProtoMember(4)] 
        public string QueryMatchId { get; }
    }
}