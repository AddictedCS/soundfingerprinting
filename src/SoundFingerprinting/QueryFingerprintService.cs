namespace SoundFingerprinting
{
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Dao;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Hashing.Utils;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Query;

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
            Dictionary<IModelReference, int> hammingSimilarities = new Dictionary<IModelReference, int>();
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
                var topMatches = hammingSimilarities.OrderByDescending(pair => pair.Value).Take(queryConfiguration.MaximumNumberOfTracksToReturnAsResult);
                var resultSet = topMatches.Select(match => new ResultEntry { Track = modelService.ReadTrackByReference(match.Key), Similarity = match.Value }).ToList();

                return new QueryResult
                           {
                               ResultEntries = resultSet,
                               IsSuccessful = true,
                               AnalyzedCandidatesCount = hammingSimilarities.Count
                           };
            }

            return new QueryResult
                {
                    ResultEntries = Enumerable.Empty<ResultEntry>().ToList(),
                    IsSuccessful = false,
                    AnalyzedCandidatesCount = 0
                };
        }
    }
}
