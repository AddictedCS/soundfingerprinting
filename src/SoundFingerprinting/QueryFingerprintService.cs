namespace SoundFingerprinting
{
    using System;
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
        private readonly IQueryMath queryMath;

        public QueryFingerprintService()
            : this(
                DependencyResolver.Current.Get<IAudioSequencesAnalyzer>(),
                DependencyResolver.Current.Get<ISimilarityUtility>(),
                DependencyResolver.Current.Get<IQueryMath>())
        {
        }

        internal QueryFingerprintService(IAudioSequencesAnalyzer audioSequencesAnalyzer, ISimilarityUtility similarityCalculationUtility, IQueryMath queryMath)
        {
            this.audioSequencesAnalyzer = audioSequencesAnalyzer;
            this.similarityCalculationUtility = similarityCalculationUtility;
            this.queryMath = queryMath;
        }
    
        public QueryResult Query(IModelService modelService, IEnumerable<HashedFingerprint> hashedFingerprints, QueryConfiguration queryConfiguration)
        {
            var hammingSimilarities = new Dictionary<IModelReference, int>();
            double snipetLength = 0;
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

                snipetLength = System.Math.Max(snipetLength, hashedFingerprint.Timestamp);
            }

            snipetLength = AdjustSnippetLengthToConfigsUsedDuringFingerprinting(snipetLength, queryConfiguration.FingerprintConfiguration);

            if (!hammingSimilarities.Any())
            {
                return new QueryResult
                    {
                        ResultEntries = Enumerable.Empty<ResultEntry>().ToList(),
                        AnalyzedTracksCount = 0,
                        Info = new QueryInfo { SnippetLength = snipetLength }
                    };
            }

            var resultEntries = hammingSimilarities.OrderByDescending(e => e.Value)
                               .Take(queryConfiguration.MaximumNumberOfTracksToReturnAsResult)
                               .Select(e => new ResultEntry
                                   {
                                       Track = modelService.ReadTrackByReference(e.Key),
                                       MatchedFingerprints = e.Value
                                   })
                                .ToList();

            return new QueryResult
                {
                    ResultEntries = resultEntries,
                    AnalyzedTracksCount = hammingSimilarities.Count,
                    Info = new QueryInfo { SnippetLength = snipetLength }
                };
        }
        
        public QueryResult QueryWithTimeSequenceInformation(IModelService modelService, IEnumerable<HashedFingerprint> hashedFingerprints, QueryConfiguration queryConfiguration)
        {
            var allCandidates = GetAllCandidates(modelService, hashedFingerprints, queryConfiguration);
            if (!allCandidates.Item1.Any())
            {
                return new QueryResult
                    {
                        ResultEntries = Enumerable.Empty<ResultEntry>().ToList(),
                        AnalyzedTracksCount = 0,
                        Info = new QueryInfo { SnippetLength = allCandidates.Item2 }
                    };
            }

            var entries = this.GetCandidatesSortedByLCS(allCandidates.Item1, allCandidates.Item2);

            var resultEntries =
                entries.Take(queryConfiguration.MaximumNumberOfTracksToReturnAsResult).Select(
                    datas =>
                    new ResultEntry
                        {
                            Track = modelService.ReadTrackByReference(datas.First().TrackReference),
                            MatchedFingerprints = datas.Count(),
                            SequenceStart = datas.First().SequenceAt,
                            SequenceLength = datas.Last().SequenceAt - datas.First().SequenceAt,
                            Confidence = (datas.Last().SequenceAt - datas.First().SequenceAt) / allCandidates.Item2
                        }).ToList();

            return new QueryResult { ResultEntries = resultEntries, AnalyzedTracksCount = allCandidates.Item1.Count };
        }

        private Tuple<Dictionary<IModelReference, SubfingerprintSetSortedByTimePosition>, double> GetAllCandidates(IModelService modelService, IEnumerable<HashedFingerprint> hashedFingerprints, QueryConfiguration queryConfiguration)
        {
            var allCandidates = new Dictionary<IModelReference, SubfingerprintSetSortedByTimePosition>();
            double snipetLength = 0;
            foreach (var hashedFingerprint in hashedFingerprints)
            {
                var subFingerprints = GetSubFingerprints(modelService, hashedFingerprint, queryConfiguration); // TODO No need to extract full subfingerprint from the DB. We use only TrackReference and Sequence # to build the result
                foreach (var subFingerprint in subFingerprints)
                {
                    if (!allCandidates.ContainsKey(subFingerprint.TrackReference))
                    {
                        allCandidates.Add(subFingerprint.TrackReference, new SubfingerprintSetSortedByTimePosition());
                    }

                    allCandidates[subFingerprint.TrackReference].Add(subFingerprint);
                }

                snipetLength = System.Math.Max(snipetLength, hashedFingerprint.Timestamp);
            }

            snipetLength = this.AdjustSnippetLengthToConfigsUsedDuringFingerprinting(
                snipetLength, queryConfiguration.FingerprintConfiguration);

            return new Tuple<Dictionary<IModelReference, SubfingerprintSetSortedByTimePosition>, double>(allCandidates, snipetLength);
        }

        private IEnumerable<IEnumerable<SubFingerprintData>> GetCandidatesSortedByLCS(Dictionary<IModelReference, SubfingerprintSetSortedByTimePosition> allCandidates, double snipetLength)
        {
             return audioSequencesAnalyzer.SortCandiatesByLongestIncresingAudioSequence(allCandidates, snipetLength).ToList();
        }
        
        private IEnumerable<SubFingerprintData> GetSubFingerprints(IModelService modelService, HashedFingerprint hash, QueryConfiguration queryConfiguration)
        {
            if (!string.IsNullOrEmpty(queryConfiguration.TrackGroupId))
            {
                return modelService.ReadSubFingerprintDataByHashBucketsThresholdWithGroupId(hash.HashBins, queryConfiguration.ThresholdVotes, queryConfiguration.TrackGroupId);
            }

            return modelService.ReadSubFingerprintDataByHashBucketsWithThreshold(hash.HashBins, queryConfiguration.ThresholdVotes);
        }

        private double AdjustSnippetLengthToConfigsUsedDuringFingerprinting(double snipetLength, FingerprintConfiguration fingerprintConfiguration)
        {
            return queryMath.AdjustSnippetLengthToConfigsUsedDuringFingerprinting(snipetLength, fingerprintConfiguration);
        }
    }
}
