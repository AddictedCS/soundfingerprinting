namespace SoundFingerprinting
{
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Dao;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Hashing.Utils;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Query;
    using SoundFingerprinting.Query.Configuration;

    public class QueryFingerprintService : IQueryFingerprintService
    {
        private readonly IModelService modelService;

        public QueryFingerprintService()
            : this(DependencyResolver.Current.Get<IModelService>())
        {
        }

        public QueryFingerprintService(IModelService modelService)
        {
            this.modelService = modelService;
        }

        public QueryResult Query(IEnumerable<HashData> hashes, IQueryConfiguration queryConfiguration)
        {
            Dictionary<ITrackReference, int> hammingSimilarities = new Dictionary<ITrackReference, int>();
            foreach (var hash in hashes)
            {
                var subFingerprints = modelService.ReadSubFingerprintDataByHashBucketsWithThreshold(hash.HashBins, queryConfiguration.ThresholdVotes);
                foreach (var subFingerprint in subFingerprints)
                {
                    int similarity = HashingUtils.CalculateHammingSimilarity(hash.SubFingerprint, subFingerprint.Signature);
                    if (hammingSimilarities.ContainsKey(subFingerprint.TrackReference))
                    {
                        hammingSimilarities[subFingerprint.TrackReference] += similarity;
                    }
                    else
                    {
                        hammingSimilarities.Add(subFingerprint.TrackReference, similarity);
                    }
                }
            }

            if (hammingSimilarities.Any())
            {
                var bestMatch = hammingSimilarities.Aggregate((l, r) => l.Value > r.Value ? l : r);
                return new QueryResult
                           {
                               BestMatch = modelService.ReadTrackByReference(bestMatch.Key),
                               IsSuccessful = true,
                               Similarity = bestMatch.Value,
                               NumberOfCandidates = hammingSimilarities.Count
                           };
            }

            return new QueryResult();
        }
    }
}
