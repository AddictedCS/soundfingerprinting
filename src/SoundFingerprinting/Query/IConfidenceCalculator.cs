namespace SoundFingerprinting.Query
{
    public interface IConfidenceCalculator
    {
        /// <summary>
        ///     Calculates how confident is the algorithm that it found a successful match
        /// </summary>
        /// <param name="queryMatchStartsAt">Original source starts to match at this position</param>
        /// <param name="coverageLength">Length of the match in the original source</param>
        /// <param name="queryLength">Total sourceMatchLength of the original source</param>
        /// <param name="trackMatchStartsAt">Start position of the match in the resulting track, as returned from the datasource</param>
        /// <param name="trackLength">Length of the result track as insertedd in datasource</param>
        /// <returns>Confidence level [0, 1)</returns>
        double CalculateConfidence(
            double queryMatchStartsAt,
            double coverageLength,
            double queryLength,
            double trackMatchStartsAt,
            double trackLength);
    }
}