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

    internal class QueryFingerprintService : IQueryFingerprintService
    {
        private readonly ISimilarityUtility similarityUtility;
        private readonly IQueryMath queryMath;

        private static readonly QueryFingerprintService Singleton = new QueryFingerprintService(new SimilarityUtility(),
            new QueryMath(new QueryResultCoverageCalculator(new LongestIncreasingTrackSequence()), new ConfidenceCalculator()));

        public static QueryFingerprintService Instance
        {
            get
            {
                return Singleton;
            }
        }

        internal QueryFingerprintService(ISimilarityUtility similarityUtility, IQueryMath queryMath)
        {
            this.similarityUtility = similarityUtility;
            this.queryMath = queryMath;
        }
    
        public QueryResult Query(List<HashedFingerprint> queryFingerprints, QueryConfiguration configuration, IModelService modelService)
        {
            GroupedQueryResults groupedQueryResults;
            if (modelService.SupportsBatchedSubFingerprintQuery)
            {
                groupedQueryResults = GetSimilaritiesUsingBatchedStrategy(queryFingerprints, configuration, modelService);
            }
            else
            {
                groupedQueryResults = GetSimilaritiesUsingNonBatchedStrategy(queryFingerprints, configuration, modelService);
            }

            if (!groupedQueryResults.ContainsMatches)
            {
                return QueryResult.EmptyResult();
            }

            var resultEntries = queryMath.GetBestCandidates(groupedQueryResults, configuration.MaxTracksToReturn, modelService, configuration);
            int totalTracksAnalyzed = groupedQueryResults.TracksCount;
            int totalSubFingerprintsAnalyzed = groupedQueryResults.SubFingerprintsCount; 
            return QueryResult.NonEmptyResult(resultEntries, totalTracksAnalyzed, totalSubFingerprintsAnalyzed);
        }

        private GroupedQueryResults GetSimilaritiesUsingNonBatchedStrategy(IEnumerable<HashedFingerprint> queryFingerprints, QueryConfiguration configuration, IModelService modelService)
        {
            var hashedFingerprints = queryFingerprints.ToList();
            var groupedResults = new GroupedQueryResults(hashedFingerprints);
            int hashesPerTable = configuration.FingerprintConfiguration.HashingConfig.NumberOfMinHashesPerTable;
            Parallel.ForEach(hashedFingerprints, queryFingerprint => 
            { 
                var subFingerprints = modelService.ReadSubFingerprints(queryFingerprint.HashBins, configuration);
                foreach (var subFingerprint in subFingerprints)
                {
                    int hammingSimilarity = similarityUtility.CalculateHammingSimilarity(queryFingerprint.HashBins, subFingerprint.Hashes, hashesPerTable);
                    groupedResults.Add(queryFingerprint, subFingerprint, hammingSimilarity);
                }
            });

            return groupedResults;
        }

        private GroupedQueryResults GetSimilaritiesUsingBatchedStrategy(IEnumerable<HashedFingerprint> queryFingerprints, QueryConfiguration configuration, IModelService modelService)
        {
            var hashedFingerprints = queryFingerprints as List<HashedFingerprint> ?? queryFingerprints.ToList();
            var allCandidates = modelService.ReadSubFingerprints(hashedFingerprints.Select(querySubfingerprint => querySubfingerprint.HashBins), configuration);
            var groupedResults = new GroupedQueryResults(hashedFingerprints);
            int hashesPerTable = configuration.FingerprintConfiguration.HashingConfig.NumberOfMinHashesPerTable;
            Parallel.ForEach(hashedFingerprints, queryFingerprint => 
            {
                var subFingerprints = allCandidates.Where(candidate => queryMath.IsCandidatePassingThresholdVotes(queryFingerprint, candidate, configuration.ThresholdVotes));
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
