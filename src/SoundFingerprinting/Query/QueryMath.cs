namespace SoundFingerprinting.Query
{
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
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

        public List<ResultEntry> GetBestCandidates(IDictionary<IModelReference, ResultEntryAccumulator> hammingSimilarites, int numberOfCandidatesToReturn, IModelService modelService, FingerprintConfiguration fingerprintConfiguration, double queryLength)
        {
            return hammingSimilarites.OrderByDescending(e => e.Value.HammingSimilaritySum)
                                     .Take(numberOfCandidatesToReturn)
                                     .Select(e => GetResultEntry(modelService, fingerprintConfiguration, e, queryLength))
                                     .ToList();
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
                track.TrackLengthSec);

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
            if (bestMatch.SubFingerprint.SequenceAt > bestMatch.HashedFingerprint.StartsAt)
            {
                return 0;
            }

            return bestMatch.HashedFingerprint.StartsAt - bestMatch.SubFingerprint.SequenceAt;
        }
    }
}
