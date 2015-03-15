namespace SoundFingerprinting
{
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.LCS;
    using SoundFingerprinting.Math;
    using SoundFingerprinting.Query;

    public class QueryFingerprintService : IQueryFingerprintService
    {
        private readonly IAudioSequencesAnalyzer audioSequencesAnalyzer;
        private readonly ISimilarityUtility similarityCalculationUtility;

        public QueryFingerprintService() : this(DependencyResolver.Current.Get<IAudioSequencesAnalyzer>(), DependencyResolver.Current.Get<ISimilarityUtility>())
        {
        }

        internal QueryFingerprintService(IAudioSequencesAnalyzer audioSequencesAnalyzer, ISimilarityUtility similarityCalculationUtility)
        {
            this.audioSequencesAnalyzer = audioSequencesAnalyzer;
            this.similarityCalculationUtility = similarityCalculationUtility;
        }

        public QueryResult Query(IModelService modelService, IEnumerable<HashedFingerprint> hashedFingerprints, QueryConfiguration queryConfiguration)
        {
            var hammingSimilarities = new Dictionary<IModelReference, int>();
            foreach (var hashedFingerprint in hashedFingerprints)
            {
                var subFingerprints = GetSubFingerprints(modelService, hashedFingerprint, queryConfiguration);
                foreach (var subFingerprint in subFingerprints)
                {
                    int hammingSimilarity = similarityCalculationUtility.CalculateHammingSimilarity(hashedFingerprint.SubFingerprint, subFingerprint.Signature);
                    if (!hammingSimilarities.ContainsKey(subFingerprint.TrackReference))
                    {
                        hammingSimilarities.Add(subFingerprint.TrackReference, 0);
                    }

                    hammingSimilarities[subFingerprint.TrackReference] += hammingSimilarity;
                }
            }

            if (!hammingSimilarities.Any())
            {
                return new QueryResult
                    {
                        ResultEntries = Enumerable.Empty<ResultEntry>().ToList(),
                        IsSuccessful = false,
                        AnalyzedCandidatesCount = 0
                    };
            }

            var resultSet = from entry in hammingSimilarities
                            orderby entry.Value descending
                            select new ResultEntry
                                    {
                                        Track = modelService.ReadTrackByReference(entry.Key),
                                        Similarity = entry.Value
                                    };

            return new QueryResult
                {
                    ResultEntries = resultSet.Take(queryConfiguration.MaximumNumberOfTracksToReturnAsResult).ToList(),
                    IsSuccessful = true,
                    AnalyzedCandidatesCount = hammingSimilarities.Count
                };
        }

        public QueryResult Query2(IModelService modelService, IEnumerable<HashedFingerprint> hashedFingerprints, QueryConfiguration queryConfiguration)
        {
            var allCandidates = GetAllCandidates(modelService, hashedFingerprints, queryConfiguration);
            var lcs = GetLongestSubSequenceAccrossAllCandidates(allCandidates);
            var returnresult = new QueryResult
                {
                    ResultEntries = new List<ResultEntry> { new ResultEntry { Track = modelService.ReadTrackByReference(lcs[0].TrackReference) } },
                    SequenceStart = lcs.First().SequenceAt,
                    SequenceLength = lcs.Last().SequenceAt - lcs.First().SequenceAt
                };

            return returnresult;
        }

        private Dictionary<IModelReference, ISet<SubFingerprintData>> GetAllCandidates(IModelService modelService, IEnumerable<HashedFingerprint> hashedFingerprints, QueryConfiguration queryConfiguration)
        {
            var allCandidates = new Dictionary<IModelReference, ISet<SubFingerprintData>>();
            foreach (var hashedFingerprint in hashedFingerprints)
            {
                var subFingerprints = GetSubFingerprints(modelService, hashedFingerprint, queryConfiguration);
                foreach (var subFingerprint in subFingerprints)
                {
                    if (!allCandidates.ContainsKey(subFingerprint.TrackReference))
                    {
                        allCandidates.Add(
                            subFingerprint.TrackReference,
                            new SortedSet<SubFingerprintData>(new SubFingerprintSequenceComparer()));
                    }

                    allCandidates[subFingerprint.TrackReference].Add(subFingerprint);
                }
            }

            return allCandidates;
        }

        private List<SubFingerprintData> GetLongestSubSequenceAccrossAllCandidates(Dictionary<IModelReference, ISet<SubFingerprintData>> allCandidates)
        {
            var lcs = Enumerable.Empty<SubFingerprintData>().ToList();
            int max = int.MinValue;
            foreach (var candidate in allCandidates)
            {
                var longest = audioSequencesAnalyzer.GetLongestIncreasingSubSequence(candidate.Value.ToList()).ToList();
                if (longest.Count > max)
                {
                    max = longest.Count;
                    lcs = longest;
                }
            }

            return lcs;
        }

        private IEnumerable<SubFingerprintData> GetSubFingerprints(IModelService modelService, HashedFingerprint hash, QueryConfiguration queryConfiguration)
        {
            if (!string.IsNullOrEmpty(queryConfiguration.TrackGroupId))
            {
                return modelService.ReadSubFingerprintDataByHashBucketsThresholdWithGroupId(hash.HashBins, queryConfiguration.ThresholdVotes, queryConfiguration.TrackGroupId);
            }

            return modelService.ReadSubFingerprintDataByHashBucketsWithThreshold(hash.HashBins, queryConfiguration.ThresholdVotes);
        }
    }
}

