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
        private readonly ISimilarityUtility similarityUtility;
        private readonly IQueryMath queryMath;

        public QueryFingerprintService()
            : this(
                DependencyResolver.Current.Get<IAudioSequencesAnalyzer>(),
                DependencyResolver.Current.Get<ISimilarityUtility>(),
                DependencyResolver.Current.Get<IQueryMath>())
        {
        }

        internal QueryFingerprintService(IAudioSequencesAnalyzer audioSequencesAnalyzer, ISimilarityUtility similarityUtility, IQueryMath queryMath)
        {
            this.audioSequencesAnalyzer = audioSequencesAnalyzer;
            this.similarityUtility = similarityUtility;
            this.queryMath = queryMath;
        }
    
        public QueryResult Query(IModelService modelService, List<HashedFingerprint> hashedFingerprints, QueryConfiguration queryConfiguration)
        {
            var hammingSimilarities = new Dictionary<IModelReference, int>();
            int subFingerprintsCount = 0;
            foreach (var hashedFingerprint in hashedFingerprints)
            {
                var subFingerprints = modelService.ReadSubFingerprints(hashedFingerprint.HashBins, queryConfiguration);
                subFingerprintsCount += subFingerprints.Count;
                similarityUtility.AccumulateHammingSimilarity(subFingerprints, hashedFingerprint.SubFingerprint, hammingSimilarities);
            }

            double snipetLength = queryMath.CalculateExactSnippetLength(hashedFingerprints, queryConfiguration.FingerprintConfiguration);
            if (!hammingSimilarities.Any())
            {
                return EmptyResult(snipetLength);
            }

            var resultEntries = queryMath.GetBestCandidates(hammingSimilarities, queryConfiguration.MaximumNumberOfTracksToReturnAsResult, modelService);
            return QueryResult(resultEntries, hammingSimilarities.Count, subFingerprintsCount, snipetLength);
        }

        public QueryResult QueryWithTimeSequenceInformation(IModelService modelService, List<HashedFingerprint> hashedFingerprints, QueryConfiguration queryConfiguration)
        {
            var allCandidates = GetAllCandidates(modelService, hashedFingerprints, queryConfiguration);
            if (!allCandidates.Item1.Any())
            {
                return this.EmptyResult(allCandidates.Item2);
            }

            var entries = GetCandidatesSortedByLCS(allCandidates.Item1, allCandidates.Item2);

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

        // TODO refactor drastically 
        // This method will be improved in 3x
        public QueryResult QueryExperimental(IModelService modelService, List<HashedFingerprint> hashedFingerprints, QueryConfiguration queryConfiguration)
        {
            var hammingSimilarities = new Dictionary<IModelReference, int>();
            double snipetLength = queryMath.CalculateExactSnippetLength(hashedFingerprints, queryConfiguration.FingerprintConfiguration);
            var allCandidates = modelService.ReadAllSubFingerprintCandidatesWithThreshold(hashedFingerprints, queryConfiguration.ThresholdVotes);

            foreach (var hashedFingerprint in hashedFingerprints)
            {
                HashedFingerprint fingerprint = hashedFingerprint;
                var subFingerprints =
                    allCandidates.Where(
                        candidate => DoesMatchThresholdVotes(queryConfiguration, fingerprint, candidate));

                similarityUtility.AccumulateHammingSimilarity(subFingerprints, hashedFingerprint.SubFingerprint, hammingSimilarities);
            }

            if (!hammingSimilarities.Any())
            {
                return EmptyResult(snipetLength);
            }

            var resultEntries = queryMath.GetBestCandidates(hammingSimilarities, queryConfiguration.MaximumNumberOfTracksToReturnAsResult, modelService);
            return QueryResult(resultEntries, hammingSimilarities.Count, allCandidates.Count, snipetLength);
        }

        private bool DoesMatchThresholdVotes(QueryConfiguration queryConfiguration, HashedFingerprint fingerprint, SubFingerprintData candidate)
        {
            long[] actual = fingerprint.HashBins;
            var result = candidate.Hashes;
            int count = 0;
            for (int i = 0; i < actual.Length; ++i)
            {
                if (actual[i] == result[i])
                {
                    count++;
                }

                if (count >= queryConfiguration.ThresholdVotes)
                {
                    return true;
                }
            }

            return false;
        }

        private Tuple<Dictionary<IModelReference, SubfingerprintSetSortedByTimePosition>, double> GetAllCandidates(IModelService modelService, List<HashedFingerprint> hashedFingerprints, QueryConfiguration queryConfiguration)
        {
            var allCandidates = new Dictionary<IModelReference, SubfingerprintSetSortedByTimePosition>();
            double snipetLength = queryMath.CalculateExactSnippetLength(hashedFingerprints, queryConfiguration.FingerprintConfiguration);
            foreach (var hashedFingerprint in hashedFingerprints)
            {
                var subFingerprints = modelService.ReadSubFingerprints(hashedFingerprint.HashBins, queryConfiguration); // TODO No need to extract full subfingerprint from the DB. We use only TrackReference and Sequence # to build the result
                foreach (var subFingerprint in subFingerprints)
                {
                    if (!allCandidates.ContainsKey(subFingerprint.TrackReference))
                    {
                        allCandidates.Add(subFingerprint.TrackReference, new SubfingerprintSetSortedByTimePosition());
                    }

                    allCandidates[subFingerprint.TrackReference].Add(subFingerprint);
                }
            }

            return new Tuple<Dictionary<IModelReference, SubfingerprintSetSortedByTimePosition>, double>(allCandidates, snipetLength);
        }

        private IEnumerable<IEnumerable<SubFingerprintData>> GetCandidatesSortedByLCS(Dictionary<IModelReference, SubfingerprintSetSortedByTimePosition> allCandidates, double snipetLength)
        {
             return audioSequencesAnalyzer.SortCandiatesByLongestIncresingAudioSequence(allCandidates, snipetLength).ToList();
        }
        
        private QueryResult EmptyResult(double snipetLength)
        {
            return new QueryResult
                {
                    ResultEntries = Enumerable.Empty<ResultEntry>().ToList(),
                    AnalyzedTracksCount = 0,
                    Info = new QueryInfo { SnippetLength = snipetLength }
                };
        }

        private QueryResult QueryResult(List<ResultEntry> resultEntries, int trackCount, int subFingerprintsCount, double snipetLength)
        {
            return new QueryResult
                {
                    ResultEntries = resultEntries,
                    AnalyzedTracksCount = trackCount,
                    AnalyzedSubFingerprintsCount = subFingerprintsCount,
                    Info = new QueryInfo { SnippetLength = snipetLength }
                };
        }
    }
}
