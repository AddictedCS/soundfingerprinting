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
                var topMatches = hammingSimilarities.OrderBy(pair => pair.Value).Take(queryConfiguration.MaximumNumberOfTracksToReturnAsResult);
                List<ResultData> resultSet = topMatches.Select(match => new ResultData { Track = modelService.ReadTrackById(match.Key), Similarity = match.Value }).ToList();

                return new QueryResult
                    {
                        IsSuccessful = true,
                        TotalNumberOfAnalyzedCandidates = hammingSimilarities.Count,
                        Results = resultSet
                    };
            }

            return new QueryResult();
        }
    }
}
