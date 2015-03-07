namespace SoundFingerprinting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.LCS;
    using SoundFingerprinting.Math;
    using SoundFingerprinting.Query;

    public class QueryFingerprintService : IQueryFingerprintService
    {
        public QueryResult Query2(IModelService modelService, IEnumerable<HashData> hashes, IQueryConfiguration queryConfiguration)
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

        public QueryResult Query(IModelService modelService, IEnumerable<HashData> hashes, IQueryConfiguration queryConfiguration)
        {
            IAudioSequencesAnalyzer sequencesAnalyzer = new AudioSequencesAnalyzer();
            HashSet<SubFingerprintData> allSubfingerprints = new HashSet<SubFingerprintData>();
            foreach (var hash in hashes)
            {
                var subFingerprints = GetSubFingerprints(modelService, hash, queryConfiguration);
                foreach (var subFingerprint in subFingerprints)
                {
                    allSubfingerprints.Add(subFingerprint);
                }
            }

            Dictionary<IModelReference, IEnumerable<SubFingerprintData>> groups = new Dictionary<IModelReference, IEnumerable<SubFingerprintData>>();
            foreach (var group in allSubfingerprints.GroupBy(s => s.TrackReference))
            {
                var subs = group.ToList();
                subs.Sort((s1, s2) => s1.SequenceNumber.CompareTo(s2.SequenceNumber));
                var longest = sequencesAnalyzer.GetLongestIncreasingSubSequence(subs);
                groups.Add(group.Key, longest);
            }

            var max = groups.Max(g => g.Value.Count());
            var result = groups.FirstOrDefault(g => g.Value.Count() == max);
            var returnresult = new QueryResult2
            {
                Track = modelService.ReadTrackByReference(result.Key),
                SequenceStart = (result.Value.First().SequenceNumber - 1) * 5115d / 5512,
                SequenceLength = 5115d / 5512 * result.Value.Count()
            };

            return returnresult;
        }

        private IEnumerable<SubFingerprintData> GetSubFingerprints(IModelService modelService, HashData hash, IQueryConfiguration queryConfiguration)
        {
            if (!string.IsNullOrEmpty(queryConfiguration.TrackGroupId))
            {
                return modelService.ReadSubFingerprintDataByHashBucketsThresholdWithGroupId(hash.HashBins, queryConfiguration.ThresholdVotes, queryConfiguration.TrackGroupId);
            }

            return modelService.ReadSubFingerprintDataByHashBucketsWithThreshold(hash.HashBins, queryConfiguration.ThresholdVotes);
        }
    }
}
