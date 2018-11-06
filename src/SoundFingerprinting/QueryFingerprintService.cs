namespace SoundFingerprinting
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.LCS;
    using SoundFingerprinting.Math;
    using SoundFingerprinting.Query;

    public class QueryFingerprintService : IQueryFingerprintService
    {
        private readonly ISimilarityUtility similarityUtility;
        private readonly IQueryMath queryMath;

        private QueryFingerprintService(ISimilarityUtility similarityUtility, IQueryMath queryMath)
        {
            this.similarityUtility = similarityUtility;
            this.queryMath = queryMath;
        }

        public static QueryFingerprintService Instance { get; } = new QueryFingerprintService(
            new SimilarityUtility(),
            new QueryMath(
                new QueryResultCoverageCalculator(new LongestIncreasingTrackSequence()),
                new ConfidenceCalculator()));
    
        public QueryResult Query(IEnumerable<HashedFingerprint> queryFingerprints, QueryConfiguration configuration, IModelService modelService)
        {
            var groupedQueryResults = GetSimilaritiesUsingBatchedStrategy(queryFingerprints, configuration, modelService);

            if (!groupedQueryResults.ContainsMatches)
            {
                return QueryResult.EmptyResult();
            }

            var resultEntries = queryMath.GetBestCandidates(groupedQueryResults, configuration.MaxTracksToReturn, modelService, configuration);
            int totalTracksAnalyzed = groupedQueryResults.TracksCount;
            int totalSubFingerprintsAnalyzed = groupedQueryResults.SubFingerprintsCount; 
            return QueryResult.NonEmptyResult(resultEntries, totalTracksAnalyzed, totalSubFingerprintsAnalyzed);
        }

        private GroupedQueryResults GetSimilaritiesUsingBatchedStrategy(IEnumerable<HashedFingerprint> queryFingerprints, QueryConfiguration configuration, IModelService modelService)
        {
            var hashedFingerprints = queryFingerprints as List<HashedFingerprint> ?? queryFingerprints.ToList();
            var result = modelService.ReadSubFingerprints(hashedFingerprints.Select(hashedFingerprint => hashedFingerprint.HashBins), configuration);
            var groupedResults = new GroupedQueryResults(hashedFingerprints);
            int hashesPerTable = configuration.FingerprintConfiguration.HashingConfig.NumberOfMinHashesPerTable;
            Parallel.ForEach(hashedFingerprints, queryFingerprint =>
            {
                var subFingerprints = result.Where(queryResult => QueryMath.IsCandidatePassingThresholdVotes(queryFingerprint.HashBins, queryResult.Hashes, configuration.ThresholdVotes));
                foreach (var subFingerprint in subFingerprints)
                {
                    int hammingSimilarity = similarityUtility.CalculateHammingSimilarity(queryFingerprint.HashBins, subFingerprint.Hashes, hashesPerTable);
                    groupedResults.Add(queryFingerprint, subFingerprint, hammingSimilarity);
                }
            });

            return groupedResults;
        }
    }
}
