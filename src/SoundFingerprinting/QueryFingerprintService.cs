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

        public QueryFingerprintService()
            : this(DependencyResolver.Current.Get<IAudioSequencesAnalyzer>())
        {
        }

        protected QueryFingerprintService(IAudioSequencesAnalyzer audioSequencesAnalyzer)
        {
            this.audioSequencesAnalyzer = audioSequencesAnalyzer;
        }

        public QueryResult Query2(IModelService modelService, IEnumerable<HashedFingerprint> hashes, QueryConfiguration queryConfiguration)
        {
            HashSet<SubFingerprintData> allSubfingerprints = new HashSet<SubFingerprintData>();
            var hammingSimilarities = new Dictionary<IModelReference, int>();
            foreach (var hash in hashes)
            {
                var subFingerprints = GetSubFingerprints(modelService, hash, queryConfiguration);
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

                    allSubfingerprints.Add(subFingerprint);
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

        public QueryResult Query(IModelService modelService, IEnumerable<HashedFingerprint> hashes, QueryConfiguration queryConfiguration)
        {
            var allSubfingerprints = new Dictionary<IModelReference, ISet<SubFingerprintData>>();
            foreach (var hash in hashes)
            {
                var subFingerprints = GetSubFingerprints(modelService, hash, queryConfiguration);
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

