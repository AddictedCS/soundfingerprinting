namespace SoundFingerprinting.LCS
{
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Query;

    internal class QueryResultCoverageCalculator : IQueryResultCoverageCalculator
    {
        public Coverage GetLongestMatch(SortedSet<MatchedPair> matches, double queryLength, double oneFingerprintCoverage)
        {
            int minI = 0, maxI = 0, curMinI = 0, maxLength = 0;
            var sortedMatches = matches.ToList();
            for (int i = 1; i < sortedMatches.Count; ++i)
            {
                if (ConsecutiveResponseMatchesAreLongerThanQuery(queryLength, sortedMatches, i))
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
                if (sortedMatches[i].SubFingerprint.SequenceAt - sortedMatches[i - 1].SubFingerprint.SequenceAt > oneFingerprintCoverage)
                {
                    notCovered += sortedMatches[i].SubFingerprint.SequenceAt - (sortedMatches[i - 1].SubFingerprint.SequenceAt + oneFingerprintCoverage);
                }
            }

            double sourceMatchLength = sortedMatches[maxI].SubFingerprint.SequenceAt - sortedMatches[minI].SubFingerprint.SequenceAt - notCovered;
            double sourceMatchStartsAt = sortedMatches[minI].HashedFingerprint.StartsAt;
            double originMatchStartsAt = sortedMatches[minI].SubFingerprint.SequenceAt;
            return new Coverage(sourceMatchStartsAt, sourceMatchLength, originMatchStartsAt);
        }

        private bool ConsecutiveResponseMatchesAreLongerThanQuery(double queryLength, List<MatchedPair> sortedMatches, int i)
        {
            return sortedMatches[i].SubFingerprint.SequenceAt - sortedMatches[i - 1].SubFingerprint.SequenceAt >= queryLength;
        }
    }
}
