namespace SoundFingerprinting.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.LCS;

    internal class QueryMath : IQueryMath
    {
        private readonly IQueryResultCoverageCalculator queryResultCoverageCalculator;

        private QueryMath(IQueryResultCoverageCalculator queryResultCoverageCalculator)
        {
            this.queryResultCoverageCalculator = queryResultCoverageCalculator;
        }

        public static QueryMath Instance { get; } = new (new QueryResultCoverageCalculator());

        public List<ResultEntry> GetBestCandidates(GroupedQueryResults groupedQueryResults, int maxNumberOfMatchesToReturn, IQueryService modelService, QueryConfiguration queryConfiguration)
        {
            var trackIds = groupedQueryResults.GetTopTracksByScore(maxNumberOfMatchesToReturn).ToList();
            var tracks = modelService.ReadTracksByReferences(trackIds);
            return tracks
                .SelectMany(track => BuildResultEntries(track, groupedQueryResults, queryConfiguration))
                .OrderByDescending(entry => entry.Score)
                .ToList();
        }

        private IEnumerable<ResultEntry> BuildResultEntries(TrackData track, GroupedQueryResults groupedQueryResults, QueryConfiguration configuration)
        {
            var coverages = queryResultCoverageCalculator.GetCoverages(track, groupedQueryResults, configuration);
            return coverages
                .Where(coverage => configuration.TruePositivesFilter.IsTruePositive(coverage))
                .Select(coverage => GetResultEntry(track, groupedQueryResults, coverage));
        }

        private static ResultEntry GetResultEntry(TrackData track, GroupedQueryResults groupedQueryResults, Coverage coverage)
        {
            // QueryMatchStartsAt is a position inside the fingerprinted window, which starts TimeOffset seconds before RelativeTo (repeated-head prefix);
            // unstamped queries (RelativeTo == MinValue) skip the offset — MatchedAt is meaningless there, and MinValue cannot go below itself
            var matchedAt = groupedQueryResults.RelativeTo == DateTime.MinValue
                ? groupedQueryResults.RelativeTo.AddSeconds(coverage.QueryMatchStartsAt)
                : groupedQueryResults.RelativeTo.AddSeconds(coverage.QueryMatchStartsAt + groupedQueryResults.TimeOffset);
            return new ResultEntry(track, groupedQueryResults.GetScoreSumForTrack(track.TrackReference), matchedAt, coverage);
        }
    }
}
