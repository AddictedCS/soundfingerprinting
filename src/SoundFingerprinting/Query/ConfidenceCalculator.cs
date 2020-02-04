namespace SoundFingerprinting.Query
{
    using static System.Math;

    public class ConfidenceCalculator : IConfidenceCalculator
    {
        /// <summary>
        ///     Calculates how confident is the algorithm that it found a successful match
        ///     Source - query file, Origin - inserted item in the database
        /// </summary>
        /// <param name="queryMatchStartsAt">Source starts to match at this position</param>
        /// <param name="coverageLength">Length of the match in the source</param>
        /// <param name="queryLength">Total length of the query</param>
        /// <param name="trackMatchStartsAt">Start position of the match in the resulting (origin) track, as returned from the datasource</param>
        /// <param name="trackLength">Length of the origin track as it was inserted in datasource</param>
        /// <returns>Confidence level [0, 1)</returns>
        public double CalculateConfidence(
            double queryMatchStartsAt,
            double coverageLength,
            double queryLength,
            double trackMatchStartsAt,
            double trackLength)
        {
            var queryHead = queryMatchStartsAt;
            var queryTail = queryLength - (queryHead + coverageLength);

            var trackHead = trackMatchStartsAt;
            var trackTail = trackLength - (trackHead + coverageLength);

            var maxPossibleCoverageLength = Min(queryHead, trackHead) + coverageLength + Min(queryTail, trackTail);

            var confidence = coverageLength / maxPossibleCoverageLength;

            // TODO: check the arguments or clip the result to [0, 1].
            return confidence;
        }
    }
}
