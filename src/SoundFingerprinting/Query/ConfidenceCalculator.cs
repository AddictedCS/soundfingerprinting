namespace SoundFingerprinting.Query
{
    using static System.Math;

    internal static class ConfidenceCalculator
    {
        /// <summary>
        ///   Calculates how confident is the algorithm that it found a successful match.
        ///   Interpret calculated value as the probability of the query match to be a True Positive.
        /// </summary>
        /// <param name="queryMatchStartsAt">Query match starts at second.</param>
        /// <param name="queryLength">Query length.</param>
        /// <param name="trackMatchStartsAt">Track match starts at second.</param>
        /// <param name="trackLength">Track length</param>
        /// <param name="trackCoverageWithPermittedGapsLength">Length of the match (including permitted gaps) in the track.</param>
        /// <param name="queryDiscreteCoverageLength">Length of the match (including gaps) in the query</param>
        /// <param name="trackDiscreteCoverageLength">Length of the match (including gaps) in the track</param>
        /// <returns>Confidence level [0, 1)</returns>
        public static double CalculateConfidence(double queryMatchStartsAt,
            double queryLength,
            double trackMatchStartsAt,
            double trackLength,
            double trackCoverageWithPermittedGapsLength,
            double queryDiscreteCoverageLength,
            double trackDiscreteCoverageLength)
        {
            var queryHead = queryMatchStartsAt;
            var queryTail = queryLength - (queryHead + queryDiscreteCoverageLength);

            var trackHead = trackMatchStartsAt;
            var trackTail = trackLength - (trackHead + trackDiscreteCoverageLength);

            var maxPossibleCoverageLength = Min(queryHead, trackHead) + trackDiscreteCoverageLength + Min(queryTail, trackTail);
            return trackCoverageWithPermittedGapsLength / maxPossibleCoverageLength;
        }
    }
}
