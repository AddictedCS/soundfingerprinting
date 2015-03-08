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
                    int hammingSimilarity = similarityCalculationUtility.CalculateHammingSimilarity(
                        hashedFingerprint.SubFingerprint, subFingerprint.Signature);
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
            var allSubfingerprints = new Dictionary<IModelReference, ISet<SubFingerprintData>>();
            foreach (var hashedFingerprint in hashedFingerprints)
            {
                var subFingerprints = GetSubFingerprints(modelService, hashedFingerprint, queryConfiguration);
                foreach (var subFingerprint in subFingerprints)
                {
                    if (!allSubfingerprints.ContainsKey(subFingerprint.TrackReference))
                    {
                        allSubfingerprints.Add(subFingerprint.TrackReference, new SortedSet<SubFingerprintData>(new SubFingerprintSequenceComparer()));
                    }

                    allSubfingerprints[subFingerprint.TrackReference].Add(subFingerprint);
                }
            }

            var groups = new Dictionary<IModelReference, IEnumerable<SubFingerprintData>>();
            foreach (var group in allSubfingerprints)
            {
                var longest = audioSequencesAnalyzer.GetLongestIncreasingSubSequence(group.Value.ToList());
                groups.Add(group.Key, longest);
            }

            var max = groups.Max(g => g.Value.Count());
            var result = groups.FirstOrDefault(g => g.Value.Count() == max);
            var returnresult = new QueryResult
                {
                    ResultEntries = new List<ResultEntry> { new ResultEntry { Track = modelService.ReadTrackByReference(result.Key) } },
                    SequenceStart = result.Value.First().SequenceAt,
                    SequenceLength = result.Value.Last().SequenceAt - result.Value.First().SequenceAt
                };

            return returnresult;
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

    class SubFingerprintSequenceComparer : IComparer<SubFingerprintData>
    {
        public int Compare(SubFingerprintData x, SubFingerprintData y)
        {
            return x.SequenceAt.CompareTo(y.SequenceAt);
        }
    }
}

