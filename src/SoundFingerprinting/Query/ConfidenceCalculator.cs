namespace SoundFingerprinting.Query
{
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
            if (QueryLongerThanTrack(queryLength, trackLength))
            {
                return
                    Ceil(
                        GetConfidenceForSmallSnippetFoundInLongQuery(
                            queryMatchStartsAt, coverageLength, queryLength, trackMatchStartsAt, trackLength));
            }

            return
                Ceil(
                    GetConfidenceForSmallSnippetFoundInLongTrack(
                        queryMatchStartsAt, coverageLength, queryLength, trackMatchStartsAt, trackLength));
        }

        private static double Ceil(double confidence)
        {
            if (confidence > 1d)
            {
                return 1d;
            }

            return confidence;
        }

        private double GetConfidenceForSmallSnippetFoundInLongTrack(
            double queryMatchStartsAt, double coverageLength, double queryLength, double trackMatchStartsAt, double trackLength)
        {
            if (QueryClippedFromTheBegining(trackMatchStartsAt, trackLength, queryLength))
            {
                return coverageLength / (trackLength - (trackMatchStartsAt - queryMatchStartsAt));
            }

            if (QueryClippedFromTheEnd(queryMatchStartsAt, trackMatchStartsAt))
            {
                return coverageLength / (queryLength - queryMatchStartsAt + trackMatchStartsAt);
            }

            return coverageLength / queryLength;
        }

        private bool QueryClippedFromTheEnd(double queryMatchStartsAt, double trackMatchStartsAt)
        {
            return queryMatchStartsAt > trackMatchStartsAt;
        }

        private bool QueryClippedFromTheBegining(double trackMatchStartsAt, double trackLength, double queryLength)
        {
            return trackMatchStartsAt + queryLength > trackLength;
        }

        private double GetConfidenceForSmallSnippetFoundInLongQuery(
            double queryMatchStartsAt, double coverageLength, double queryLength, double trackMatchStartsAt, double trackLength)
        {
            if (TrackIsClippedFromTheBegining(queryMatchStartsAt, trackMatchStartsAt))
            {
                return coverageLength / (trackLength - (trackMatchStartsAt - queryMatchStartsAt));
            }

            if (TrackIsClippedAtTheEnd(queryMatchStartsAt, queryLength, trackLength))
            {
                return coverageLength / (queryLength - queryMatchStartsAt + trackMatchStartsAt);
            }

            return coverageLength / trackLength;
        }

        private bool QueryLongerThanTrack(double queryLength, double trackLength)
        {
            return queryLength > trackLength;
        }

        private bool TrackIsClippedAtTheEnd(double queryMatchStartsAt, double queryLength, double trackLength)
        {
            return queryMatchStartsAt + trackLength > queryLength;
        }

        private bool TrackIsClippedFromTheBegining(double queryMatchStartsAt, double trackMatchStartsAt)
        {
            return trackMatchStartsAt > queryMatchStartsAt;
        }
    }
}
