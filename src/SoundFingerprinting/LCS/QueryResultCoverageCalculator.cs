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

        public IEnumerable<Coverage> GetCoverages(TrackData trackData, GroupedQueryResults groupedQueryResults, QueryConfiguration configuration)
        {
            var fingerprintConfiguration = configuration.FingerprintConfiguration;
            
            // TODO this is redundant as matches are not required to be ordered down the line
            // TODO simplify GroupedQueryResults class
            var matches = groupedQueryResults.GetMatchesForTrack(trackData.TrackReference);

            double queryLength = groupedQueryResults.GetQueryLength();

            if (configuration.AllowMultipleMatchesOfTheSameTrackInQuery)
            {
                var sequences = longestIncreasingTrackSequence.FindAllIncreasingTrackSequences(matches, configuration.PermittedGap);
                var filtered = OverlappingRegionFilter.FilterOverlappingSequences(sequences, configuration.PermittedGap);
                return filtered.Select(matchedSequence => GetCoverage(matchedSequence, queryLength, fingerprintConfiguration.FingerprintLengthInSeconds));
            }

            return new List<Coverage>
                   {
                       GetCoverage(matches, queryLength, fingerprintConfiguration.FingerprintLengthInSeconds)
                   };
        }

        public Coverage GetCoverage(IEnumerable<MatchedWith> matches, double queryLength, double fingerprintLengthIsSeconds)
        {
            var orderedByResultAt = matches.OrderBy(with => with.ResultAt)
                                           .ToList();

            var trackRegion = CoverageEstimator.EstimateTrackCoverage(orderedByResultAt, queryLength, fingerprintLengthIsSeconds);

            var notCovered = GetNotCoveredLength(orderedByResultAt, trackRegion, fingerprintLengthIsSeconds, out var bestMatch);

            // optimistic coverage length
            double sourceCoverageLength = SubFingerprintsToSeconds.AdjustLengthToSeconds(orderedByResultAt[trackRegion.EndAt].ResultAt, orderedByResultAt[trackRegion.StartAt].ResultAt, fingerprintLengthIsSeconds);

            double queryMatchLength = sourceCoverageLength - notCovered; // exact length of matched fingerprints

            double queryMatchStartsAt = orderedByResultAt[trackRegion.StartAt].QueryAt;
            double trackMatchStartsAt = orderedByResultAt[trackRegion.StartAt].ResultAt;

            return new Coverage(queryMatchStartsAt, queryMatchLength, sourceCoverageLength, trackMatchStartsAt, GetTrackStartsAt(bestMatch), queryLength);
        }

        private static double GetNotCoveredLength(IReadOnlyList<MatchedWith> orderedByResultAt, TrackRegion trackRegion, double fingerprintLengthInSeconds, out MatchedWith bestMatch)
        {
            double notCovered = 0d;
            bestMatch = orderedByResultAt[trackRegion.StartAt];
            for (int i = trackRegion.StartAt + 1; i <= trackRegion.EndAt; ++i)
            {
                if (orderedByResultAt[i].ResultAt - orderedByResultAt[i - 1].ResultAt > fingerprintLengthInSeconds)
                {
                    notCovered += orderedByResultAt[i].ResultAt - (orderedByResultAt[i - 1].ResultAt + fingerprintLengthInSeconds);
                }

                if (bestMatch.HammingSimilarity < orderedByResultAt[i].HammingSimilarity)
                {
                    bestMatch = orderedByResultAt[i];
                }
            }

            return notCovered;
        }

        private static double GetTrackStartsAt(MatchedWith bestMatch)
        {
            return bestMatch.QueryAt - bestMatch.ResultAt;
        }
    }
}
