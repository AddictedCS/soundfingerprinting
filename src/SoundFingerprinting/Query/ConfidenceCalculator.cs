namespace SoundFingerprinting.Query
{
    using SoundFingerprinting.LCS;
    using static System.Math;

    public static class ConfidenceCalculator
    {
        public static double CalculateConfidence(Coverage coverage)
        {
            return CalculateConfidence(
                coverage.QueryMatchStartsAt, 
                coverage.QueryLength, 
                coverage.TrackMatchStartsAt, 
                coverage.TrackLength, 
                coverage.TrackCoverageWithPermittedGapsLength, 
                coverage.QueryDiscreteCoverageLength, 
                coverage.TrackDiscreteCoverageLength);
        }

        /// <summary>
        ///     Calculates how confident is the algorithm that it found a successful match
        ///     Source - query file, Origin - inserted item in the database
        /// </summary>
        /// <param name="queryMatchStartsAt">Source starts to match at this position</param>
        /// <param name="queryLength">Total length of the query</param>
        /// <param name="trackMatchStartsAt">Start position of the match in the resulting (origin) track, as returned from the datasource</param>
        /// <param name="trackLength">Length of the origin track as it was inserted in datasource</param>
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
