namespace SoundFingerprinting.LCS
{
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Query;

    internal class QueryResultCoverageCalculator : IQueryResultCoverageCalculator
    {
        public Coverage GetCoverage(SortedSet<MatchedPair> matches, double queryLength, FingerprintConfiguration configuration)
        {
            int minI = 0, maxI = 0, curMinI = 0, maxLength = 0;
            var sortedMatches = matches.ToList();
            for (int i = 1; i < sortedMatches.Count; ++i)
            {
                if (ConsecutiveMatchesAreLongerThanTheQuery(queryLength, sortedMatches, i, configuration))
                {
                    // potentialy a new start of best matched sequence
                    curMinI = i;
                }

                if (i - curMinI > maxLength)
                {
                    maxLength = i - curMinI;
                    maxI = i;
                    minI = curMinI;
                }
            }

            double notCovered = 0d;
            for (int i = minI + 1; i <= maxI; ++i)
            {
                if (sortedMatches[i].SubFingerprint.SequenceAt - sortedMatches[i - 1].SubFingerprint.SequenceAt > configuration.FingerprintLengthInSeconds)
                {
                    notCovered += sortedMatches[i].SubFingerprint.SequenceAt - (sortedMatches[i - 1].SubFingerprint.SequenceAt + configuration.FingerprintLengthInSeconds);
                }
            }

            double sourceMatchLength = SubFingerprintsToSeconds.AdjustLengthToSeconds(
                    sortedMatches[maxI].SubFingerprint.SequenceAt,
                    sortedMatches[minI].SubFingerprint.SequenceAt,
                    configuration) - notCovered;

            double sourceMatchStartsAt = sortedMatches[minI].HashedFingerprint.StartsAt;
            double originMatchStartsAt = sortedMatches[minI].SubFingerprint.SequenceAt;
            return new Coverage(sourceMatchStartsAt, sourceMatchLength, originMatchStartsAt);
        }

        private bool ConsecutiveMatchesAreLongerThanTheQuery(double queryLength, List<MatchedPair> sortedMatches, int index, FingerprintConfiguration config)
        {
            return SubFingerprintsToSeconds.AdjustLengthToSeconds(
                sortedMatches[index].SubFingerprint.SequenceAt,
                sortedMatches[index - 1].SubFingerprint.SequenceAt,
                config) > queryLength;
        }
    }
}
