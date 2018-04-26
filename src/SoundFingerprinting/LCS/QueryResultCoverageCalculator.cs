namespace SoundFingerprinting.LCS
{
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Query;

    internal class QueryResultCoverageCalculator : IQueryResultCoverageCalculator
    {
        private readonly ILongestIncreasingTrackSequence longestIncreasingTrackSequence;

        public QueryResultCoverageCalculator(ILongestIncreasingTrackSequence longestIncreasingTrackSequence)
        {
            this.longestIncreasingTrackSequence = longestIncreasingTrackSequence;
        }

        public IEnumerable<Coverage> GetCoverages(TrackData trackData, GroupedQueryResults groupedQueryResults, FingerprintConfiguration configuration)
        {
            var matches = groupedQueryResults.GetMatchesForTrackOrderedByQueryAt(trackData.TrackReference);
            /* if (false)
            {
                var sequences = longestIncreasingTrackSequence.FindAllIncreasingTrackSequences(matches);
                var filtered = OverlappingRegionFilter.FilterOverlappingSequences(sequences, trackData.Length);
                return filtered.Select(matchedSequence => GetCoverage(matchedSequence.OrderBy(m => m.ResultAt).ToList(), configuration));
            } */

            yield return GetCoverage(matches.OrderBy(with => with.ResultAt).ToList(), configuration);
        }

        private Coverage GetCoverage(List<MatchedWith> sortedMatches, FingerprintConfiguration configuration)
        {
            double notCovered = 0d;
            var bestMatch = sortedMatches[0];
            for (int i = 1; i < sortedMatches.Count; ++i)
            {
                if (sortedMatches[i].ResultAt - sortedMatches[i - 1].ResultAt > configuration.FingerprintLengthInSeconds)
                {
                    notCovered += sortedMatches[i].ResultAt - (sortedMatches[i - 1].ResultAt + configuration.FingerprintLengthInSeconds);
                }

                if (bestMatch.HammingSimilarity < sortedMatches[i].HammingSimilarity)
                {
                    bestMatch = sortedMatches[i];
                }
            }

            double sourceCoverageLength = SubFingerprintsToSeconds.AdjustLengthToSeconds(sortedMatches[sortedMatches.Count - 1].ResultAt,
                sortedMatches[0].ResultAt,
                configuration);

            double sourceMatchLength = sourceCoverageLength - notCovered;

            double sourceMatchStartsAt = sortedMatches[0].QueryAt;
            double originMatchStartsAt = sortedMatches[0].ResultAt;
            return new Coverage(sourceMatchStartsAt, sourceMatchLength, sourceCoverageLength, originMatchStartsAt, GetTrackStartsAt(bestMatch));
        }

        private double GetTrackStartsAt(MatchedWith bestMatch)
        {
            return bestMatch.QueryAt - bestMatch.ResultAt;
        }
    }
}
