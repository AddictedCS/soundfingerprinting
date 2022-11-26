// ReSharper disable UnusedMember.Local
namespace SoundFingerprinting.LCS
{
    using System.Collections.Generic;
    using System.Linq;
    using ProtoBuf;
    using SoundFingerprinting.Query;

    /// <summary>
    ///  Object containing information about query match coverage
    /// </summary>
    [ProtoContract(SkipConstructor = true)]
    public class Coverage
    {
        /// <summary>
        ///  Initializes a new instance of the <see cref="Coverage"/> class.
        /// </summary>
        /// <param name="bestPath">Best path between query and track, calculated by given <see cref="QueryPathReconstructionStrategyType"/> query parameter.</param>
        /// <param name="queryLength">Query length in seconds.</param>
        /// <param name="trackLength">Track length in seconds.</param>
        /// <param name="fingerprintLength">Fingerprint length in seconds.</param>
        /// <param name="permittedGap">Length of the permitted gap.</param>
        public Coverage(IEnumerable<MatchedWith> bestPath, double queryLength, double trackLength, double fingerprintLength, double permittedGap)
        {
            BestPath = bestPath.ToList();
            QueryLength = queryLength;
            TrackLength = trackLength;
            FingerprintLength = fingerprintLength;
            PermittedGap = permittedGap;
        }

        /// <summary>
        ///  Gets calculated confidence of the coverage object.
        ///  A value between [0, 1) equaling the probability that the query match is correct.
        /// </summary>
        public double Confidence =>
            ConfidenceCalculator.CalculateConfidence(QueryMatchStartsAt,
                QueryLength,
                TrackMatchStartsAt,
                TrackLength,
                TrackCoverageWithPermittedGapsLength,
                QueryDiscreteCoverageLength,
                TrackDiscreteCoverageLength);

        /// <summary>
        ///  Gets the starting point of the query match. Measured in seconds.
        /// </summary>
        public double QueryMatchStartsAt => BestPath.First().QueryMatchAt;

        /// <summary>
        ///  Gets the starting point of the track match. Measured in seconds.
        /// </summary>
        public double TrackMatchStartsAt => BestPath.First().TrackMatchAt;
        
        /// <summary>
        ///  Gets ending point of the track match. Measured in seconds.
        /// </summary>
        public double TrackMatchEndsAt => TrackMatchStartsAt + TrackDiscreteCoverageLength;

        /// <summary>
        ///  Gets ending point of the query match. Measured in seconds.
        /// </summary>
        public double QueryMatchEndsAt => QueryMatchStartsAt + QueryDiscreteCoverageLength;

        /// <summary>
        ///  Gets track coverage length sum in seconds, allowing gaps specified by permitted gap query parameter.
        /// </summary>
        public double TrackCoverageWithPermittedGapsLength
        {
            get
            {
                return TrackDiscreteCoverageLength - TrackGaps.Where(g => !g.IsOnEdge).Sum(d => d.LengthInSeconds);
            }
        }
        
        /// <summary>
        ///  Gets query coverage length sum in seconds, allowing gaps specified by permitted gap query parameter
        /// </summary>
        public double QueryCoverageWithPermittedGapsLength
        {
            get
            {
                return QueryDiscreteCoverageLength - QueryGaps.Where(g => !g.IsOnEdge).Sum(d => d.LengthInSeconds);
            }
        }

        /// <summary>
        ///  Gets the track match length including all track gaps (if any).
        /// </summary>
        public double TrackDiscreteCoverageLength => SubFingerprintsToSeconds.MatchLengthToSeconds(BestPath.Last().TrackMatchAt, TrackMatchStartsAt, FingerprintLength);

        /// <summary>
        ///  Gets the query match length including all query gaps (if any).
        /// </summary>
        public double QueryDiscreteCoverageLength => SubFingerprintsToSeconds.MatchLengthToSeconds(BestPath.Last().QueryMatchAt, QueryMatchStartsAt, FingerprintLength);

        /// <summary>
        ///  Gets the length of not covered portion of the query match in the track
        /// </summary>
        /// <returns>Seconds of not covered length</returns>
        public double TrackGapsCoverageLength
        {
            get
            {
                return TrackGaps.Sum(gap => gap.LengthInSeconds);
            }
        }

        /// <summary>
        ///  Gets the length of not covered portion of the track in the query
        /// </summary>
        /// <returns>Seconds of not covered length</returns>
        public double QueryGapsCoverageLength
        {
            get
            {
                return QueryGaps.Sum(gap => gap.LengthInSeconds); 
            }
        }
        
        /// <summary>
        ///  Gets best estimate of where does the track actually starts.
        ///  Can be negative, if algorithm assumes the track starts in the past point relative to the query
        /// </summary>
        public double TrackStartsAt => QueryMatchStartsAt - TrackMatchStartsAt;

        /// <summary>
        ///  Gets query length
        /// </summary>
        [ProtoMember(1)]
        public double QueryLength { get; }
        
        /// <summary>
        ///  Gets track length
        /// </summary>
        [ProtoMember(2)]
        public double TrackLength { get; }

        /// <summary>
        ///  Gets best reconstructed path
        /// </summary>
        [ProtoMember(3)]
        public IEnumerable<MatchedWith> BestPath { get; }

        /// <summary>
        ///  Gets query match gaps from the best path
        /// </summary>
        public IEnumerable<Gap> QueryGaps => BestPath.FindQueryGaps(QueryLength, PermittedGap, FingerprintLength);

        /// <summary>
        ///  Gets track match gaps from the best path
        /// </summary>
        public IEnumerable<Gap> TrackGaps => BestPath.FindTrackGaps(TrackLength, PermittedGap, FingerprintLength);

        /// <summary>
        ///  Checks if this coverage contains other coverage.
        /// </summary>
        /// <param name="other">Instance of other coverage.</param>
        /// <param name="delta">Allowed misalignment.</param>
        /// <returns>True if contains, otherwise false.</returns>
        public bool Contains(Coverage other, double delta = 0d)
        {
            return TrackMatchStartsAt - delta <= other.TrackMatchStartsAt && TrackMatchEndsAt + delta >= other.TrackMatchEndsAt 
                   && 
                   QueryMatchStartsAt - delta <= other.QueryMatchStartsAt && QueryMatchEndsAt + delta >= other.QueryMatchEndsAt;
        }

        /// <summary>
        ///  Gets fingerprint length used in calculating coverage information.
        /// </summary>
        [ProtoMember(4)]
        public double FingerprintLength { get; }

        /// <summary>
        ///  Gets permitted gap used in calculating coverage information.
        /// </summary>
        [ProtoMember(5)]
        public double PermittedGap { get; }

        /// <summary>
        ///  Extend query length by provided extended by value.
        /// </summary>
        /// <param name="extendedBy">Query length to extend by.</param>
        /// <returns>New instance of <see cref="Coverage"/> class.</returns>
        public Coverage WithExtendedQueryLength(double extendedBy)
        {
            return new Coverage(BestPath, QueryLength + extendedBy, TrackLength, FingerprintLength, PermittedGap);
        }

        /// <inheritdoc cref="object.ToString"/>
        public override string ToString()
        {
            return
                "Coverage[" +
                $"TrackCoverageWithPermittedGapsLength={TrackCoverageWithPermittedGapsLength:0.00},TrackMatchStartsAt={TrackMatchStartsAt:0.00},TrackLength={TrackLength}," +
                $"QueryCoverageWithPermittedGapsLength={QueryCoverageWithPermittedGapsLength:0.00},QueryMatchStartsAt={QueryMatchStartsAt:0.00},QueryLength={QueryLength}]";
        }
    }
}
