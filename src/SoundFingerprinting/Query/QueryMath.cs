namespace SoundFingerprinting.Query
{
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.LCS;

    internal class QueryMath : IQueryMath
    {
        private readonly IQueryResultCoverageCalculator queryResultCoverageCalculator;
        private readonly IConfidenceCalculator confidenceCalculator;

        public QueryMath()
            : this(
                DependencyResolver.Current.Get<IQueryResultCoverageCalculator>(),
                DependencyResolver.Current.Get<IConfidenceCalculator>())
        {
        }

        internal QueryMath(IQueryResultCoverageCalculator queryResultCoverageCalculator, IConfidenceCalculator confidenceCalculator)
        {
            this.queryResultCoverageCalculator = queryResultCoverageCalculator;
            this.confidenceCalculator = confidenceCalculator;
        }

        public List<ResultEntry> GetBestCandidates(
            List<HashedFingerprint> hashedFingerprints,
            IDictionary<IModelReference, ResultEntryAccumulator> hammingSimilarites,
            int maxNumberOfMatchesToReturn,
            IModelService modelService,
            FingerprintConfiguration fingerprintConfiguration)
        {
            double queryLength = CalculateExactQueryLength(hashedFingerprints, fingerprintConfiguration);
            return hammingSimilarites.OrderByDescending(e => e.Value.HammingSimilaritySum)
                                     .Take(maxNumberOfMatchesToReturn)
                                     .Select(e => GetResultEntry(modelService, fingerprintConfiguration, e, queryLength))
                                     .ToList();
        }

        public bool IsCandidatePassingThresholdVotes(HashedFingerprint queryFingerprint, SubFingerprintData candidate, int thresholdVotes)
        {
            long[] query = queryFingerprint.HashBins;
            long[] result = candidate.Hashes;
            int count = 0;
            for (int i = 0; i < query.Length; ++i)
            {
                if (query[i] == result[i])
                {
                    count++;
                }

                if (count >= thresholdVotes)
                {
                    return true;
                }
            }

            return false;
        }

        public double CalculateExactQueryLength(IEnumerable<HashedFingerprint> hashedFingerprints, FingerprintConfiguration fingerprintConfiguration)
        {
            double startsAt = double.MaxValue, endsAt = double.MinValue;
            foreach (var hashedFingerprint in hashedFingerprints)
            {
                startsAt = System.Math.Min(startsAt, hashedFingerprint.StartsAt);
                endsAt = System.Math.Max(endsAt, hashedFingerprint.StartsAt);
            }

            return SubFingerprintsToSeconds.AdjustLengthToSeconds(endsAt, startsAt, fingerprintConfiguration);
        }

        private ResultEntry GetResultEntry(IModelService modelService, FingerprintConfiguration configuration, KeyValuePair<IModelReference, ResultEntryAccumulator> pair, double queryLength)
        {
            var track = modelService.ReadTrackByReference(pair.Key);
            var coverage = queryResultCoverageCalculator.GetCoverage(
                pair.Value.Matches,
                queryLength,
                configuration);

            double confidence = confidenceCalculator.CalculateConfidence(
                coverage.SourceMatchStartsAt,
                coverage.SourceMatchLength,
                queryLength,
                coverage.OriginMatchStartsAt,
                track.Length);

            return new ResultEntry(
                track,
                coverage.SourceMatchStartsAt,
                coverage.SourceMatchLength,
                coverage.OriginMatchStartsAt,
                GetTrackStartsAt(pair.Value.BestMatch),
                confidence,
                pair.Value.HammingSimilaritySum,
                queryLength,
                pair.Value.BestMatch);
        }

        private double GetTrackStartsAt(MatchedPair bestMatch)
        {
            return bestMatch.HashedFingerprint.StartsAt - bestMatch.SubFingerprint.SequenceAt;
        }
    }
}
