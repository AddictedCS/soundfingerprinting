namespace SoundFingerprinting
{
    using System.Diagnostics;
    using System.Linq;
    using System.Text.RegularExpressions;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Command;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Query;

    /// <summary>
    ///  Query fingerprint service.
    /// </summary>
    /// <remarks>
    /// Please use <see cref="QueryCommandBuilder"/> for creating <see cref="QueryCommand"/> objects that will issue query to the provided <see cref="IModelService"/>.
    /// </remarks>
    public class QueryFingerprintService : IQueryFingerprintService
    {
        private readonly IQueryMath queryMath;

        private QueryFingerprintService(IQueryMath queryMath)
        {
            this.queryMath = queryMath; 
        }
        
        /// <summary>
        ///  Gets an instance of the <see cref="QueryFingerprintService"/> class.
        /// </summary>
        public static QueryFingerprintService Instance { get; } = new (QueryMath.Instance);

        /// <inheritdoc cref="IQueryFingerprintService.Query"/>
        public QueryResult Query(Hashes hashes, QueryConfiguration configuration, IModelService modelService)
        {
            var queryStopwatch = Stopwatch.StartNew();
            var groupedQueryResults = GetSimilaritiesUsingBatchedStrategy(hashes, configuration, modelService);
            if (!groupedQueryResults.ContainsMatches)
            {
                return QueryResult.Empty(hashes,  queryStopwatch.ElapsedMilliseconds);
            }

            var resultEntries = queryMath.GetBestCandidates(groupedQueryResults, configuration.MaxTracksToReturn, modelService, configuration);
            int totalTracksAnalyzed = groupedQueryResults.TracksCount;
            int totalSubFingerprintsAnalyzed = groupedQueryResults.SubFingerprintsCount;
            return QueryResult.NonEmptyResult(resultEntries, hashes, totalTracksAnalyzed, totalSubFingerprintsAnalyzed,  queryStopwatch.ElapsedMilliseconds);
        }

        private static GroupedQueryResults GetSimilaritiesUsingBatchedStrategy(Hashes queryHashes, QueryConfiguration configuration, IModelService modelService)
        {
            var candidates = modelService.QueryEfficiently(queryHashes, configuration);
            var groupedResults = new GroupedQueryResults(queryHashes.DurationInSeconds, queryHashes.RelativeTo);
            foreach (var track in candidates.GetMatchedTracks())
            {
                var matches = candidates.GetMatchesForTrack(track);
                foreach (var match in matches)
                {
                    groupedResults.Add(match.QuerySequenceNumber, track, match);
                }
            }

            return groupedResults;
        }
    }
}