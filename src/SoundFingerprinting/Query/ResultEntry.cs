namespace SoundFingerprinting.Query
{
    using System;
    using ProtoBuf;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.LCS;

    /// <summary>
    ///  Represents an instance of result entry object containing information about the resulting match.
    /// </summary>
    [ProtoContract(SkipConstructor = true)]
    public class ResultEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResultEntry"/> class.
        /// </summary>
        /// <param name="track">Matched track.</param>
        /// <param name="score">Match score.</param>
        /// <param name="matchedAt">Matched at. To identify relative time when the match starts, set  <see cref="Hashes.RelativeTo"/> during query time.</param>
        /// <param name="coverage">An instance of the <see cref="Coverage"/> object.</param>
        public ResultEntry(TrackData track, double score, DateTime matchedAt, Coverage coverage)
        {
            Coverage = coverage;
            Track = track;
            QueryMatchStartsAt = Coverage.QueryMatchStartsAt;
            TrackCoverageWithPermittedGapsLength = Coverage.TrackCoverageWithPermittedGapsLength;
            DiscreteTrackCoverageLength = Coverage.TrackDiscreteCoverageLength;
            TrackMatchStartsAt = Coverage.TrackMatchStartsAt;
            Confidence = Coverage.Confidence;
            Score = score;
            TrackStartsAt = Coverage.TrackStartsAt;
            QueryLength = Coverage.QueryLength;
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
        /// Gets query coverage length with permitted gaps 
        /// </summary>
        [ProtoMember(3)]
        public double TrackCoverageWithPermittedGapsLength { get; }

        /// <summary>
        ///  Gets estimated track coverage inferred from matching start and end of the resulting track in the query
        /// </summary>
        [ProtoMember(11)]
        public double DiscreteTrackCoverageLength { get; }

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
        ///  Gets the time position in seconds where the origin track started to match the query.
        /// </summary>
        /// <example>
        ///  Resulting track <c>A</c> in the data store is of 100 sec. The query started to match at 40th sec. <code>TrackMatchStartsAt</code> will be equal to 40.
        /// </example>
        [ProtoMember(6)]
        public double TrackMatchStartsAt { get; }

        /// <summary>
        ///  Gets the percentage of how much the query match covered the original track.
        /// </summary>
        public double TrackRelativeCoverage => TrackCoverageWithPermittedGapsLength / Track.Length;

        /// <summary>
        ///  Gets the percentage of how much the track match covered the original query
        /// </summary>
        public double QueryRelativeCoverage => Coverage.QueryCoverageWithPermittedGapsLength / QueryLength;

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
        [Obsolete("Will be removed in v9. To use a similarity metric use Coverage property.")]
        public double Score { get; }

        /// <summary>
        ///  Convert ResultEntry to an instance of <see cref="QueryMatch"/>.
        /// </summary>
        /// <returns>Instance of <see cref="QueryMatch"/>.</returns>
        public QueryMatch ToQueryMatch()
        {
            var trackInfo = new TrackInfo(Track.Id, Track.Title, Track.Artist, Track.MetaFields, Track.MediaType);
            return new QueryMatch(Guid.NewGuid().ToString(), trackInfo, Coverage, MatchedAt);
        }

        /// <inheritdoc cref="object.ToString"/>
        public override string ToString()
        {
            return $"ResultEntry[TrackId={Track.Id},TrackMatchStartsAt={TrackMatchStartsAt:0.00},TrackRelativeCoverage={TrackRelativeCoverage:0.00},QueryMatchStartsAt={QueryMatchStartsAt:0.00},QueryRelativeCoverage={QueryRelativeCoverage:0.00},TrackLength={Track.Length:0.00},QueryLength={QueryLength:0.00},MatchedAt={MatchedAt:O}]";
        }
    }
}