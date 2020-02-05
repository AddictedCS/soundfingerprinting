namespace SoundFingerprinting.Query
{
    public interface IConfidenceCalculator
    {
        /// <summary>
        ///     Calculates how confident is the algorithm that it found a successful match
        /// </summary>
        /// <param name="queryMatchStartsAt">Original source starts to match at this position</param>
        /// <param name="queryLength">Total sourceMatchLength of the original source</param>
        /// <param name="trackMatchStartsAt">Start position of the match in the resulting track, as returned from the datasource</param>
        /// <param name="trackLength">Length of the result track as insertedd in datasource</param>
        /// <param name="coverageWithPermittedGapsLength">Length of the match (including permitted gaps) in the track.</param>
        /// <param name="queryDiscreteCoverageLength">Length of the match (including gaps) in the query</param>
        /// <param name="trackDiscreteCoverageLength">Length of the match (including gaps) in the track</param>
        /// <returns>Confidence level [0, 1)</returns>
        double CalculateConfidence(
            double queryMatchStartsAt,
            double queryLength,
            double trackMatchStartsAt,
            double trackLength,
            double coverageWithPermittedGapsLength,
            double queryDiscreteCoverageLength,
            double trackDiscreteCoverageLength);
    }
}