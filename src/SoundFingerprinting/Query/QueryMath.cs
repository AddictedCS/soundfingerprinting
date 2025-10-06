namespace SoundFingerprinting.Query
{
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
            return new ResultEntry(track, groupedQueryResults.GetScoreSumForTrack(track.TrackReference), groupedQueryResults.RelativeTo.AddSeconds(coverage.QueryMatchStartsAt), coverage);
        }
    }
}
