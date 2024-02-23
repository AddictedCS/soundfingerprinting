namespace SoundFingerprinting
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Command;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
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
        public QueryResult Query(Hashes hashes, QueryConfiguration configuration, IQueryService queryService)
        {
            var queryStopwatch = Stopwatch.StartNew();
            var groupedQueryResults = GetSimilaritiesUsingBatchedStrategy(hashes, configuration, queryService);
            if (!groupedQueryResults.ContainsMatches)
            {
                return QueryResult.Empty(hashes,  queryStopwatch.ElapsedMilliseconds);
            }

            var resultEntries = queryMath.GetBestCandidates(groupedQueryResults, configuration.MaxTracksToReturn, queryService, configuration);
            int totalTracksAnalyzed = groupedQueryResults.TracksCount;
            int totalSubFingerprintsAnalyzed = groupedQueryResults.SubFingerprintsCount;
            return QueryResult.NonEmptyResult(resultEntries, hashes, totalTracksAnalyzed, totalSubFingerprintsAnalyzed,  queryStopwatch.ElapsedMilliseconds);
        }

        private static GroupedQueryResults GetSimilaritiesUsingBatchedStrategy(Hashes queryHashes, QueryConfiguration configuration, IQueryService queryService)
        {
            var candidates = queryService.QueryEfficiently(queryHashes, configuration);
            var groupedResults = new GroupedQueryResults(queryHashes.DurationInSeconds, queryHashes.RelativeTo);
            foreach (KeyValuePair<IModelReference, List<MatchedWith>> kv in candidates.GetMatches())
            {
                foreach (var match in kv.Value)
                {
                    groupedResults.Add(match.QuerySequenceNumber, kv.Key, match);
                }
            }

            return groupedResults;
        }
    }
}