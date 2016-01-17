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
    
        private static QueryResult NoResult
        {
            get
            {
                return new QueryResult
                    {
                        ResultEntries = Enumerable.Empty<ResultEntry>().ToList(),
                        IsSuccessful = false,
                        AnalyzedCandidatesCount = 0
                    };
            }
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
                return NoResult;
            }

            var resultEntries = hammingSimilarities.OrderByDescending(e => e.Value)
                               .Take(queryConfiguration.MaximumNumberOfTracksToReturnAsResult)
                               .Select(e => new ResultEntry
                                   {
                                       Track = modelService.ReadTrackByReference(e.Key),
                                       Similarity = e.Value
                                   })
                                .ToList();

            return new QueryResult
                {
                    ResultEntries = resultEntries,
                    IsSuccessful = true,
                    AnalyzedCandidatesCount = hammingSimilarities.Count
                };
        }

        public QueryResult QueryWithTimeSequenceInformation(IModelService modelService, IEnumerable<HashedFingerprint> hashedFingerprints, QueryConfiguration queryConfiguration)
        {
            var allCandidates = GetAllCandidates(modelService, hashedFingerprints, queryConfiguration);
            if (!allCandidates.Any())
            {
                return NoResult;
            }

            var entries = this.GetCandidatesSortedByLCS(allCandidates);

            var resultEntries = entries
                   .Take(queryConfiguration.MaximumNumberOfTracksToReturnAsResult)
                   .Select(datas => new ResultEntry
                    {
                        Track = modelService.ReadTrackByReference(datas.First().TrackReference),
                        Similarity = datas.Count(),
                        SequenceStart = datas.First().SequenceAt,
                        SequenceLength = datas.Last().SequenceAt - datas.First().SequenceAt + 1.48d // TODO 1.48 because of default fingerprint config. For other configurations there is going to be equal to Overlap * ImageLength / SampleRate 
                    })
                    .ToList();

            var returnresult = new QueryResult
                {
                    IsSuccessful = true,
                    ResultEntries = resultEntries,
                    AnalyzedCandidatesCount = allCandidates.Count
                };

            return returnresult;
        }

        private Dictionary<IModelReference, ISet<SubFingerprintData>> GetAllCandidates(IModelService modelService, IEnumerable<HashedFingerprint> hashedFingerprints, QueryConfiguration queryConfiguration)
        {
            var allCandidates = new Dictionary<IModelReference, ISet<SubFingerprintData>>();
            foreach (var hashedFingerprint in hashedFingerprints)
            {
                var subFingerprints = GetSubFingerprints(modelService, hashedFingerprint, queryConfiguration); // TODO No need to extract full subfingerprint from the DB. We use only TrackReference and Sequence # to build the result
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

        private IEnumerable<IEnumerable<SubFingerprintData>> GetCandidatesSortedByLCS(Dictionary<IModelReference, ISet<SubFingerprintData>> allCandidates)
        {
            var resultSet = new SortedSet<IEnumerable<SubFingerprintData>>(lengthComparer);

            foreach (var candidate in allCandidates)
            {
                var lcs = audioSequencesAnalyzer.GetLongestIncreasingSubSequence(candidate.Value.ToList()).ToList();
                foreach (var lc in lcs)
                {
                    resultSet.Add(lc);
                }
            }

            return resultSet;
        }
        
        private IEnumerable<SubFingerprintData> GetSubFingerprints(IModelService modelService, HashedFingerprint hash, QueryConfiguration queryConfiguration)
        {
            if (!string.IsNullOrEmpty(queryConfiguration.TrackGroupId))
            {
                return modelService.ReadSubFingerprintDataByHashBucketsThresholdWithGroupId(hash.HashBins, queryConfiguration.ThresholdVotes, queryConfiguration.TrackGroupId);
            }

            return modelService.ReadSubFingerprintDataByHashBucketsWithThreshold(hash.HashBins, queryConfiguration.ThresholdVotes);
        }
    
        // TODO Loose comparison, if 2 sequences are equal last added will be selected as the winner
        private Comparer<IEnumerable<SubFingerprintData>> lengthComparer = 
            Comparer<IEnumerable<SubFingerprintData>>.Create((a, b) => b.Count().CompareTo(a.Count())); 
    }
}
