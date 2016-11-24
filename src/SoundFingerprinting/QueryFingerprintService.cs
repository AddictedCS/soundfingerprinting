namespace SoundFingerprinting
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
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
    }
}
