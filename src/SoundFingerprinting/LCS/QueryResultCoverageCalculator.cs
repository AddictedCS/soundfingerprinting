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
            var matches = groupedQueryResults.GetOrderedMatchesForTrack(trackData.TrackReference);
            var sequences = longestIncreasingTrackSequence.FindAllIncreasingTrackSequences(matches);
            var filtered = FilterOverlappingSequences(sequences);
            return filtered.Select(matchedSequence => GetCoverage(matchedSequence, configuration));
        }

        private List<MatchedWith[]> FilterOverlappingSequences(List<MatchedWith[]> sequences)
        {
            // TODO is this possible?
            return null;
        }

        public Coverage GetCoverage(MatchedWith[] sortedMatches, FingerprintConfiguration configuration)
        {
            double notCovered = 0d;
            for (int i = 1; i < sortedMatches.Length; ++i)
            {
                if (sortedMatches[i].ResultAt - sortedMatches[i - 1].ResultAt > configuration.FingerprintLengthInSeconds)
                {
                    notCovered += sortedMatches[i].ResultAt - (sortedMatches[i - 1].ResultAt + configuration.FingerprintLengthInSeconds);
                }
            }

            double sourceMatchLength = SubFingerprintsToSeconds.AdjustLengthToSeconds(
                    sortedMatches[sortedMatches.Length - 1].ResultAt,
                    sortedMatches[0].ResultAt,
                    configuration) - notCovered;

            double sourceMatchStartsAt = sortedMatches[0].QueryAt;
            double originMatchStartsAt = sortedMatches[0].ResultAt;
            return new Coverage(sourceMatchStartsAt, sourceMatchLength, originMatchStartsAt);
        }
    }
}
