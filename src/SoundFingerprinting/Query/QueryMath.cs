namespace SoundFingerprinting.Query
{
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.LCS;

    public class QueryMath : IQueryMath
    {
        private readonly IQueryResultCoverageCalculator queryResultCoverageCalculator;
        private readonly IConfidenceCalculator confidenceCalculator;

        internal QueryMath(IQueryResultCoverageCalculator queryResultCoverageCalculator, IConfidenceCalculator confidenceCalculator)
        {
            this.queryResultCoverageCalculator = queryResultCoverageCalculator;
            this.confidenceCalculator = confidenceCalculator;
        }

        public static QueryMath Instance { get; } = new QueryMath(new QueryResultCoverageCalculator(new LongestIncreasingTrackSequence()), new ConfidenceCalculator());

        public List<ResultEntry> GetBestCandidates(GroupedQueryResults groupedQueryResults, int maxNumberOfMatchesToReturn, IModelService modelService, QueryConfiguration queryConfiguration)
        {
            var trackIds = groupedQueryResults.GetTopTracksByScore(maxNumberOfMatchesToReturn).ToList();
            var tracks = modelService.ReadTracksByReferences(trackIds);
            return tracks.SelectMany(track => BuildResultEntries(track, groupedQueryResults, queryConfiguration))
                .OrderByDescending(entry => entry.Score)
                .ToList();
        }

        public static bool IsCandidatePassingThresholdVotes(int[] query, int[] result, int thresholdVotes)
        {
            int count = 0;
            for (int i = 0; i < query.Length; ++i)
            {
                if (query[i] == result[i])
                {
                    count++;
                }

                if (count >= thresholdVotes)
                {
                    return true;
                }
            }

            return false;
        }

        private IEnumerable<ResultEntry> BuildResultEntries(TrackData track, GroupedQueryResults groupedQueryResults, QueryConfiguration configuration)
        {
            var coverages = queryResultCoverageCalculator.GetCoverages(track, groupedQueryResults, configuration);
            return coverages.Select(coverage =>
               {
                    double confidence = confidenceCalculator.CalculateConfidence(
                        coverage.QueryMatchStartsAt,
                        coverage.CoverageLength,
                        coverage.QueryLength,
                        coverage.TrackMatchStartsAt,
                        track.Length);

                    return new ExtendedResultEntry(
                        track,
                        coverage,
                        confidence,
                        groupedQueryResults.GetScoreSumForTrack(track.TrackReference),
                        groupedQueryResults.RelativeTo.AddSeconds(coverage.QueryMatchStartsAt));
               });
        }
    }
}
