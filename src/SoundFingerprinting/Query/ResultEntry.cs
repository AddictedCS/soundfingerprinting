// ReSharper disable UnusedMember.Local
namespace SoundFingerprinting.Query
{
    using System;
    using System.Linq;
    using ProtoBuf;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.LCS;

    /// <summary>
    ///  Represents an instance of result entry object containing information about the resulting match
    /// </summary>
    [ProtoContract]
    public class ResultEntry
    {
        public ResultEntry(TrackData track, double confidence, double score, DateTime matchedAt, Coverage coverage)
            : this(track,
                confidence,
                score,
                matchedAt,
                coverage.QueryLength,
                coverage.QueryMatchStartsAt,
                coverage.CoverageWithPermittedGapsLength,
                coverage.DiscreteCoverageLength,
                coverage.TrackMatchStartsAt,
                coverage.TrackStartsAt)
        {
            Coverage = coverage;
        }

        private ResultEntry()
        {
            // left for proto-buf
        }

        [Obsolete("Left for unit tests")]
        public ResultEntry(TrackData track,
            double confidence,
            double score,
            DateTime matchedAt,
            double queryLength,
            double queryMatchStartsAt,
            double coverageWithPermittedGapsLength,
            double discreteCoverageLength,
            double trackMatchStartsAt,
            double trackStartsAt)
        {
            Track = track;
            QueryMatchStartsAt = queryMatchStartsAt;
            CoverageWithPermittedGapsLength = coverageWithPermittedGapsLength;
            DiscreteCoverageLength = discreteCoverageLength;
            TrackMatchStartsAt = trackMatchStartsAt;
            Confidence = confidence;
            Score = score;
            TrackStartsAt = trackStartsAt;
            QueryLength = queryLength;
            MatchedAt = matchedAt;
        }

        /// <summary>
        ///  Gets the resulting matched track from the data store
        /// </summary>
        [ProtoMember(1)]
        public TrackData Track { get; }

        /// <summary>
        ///  Gets coverage of the the provided result entry
        /// </summary>
        [ProtoMember(2)]
        public Coverage Coverage { get; }

        /// <summary>
        ///  Gets the total track length that was covered by the query. Exact length of matched fingerprints, not necessary consecutive.
        /// </summary>
        public double CoverageLength => Coverage.CoverageLength;

        /// <summary>
        /// Gets query coverage length with permitted gaps 
        /// </summary>
        [ProtoMember(3)]
        public double CoverageWithPermittedGapsLength { get; }
        
        /// <summary>
        ///  Gets estimated track coverage inferred from matching start and end of the resulting track in the query
        /// </summary>
        [ProtoMember(11)]
        public double DiscreteCoverageLength { get; }

        /// <summary>
        ///  Gets the exact position in seconds where resulting track started to match in the query
        /// </summary>
        /// <example>
        ///  Query length is of 30 seconds. It started to match at 10th second, <code>QueryMatchStartsAt</code> will be equal to 10.
        /// </example>
        [ProtoMember(4)]
        public double QueryMatchStartsAt { get; }

        /// <summary>
        ///  Gets best guess in seconds where does the result track starts in the query snippet. This value may be negative.
        /// </summary>
        /// <example>
        ///   Resulting Track <c>A</c> in the data store is of 30 sec. The query is of 10 seconds, with <code>TrackMatchStartsAt</code> at 15th second. <code>TrackStartsAt</code> will be equal to -15;
        /// </example>
        [ProtoMember(5)]
        public double TrackStartsAt { get; }

        /// <summary>
        ///  Gets the time position in seconds where the origin track started to match the query
        /// </summary>
        /// <example>
        ///  Resulting track <c>A</c> in the data store is of 100 sec. The query started to match at 40th sec. <code>TrackMatchStartsAt</code> will be equal to 40.
        /// </example>
        [ProtoMember(6)]
        public double TrackMatchStartsAt { get; }

        /// <summary>
        ///  Gets the percentage of how much the query match covered the original track
        /// </summary>
        public double RelativeCoverage => CoverageWithPermittedGapsLength / Track.Length;
        
        /// <summary>
        ///  Gets the percentage of how much the track match covered the original query
        /// </summary>
        public double QueryRelativeCoverage => Coverage.QueryCoverageWithPermittedGapsLength / QueryLength;

        /// <summary>
        ///  Gets the estimated percentage of how much the resulting track got covered by the query
        /// </summary>
        public double DiscreteCoverage => DiscreteCoverageLength / Track.Length;

        /// <summary>
        ///  Gets the value [0, 1) of how confident is the framework that query match corresponds to result track
        /// </summary>
        [ProtoMember(7)]
        public double Confidence { get; }

        /// <summary>
        ///  Gets the exact query length used to generate this entry
        /// </summary>
        [ProtoMember(8)]
        public double QueryLength { get; }

        /// <summary>
        ///  Gets date time instance when did the match took place
        /// </summary>
        [ProtoMember(9)]
        public DateTime MatchedAt { get; }

        /// <summary>
        ///  Gets similarity count between query match and track
        /// </summary>
        [ProtoMember(10)]
        public double Score { get; }



        /// <summary>
        ///  Gets information about gaps in the result entry coverage
        /// </summary>
        public bool NoGaps => !Coverage.TrackGaps.Any() && !Coverage.QueryGaps.Any();
    }
}