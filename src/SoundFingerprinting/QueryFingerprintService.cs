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

        internal QueryFingerprintService(ISimilarityUtility similarityUtility, IQueryMath queryMath)
        {
            this.similarityUtility = similarityUtility;
            this.queryMath = queryMath;
        }

        public static QueryFingerprintService Instance { get; } = new QueryFingerprintService(
            new SimilarityUtility(),
            new QueryMath(
                new QueryResultCoverageCalculator(new LongestIncreasingTrackSequence()),
                new ConfidenceCalculator()));
    
        public QueryResult Query(List<HashedFingerprint> queryFingerprints, QueryConfiguration configuration, IModelService modelService)
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
            var result = modelService.ReadSubFingerprints(hashedFingerprints.Select(hashedFingerprint => new QueryHash(hashedFingerprint.HashBins, (int)hashedFingerprint.SequenceNumber)), configuration);
            var groupedResults = new GroupedQueryResults(hashedFingerprints);
            int hashesPerTable = configuration.FingerprintConfiguration.HashingConfig.NumberOfMinHashesPerTable;

            var allCandidates = result.Matches.ToList();

            var joined = from candidate in allCandidates
                         join hashedFingerprint in hashedFingerprints on candidate.QuerySequenceNumber equals (int)hashedFingerprint.SequenceNumber
                         select new { candidate.SubFingerprint, HashedFingerprint = hashedFingerprint };

            Parallel.ForEach(joined, pair =>
            {
                int hammingSimilarity = similarityUtility.CalculateHammingSimilarity(pair.HashedFingerprint.HashBins, pair.SubFingerprint.Hashes, hashesPerTable);
                groupedResults.Add(pair.HashedFingerprint, pair.SubFingerprint, hammingSimilarity);
            });

            return groupedResults;
        }
    }
}
