namespace SoundFingerprinting
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Math;
    using SoundFingerprinting.Query;

    internal class QueryFingerprintService : IQueryFingerprintService
    {
        private readonly ISimilarityUtility similarityUtility;
        private readonly IQueryMath queryMath;

        public QueryFingerprintService()
            : this(
                DependencyResolver.Current.Get<ISimilarityUtility>(),
                DependencyResolver.Current.Get<IQueryMath>())
        {
        }

        internal QueryFingerprintService(ISimilarityUtility similarityUtility, IQueryMath queryMath)
        {
            this.similarityUtility = similarityUtility;
            this.queryMath = queryMath;
        }
    
        public QueryResult Query(List<HashedFingerprint> queryFingerprints, QueryConfiguration configuration, IModelService modelService)
        {
            if (modelService.SupportsBatchedSubFingerprintQuery)
            {
                return GetResultsViaBatchQuery(queryFingerprints, configuration, modelService);
            }

            var hammingSimilarities = new ConcurrentDictionary<IModelReference, ResultEntryAccumulator>();
            Parallel.ForEach(queryFingerprints, queryFingerprint =>
            {
                var subFingerprints = modelService.ReadSubFingerprints(queryFingerprint.HashBins, configuration);
                similarityUtility.AccumulateHammingSimilarity(subFingerprints, queryFingerprint, hammingSimilarities);
            });

            if (!hammingSimilarities.Any())
            {
                return QueryResult.EmptyResult();
            }

            var resultEntries = queryMath.GetBestCandidates(queryFingerprints, hammingSimilarities, configuration.MaxTracksToReturn, modelService, configuration.FingerprintConfiguration);
            return QueryResult.NonEmptyResult(resultEntries);
        }

        private QueryResult GetResultsViaBatchQuery(IEnumerable<HashedFingerprint> queryFingerprints, QueryConfiguration configuration, IModelService modelService)
        {
            var hashedFingerprints = queryFingerprints as List<HashedFingerprint> ?? queryFingerprints.ToList();
            var allCandidates = modelService.ReadSubFingerprints(hashedFingerprints.Select(querySubfingerprint => querySubfingerprint.HashBins), configuration);
            var hammingSimilarities = new ConcurrentDictionary<IModelReference, ResultEntryAccumulator>();
            foreach (var hashedFingerprint in hashedFingerprints)
            {
                HashedFingerprint fingerprint = hashedFingerprint;
                var subFingerprints = allCandidates.Where(candidate => DoesMatchThisExactCandidate(configuration, fingerprint, candidate));
                similarityUtility.AccumulateHammingSimilarity(subFingerprints, hashedFingerprint, hammingSimilarities);
            }

            if (!hammingSimilarities.Any())
            {
                return QueryResult.EmptyResult();
            }

            var resultEntries = queryMath.GetBestCandidates(
                hashedFingerprints.ToList(),
                hammingSimilarities,
                configuration.MaxTracksToReturn,
                modelService,
                configuration.FingerprintConfiguration);
            return QueryResult.NonEmptyResult(resultEntries);
        }

        private bool DoesMatchThisExactCandidate(QueryConfiguration queryConfiguration, HashedFingerprint fingerprint, SubFingerprintData candidate)
        {
            long[] actual = fingerprint.HashBins;
            var result = candidate.Hashes;
            int count = 0;
            for (int i = 0; i < actual.Length; ++i)
            {
                if (actual[i] == result[i])
                {
                    count++;
                }

                if (count >= queryConfiguration.ThresholdVotes)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
