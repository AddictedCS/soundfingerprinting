namespace SoundFingerprinting
{
    using System.Diagnostics;
    using System.Linq;
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

        private GroupedQueryResults GetSimilaritiesUsingBatchedStrategy(Hashes queryHashes, QueryConfiguration configuration, IModelService modelService)
        {
            var matchedSubFingerprints = modelService.Query(queryHashes, configuration);
            return queryHashes
                .AsParallel()
                .Aggregate(new GroupedQueryResults(queryHashes.DurationInSeconds, queryHashes.RelativeTo), (seed, queryFingerprint) =>
                {
                    var matched = matchedSubFingerprints.Where(queryResult => QueryMath.IsCandidatePassingThresholdVotes(queryFingerprint.HashBins, queryResult.Hashes, configuration.ThresholdVotes));
                    foreach (var subFingerprint in matched)
                    {
                        double score = configuration.ScoreAlgorithm.GetScore(queryFingerprint, subFingerprint, configuration);
                        seed.Add(queryFingerprint, subFingerprint, score);
                    }

                    return seed;
                });
        }
    }
}