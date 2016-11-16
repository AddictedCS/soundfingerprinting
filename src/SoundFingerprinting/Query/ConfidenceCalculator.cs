namespace SoundFingerprinting.Query
{
    internal class ConfidenceCalculator : IConfidenceCalculator
    {
        /// <summary>
        ///     Calculates how confident is the algorithm that it found a successful match
        ///     Source - query file, Origin - inserted item in the database
        /// </summary>
        /// <param name="sourceMatchStartsAt">Source starts to match at this position</param>
        /// <param name="sourceMatchLength">Length of the match in the source</param>
        /// <param name="sourceLength">Total length of the source</param>
        /// <param name="originStartsAt">Start position of the match in the resulting (origin) track, as returned from the datasource</param>
        /// <param name="originLength">Length of the origin track as it was inserted in datasource</param>
        /// <returns>Confidence level [0, 1)</returns>
        public double CalculateConfidence(
            double sourceMatchStartsAt,
            double sourceMatchLength,
            double sourceLength,
            double originStartsAt,
            double originLength)
        {
            if (OriginTrackIsClippedFromTheBegining(sourceMatchStartsAt, originStartsAt))
            {
                return sourceMatchLength / (originLength - (originStartsAt - sourceMatchStartsAt));
            }

            if (OriginTrackIsClippedAtTheEnd(sourceMatchStartsAt, sourceLength, originLength))
            {
                return sourceMatchLength / (sourceLength - sourceMatchStartsAt + originStartsAt);
            }

            return sourceMatchLength / originLength;
        }

        private static bool OriginTrackIsClippedAtTheEnd(double sourceMatchStartsAt, double sourceLength, double originLength)
        {
            return sourceMatchStartsAt + originLength > sourceLength;
        }

        private static bool OriginTrackIsClippedFromTheBegining(double sourceMatchStartsAt, double originStartsAt)
        {
            return originStartsAt > sourceMatchStartsAt;
        }
    }
}
