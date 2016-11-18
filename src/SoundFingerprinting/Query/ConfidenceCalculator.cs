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
        /// <param name="queryLength">Total length of the query</param>
        /// <param name="originStartsAt">Start position of the match in the resulting (origin) track, as returned from the datasource</param>
        /// <param name="originLength">Length of the origin track as it was inserted in datasource</param>
        /// <returns>Confidence level [0, 1)</returns>
        public double CalculateConfidence(
            double sourceMatchStartsAt,
            double sourceMatchLength,
            double queryLength,
            double originStartsAt,
            double originLength)
        {
            if (NeedleInHaystack(queryLength, originLength))
            {
                return
                    Ceil(
                        GetConfidenceForSmallSnippetFoundInLongQuery(
                            sourceMatchStartsAt, sourceMatchLength, queryLength, originStartsAt, originLength));
            }

            return
                Ceil(
                    GetConfidenceForSmallSnippetFoundInLongOrigin(
                        sourceMatchStartsAt, sourceMatchLength, queryLength, originStartsAt, originLength));
        }

        private static double Ceil(double confidence)
        {
            if (confidence > 1d)
            {
                return 1d;
            }

            return confidence;
        }

        private double GetConfidenceForSmallSnippetFoundInLongOrigin(double sourceMatchStartsAt, double sourceMatchLength, double queryLength, double originStartsAt, double originLength)
        {
            if (QueryClippedFromTheBegining(originStartsAt, originLength, queryLength))
            {
                return sourceMatchLength / (originLength - (originStartsAt - sourceMatchStartsAt));
            }

            if (QueryClippedFromTheEnd(sourceMatchStartsAt, originStartsAt))
            {
                return sourceMatchLength / (queryLength - sourceMatchStartsAt + originStartsAt);
            }

            return sourceMatchLength / queryLength;
        }

        private bool QueryClippedFromTheEnd(double sourceMatchStartsAt, double originStartsAt)
        {
            return sourceMatchStartsAt > originStartsAt;
        }

        private bool QueryClippedFromTheBegining(double originStartsAt, double originLength, double queryLength)
        {
            return originStartsAt + queryLength > originLength;
        }

        private double GetConfidenceForSmallSnippetFoundInLongQuery(
            double sourceMatchStartsAt, double sourceMatchLength, double queryLength, double originStartsAt, double originLength)
        {
            if (this.OriginTrackIsClippedFromTheBegining(sourceMatchStartsAt, originStartsAt))
            {
                return sourceMatchLength / (originLength - (originStartsAt - sourceMatchStartsAt));
            }

            if (this.OriginTrackIsClippedAtTheEnd(sourceMatchStartsAt, queryLength, originLength))
            {
                return sourceMatchLength / (queryLength - sourceMatchStartsAt + originStartsAt);
            }

            return sourceMatchLength / originLength;
        }

        private bool NeedleInHaystack(double queryLength, double originLength)
        {
            return queryLength > originLength;
        }

        private bool OriginTrackIsClippedAtTheEnd(double sourceMatchStartsAt, double queryLength, double originLength)
        {
            return sourceMatchStartsAt + originLength > queryLength;
        }

        private bool OriginTrackIsClippedFromTheBegining(double sourceMatchStartsAt, double originStartsAt)
        {
            return originStartsAt > sourceMatchStartsAt;
        }
    }
}
