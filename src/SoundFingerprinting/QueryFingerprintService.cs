namespace SoundFingerprinting
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Math;
    using SoundFingerprinting.Query;

    public class QueryFingerprintService : IQueryFingerprintService
    {
        private readonly IScoreAlgorithm scoreAlgorithm;
        private readonly IQueryMath queryMath;

        public QueryFingerprintService(IScoreAlgorithm scoreAlgorithm, IQueryMath queryMath)
        {
            this.scoreAlgorithm = scoreAlgorithm;
            this.queryMath = queryMath;
        }

        public static QueryFingerprintService Instance { get; } = new QueryFingerprintService(new HammingSimilarityScoreAlgorithm(new SimilarityUtility()), QueryMath.Instance);
    
        public QueryResult Query(IEnumerable<HashedFingerprint> queryFingerprints, QueryConfiguration configuration, IModelService modelService)
        {
            var groupedQueryResults = GetSimilaritiesUsingBatchedStrategy(queryFingerprints, configuration, modelService);

            if (!groupedQueryResults.ContainsMatches)
            {
                return QueryResult.Empty;
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
            double queryLength = hashedFingerprints.QueryLength(configuration.FingerprintConfiguration);
            var groupedResults = new GroupedQueryResults(queryLength, configuration.RelativeTo);
            int hashesPerTable = configuration.FingerprintConfiguration.HashingConfig.NumberOfMinHashesPerTable;
            Parallel.ForEach(hashedFingerprints, queryFingerprint =>
            {
                var subFingerprints = result.Where(queryResult => QueryMath.IsCandidatePassingThresholdVotes(queryFingerprint.HashBins, queryResult.Hashes, configuration.ThresholdVotes));
                foreach (var subFingerprint in subFingerprints)
                {
                    int score = scoreAlgorithm.GetScore(queryFingerprint, subFingerprint, configuration);
                    groupedResults.Add(queryFingerprint, subFingerprint, score);
                }
            });

            return groupedResults;
        }
    }
}
