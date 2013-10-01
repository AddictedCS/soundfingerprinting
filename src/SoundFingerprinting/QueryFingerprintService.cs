namespace SoundFingerprinting
{
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Dao;
    using SoundFingerprinting.Hashing;
    using SoundFingerprinting.Hashing.Utils;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Query;
    using SoundFingerprinting.Query.Configuration;

    public class QueryFingerprintService : IQueryFingerprintService
    {
        private readonly ICombinedHashingAlgoritm hashingAlgorithm;
        private readonly IModelService modelService;

        public QueryFingerprintService()
            : this(DependencyResolver.Current.Get<ICombinedHashingAlgoritm>(), DependencyResolver.Current.Get<IModelService>())
        {
        }

        public QueryFingerprintService(ICombinedHashingAlgoritm hashingAlgorithm, IModelService modelService)
        {
            this.hashingAlgorithm = hashingAlgorithm;
            this.modelService = modelService;
        }

        public QueryResult Query(IEnumerable<bool[]> fingerprints, IQueryConfiguration queryConfiguration)
        {
            Dictionary<int, int> hammingSimilarities = new Dictionary<int, int>();
            foreach (var fingerprint in fingerprints)
            {
                var tuple = hashingAlgorithm.Hash(fingerprint, queryConfiguration.NumberOfLSHTables, queryConfiguration.NumberOfMinHashesPerTable);
                var subFingerprints = modelService.ReadSubFingerprintsByHashBucketsHavingThreshold(tuple.Item2, queryConfiguration.ThresholdVotes);
                foreach (var subFingerprint in subFingerprints)
                {
                    int similarity = HashingUtils.CalculateHammingSimilarity(tuple.Item1, subFingerprint.Item1.Signature);
                    if (hammingSimilarities.ContainsKey(subFingerprint.Item1.TrackId))
                    {
                        hammingSimilarities[subFingerprint.Item1.TrackId] += similarity;
                    }
                    else
                    {
                        hammingSimilarities.Add(subFingerprint.Item1.TrackId, similarity);
                    }
                }
            }

            if (hammingSimilarities.Any())
            {
                var bestMatch = hammingSimilarities.Aggregate((l, r) => l.Value > r.Value ? l : r);
                return new QueryResult
                           {
                               BestMatch = modelService.ReadTrackById(bestMatch.Key),
                               IsSuccessful = true,
                               Similarity = bestMatch.Value,
                               NumberOfCandidates = hammingSimilarities.Count
                           };
            }

            return new QueryResult();
        }
    }
}
