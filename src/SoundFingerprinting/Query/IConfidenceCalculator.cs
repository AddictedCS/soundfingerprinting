namespace SoundFingerprinting.Query
{
    internal interface IConfidenceCalculator
    {
        /// <summary>
        ///     Calculates how confident is the algorithm that it found a successful match
        /// </summary>
        /// <param name="sourceMatchStartsAt">Original source starts to match at this position</param>
        /// <param name="sourceMatchLength">Length of the match in the original source</param>
        /// <param name="queryLength">Total sourceMatchLength of the original source</param>
        /// <param name="originStartsAt">Start position of the match in the resulting track, as returned from the datasource</param>
        /// <param name="originLength">Length of the result track as insertedd in datasource</param>
        /// <returns>Confidence level [0, 1)</returns>
        double CalculateConfidence(
            double sourceMatchStartsAt,
            double sourceMatchLength,
            double queryLength,
            double originStartsAt,
            double originLength);
    }
}