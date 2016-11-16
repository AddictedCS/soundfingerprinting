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

        public double CalculateExactSnippetLength(IEnumerable<HashedFingerprint> hashedFingerprints, FingerprintConfiguration fingerprintConfiguration)
        {
            double min = double.MaxValue, max = double.MinValue;
            foreach (var hashedFingerprint in hashedFingerprints)
            {
                min = System.Math.Min(min, hashedFingerprint.Timestamp);
                max = System.Math.Max(max, hashedFingerprint.Timestamp);
            }

            return AdjustSnippetLengthToConfigsUsedDuringFingerprinting(max - min, fingerprintConfiguration);
        }

        public List<ResultEntry> GetBestCandidates(IDictionary<IModelReference, ResultEntryAccumulator> hammingSimilarites, int numberOfCandidatesToReturn, IModelService modelService, FingerprintConfiguration fingerprintConfiguration, double queryLength)
        {
            return hammingSimilarites.OrderByDescending(e => e.Value.SummedHammingSimilarity)
                                     .Take(numberOfCandidatesToReturn)
                                     .Select(e => GetResultEntry(modelService, fingerprintConfiguration, e, queryLength))
                                     .ToList();
        }

        private ResultEntry GetResultEntry(IModelService modelService, FingerprintConfiguration fingerprintConfiguration, KeyValuePair<IModelReference, ResultEntryAccumulator> pair, double queryLength)
        {
            var track = modelService.ReadTrackByReference(pair.Key);
            var coverage = queryResultCoverageCalculator.GetLongestMatch(
                pair.Value.Matches,
                queryLength,
                (double)fingerprintConfiguration.SamplesPerFingerprint / fingerprintConfiguration.SampleRate);

            double adjustedSourceMatchLength =
                AdjustSnippetLengthToConfigsUsedDuringFingerprinting(
                    coverage.SourceMatchLength, fingerprintConfiguration);

            double confidence = confidenceCalculator.CalculateConfidence(
                coverage.SourceMatchStartsAt,
                adjustedSourceMatchLength,
                pair.Value.BestMatch.HashedFingerprint.SourceDuration,
                coverage.OriginMatchStartsAt,
                track.TrackLengthSec);

            return new ResultEntry(
                track,
                coverage.SourceMatchStartsAt,
                adjustedSourceMatchLength,
                coverage.OriginMatchStartsAt,
                GetTrackStartsAt(pair.Value.BestMatch),
                confidence,
                pair.Value.SummedHammingSimilarity)
                    {
                        BestMatch = pair.Value.BestMatch
                    };
        }

        private double AdjustSnippetLengthToConfigsUsedDuringFingerprinting(double snipetLength, FingerprintConfiguration fingerprintConfiguration)
        {
            int sampleRate = fingerprintConfiguration.SampleRate;
            int wdftSize = fingerprintConfiguration.SpectrogramConfig.WdftSize;
            int fingerprintSize = fingerprintConfiguration.SamplesPerFingerprint;
            double firstFingerprint = ((double)(fingerprintSize + wdftSize)) / sampleRate;
            return snipetLength + firstFingerprint;
        }

        private double GetTrackStartsAt(MatchedPair bestMatch)
        {
            if (bestMatch.SubFingerprint.SequenceAt > bestMatch.HashedFingerprint.Timestamp)
            {
                return 0;
            }

            return bestMatch.HashedFingerprint.Timestamp - bestMatch.SubFingerprint.SequenceAt;
        }
    }
}
