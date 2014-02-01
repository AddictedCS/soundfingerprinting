namespace SoundFingerprinting
{
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Dao;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Math;
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
            var hammingSimilarities = new Dictionary<IModelReference, int>();
            foreach (var hash in hashes)
            {
                var subFingerprints = GetSubFingerprints(hash, queryConfiguration);
                foreach (var subFingerprint in subFingerprints)
                {
                    int similarity = SimilarityUtility.CalculateHammingSimilarity(hash.SubFingerprint, subFingerprint.Signature);
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

        private IEnumerable<SubFingerprintData> GetSubFingerprints(HashData hash, IQueryConfiguration queryConfiguration)
        {
            if (!string.IsNullOrEmpty(queryConfiguration.TrackGroupId))
            {
                return modelService.ReadSubFingerprintDataByHashBucketsThresholdWithGroupId(
                    hash.HashBins, queryConfiguration.ThresholdVotes, queryConfiguration.TrackGroupId);
            }

            return modelService.ReadSubFingerprintDataByHashBucketsWithThreshold(
                hash.HashBins, queryConfiguration.ThresholdVotes);
        }
    }
}
