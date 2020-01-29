namespace SoundFingerprinting.Query
{
    using System;
    using SoundFingerprinting.DAO.Data;

    /// <summary>
    ///  Represents an instance of result entry object containing information about the resulting match
    /// </summary>
    public class ResultEntry
    {
        public ResultEntry(TrackData track, 
            double queryMatchStartsAt, 
            double coverageLength, 
            double discreteCoverageLength, 
            double trackMatchStartsAt, 
            double trackStartsAt, 
            double confidence, 
            double score, 
            double queryLength,
            DateTime matchedAt)
        {
            Track = track;
            QueryMatchStartsAt = queryMatchStartsAt;
            CoverageLength = coverageLength;
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
        public TrackData Track { get; }

        /// <summary>
        /// Gets query coverage sum in seconds. Exact length of matched fingerprints, not necessary consecutive, just how much length has been covered by the query
        /// </summary>
        public double CoverageLength { get; }
        
        /// <summary>
        ///  Gets the exact position in seconds where resulting track started to match in the query
        /// </summary>
        /// <example>
        ///  Query length is of 30 seconds. It started to match at 10th second, <code>QueryMatchStartsAt</code> will be equal to 10.
        /// </example>
        public double QueryMatchStartsAt { get; }

        /// <summary>
        ///  Gets best guess in seconds where does the result track starts in the query snippet. This value may be negative.
        /// </summary>
        /// <example>
        ///   Resulting Track <c>A</c> in the data store is of 30 sec. The query is of 10 seconds, with <code>TrackMatchStartsAt</code> at 15th second. <code>TrackStartsAt</code> will be equal to -15;
        /// </example>
        public double TrackStartsAt { get; }

        /// <summary>
        ///  Gets the time position in seconds where the origin track started to match the query
        /// </summary>
        /// <example>
        ///  Resulting track <c>A</c> in the data store is of 100 sec. The query started to match at 40th sec. <code>TrackMatchStartsAt</code> will be equal to 40.
        /// </example>
        public double TrackMatchStartsAt { get; }

        /// <summary>
        ///  Gets the percentage of how much the query match covered the original track
        /// </summary>
        public double RelativeCoverage => CoverageLength / Track.Length;

        /// <summary>
        ///  Gets the estimated percentage of how much the resulting track got covered by the query
        /// </summary>
        public double DiscreteCoverage => DiscreteCoverageLength / Track.Length;

        /// <summary>
        ///  Gets the value [0, 1) of how confident is the framework that query match corresponds to result track
        /// </summary>
        public double Confidence { get; }

        /// <summary>
        ///  Gets the exact query length used to generate this entry
        /// </summary>
        public double QueryLength { get; }

        /// <summary>
        ///  Gets date time instance when did the match took place
        /// </summary>
        public DateTime MatchedAt { get; }

        /// <summary>
        ///  Gets similarity count between query match and track
        /// </summary>
        public double Score { get; }

        /// <summary>
        ///  Gets estimated track coverage inferred from matching start and end of the resulting track in the query
        /// </summary>
        public double DiscreteCoverageLength { get; }
    }
}