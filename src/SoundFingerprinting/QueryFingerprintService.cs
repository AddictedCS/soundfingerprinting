namespace SoundFingerprinting
{
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Math;
    using SoundFingerprinting.Query;

    public class QueryFingerprintService : IQueryFingerprintService
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
    
        public QueryResult Query(IModelService modelService, List<HashedFingerprint> hashedFingerprints, QueryConfiguration queryConfiguration)
        {
            var hammingSimilarities = new Dictionary<IModelReference, ResultEntryAccumulator>();
            int subFingerprintsCount = 0;
            foreach (var hashedFingerprint in hashedFingerprints)
            {
                var subFingerprints = modelService.ReadSubFingerprints(hashedFingerprint.HashBins, queryConfiguration);
                subFingerprintsCount += subFingerprints.Count;
                similarityUtility.AccumulateHammingSimilarity(subFingerprints, hashedFingerprint, hammingSimilarities);
            }

            if (!hammingSimilarities.Any())
            {
                return QueryResult.EmptyResult();
            }

            var resultEntries = queryMath.GetBestCandidates(hashedFingerprints, hammingSimilarities, queryConfiguration.MaxTracksToReturn, modelService, queryConfiguration.FingerprintConfiguration);
            return QueryResult.NonEmptyResult(resultEntries, subFingerprintsCount);
        }

        public QueryResult QueryExperimental(IModelService modelService, List<HashedFingerprint> hashedFingerprints, QueryConfiguration queryConfiguration)
        {
            var hammingSimilarities = new Dictionary<IModelReference, ResultEntryAccumulator>();
            var allCandidates = modelService.ReadSubFingerprints(hashedFingerprints.Select(h => h.HashBins), queryConfiguration);

            foreach (var hashedFingerprint in hashedFingerprints)
            {
                HashedFingerprint fingerprint = hashedFingerprint;
                var subFingerprints =
                    allCandidates.Where(
                        candidate => DoesMatchThresholdVotes(queryConfiguration, fingerprint, candidate));

                similarityUtility.AccumulateHammingSimilarity(subFingerprints, hashedFingerprint, hammingSimilarities);
            }

            if (!hammingSimilarities.Any())
            {
                return QueryResult.EmptyResult();
            }

            var resultEntries = queryMath.GetBestCandidates(
                hashedFingerprints,
                hammingSimilarities,
                queryConfiguration.MaxTracksToReturn,
                modelService,
                queryConfiguration.FingerprintConfiguration);
            return QueryResult.NonEmptyResult(resultEntries, allCandidates.Count);
        }

        private bool DoesMatchThresholdVotes(QueryConfiguration queryConfiguration, HashedFingerprint fingerprint, SubFingerprintData candidate)
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
