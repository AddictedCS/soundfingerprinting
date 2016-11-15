namespace SoundFingerprinting.Query
{
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    internal class QueryMath : IQueryMath
    {
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
            return hammingSimilarites.OrderByDescending(e => e.Value.HammingSimilarity)
                                     .Take(numberOfCandidatesToReturn)
                                     .Select(e => GetResultEntry(modelService, fingerprintConfiguration, e, queryLength))
                                     .ToList();
        }

        private ResultEntry GetResultEntry(IModelService modelService, FingerprintConfiguration fingerprintConfiguration, KeyValuePair<IModelReference, ResultEntryAccumulator> pair, double queryLength)
        {
            var track = modelService.ReadTrackByReference(pair.Key);
            double longestMatch = GetLongestMatch(pair.Value.Matches, queryLength, (double)fingerprintConfiguration.SamplesPerFingerprint / fingerprintConfiguration.SampleRate);
            double sequenceLength = AdjustSnippetLengthToConfigsUsedDuringFingerprinting(longestMatch, fingerprintConfiguration);
            double confidence = sequenceLength / track.TrackLengthSec;
            return new ResultEntry(track, pair.Value.StartAt, sequenceLength, confidence, pair.Value.HammingSimilarity);
        }

        private double GetLongestMatch(List<SubFingerprintData> matches, double queryLength, double oneFingerprintLength)
        {
            int minI = 0, maxI = 0, curMinI = 0, maxLength = 0;
            for (int i = 1; i < matches.Count; ++i)
            {
                if (matches[i].SequenceAt - matches[i - 1].SequenceAt >= queryLength)
                {
                    // potentialy a new start of best match sequence
                    curMinI = i;
                }

                if (i - curMinI > maxLength)
                {
                    maxLength = i - curMinI;
                    maxI = i;
                    minI = curMinI;
                }
            }

            double notCovered = 0d;
            for (int i = minI + 1; i <= maxI; ++i)
            {
                if (matches[i].SequenceAt - matches[i - 1].SequenceAt > oneFingerprintLength)
                {
                    notCovered += matches[i].SequenceAt - (matches[i - 1].SequenceAt + oneFingerprintLength);
                }
            }

            return matches[maxI].SequenceAt - matches[minI].SequenceAt - notCovered;
        }

        private double AdjustSnippetLengthToConfigsUsedDuringFingerprinting(double snipetLength, FingerprintConfiguration fingerprintConfiguration)
        {
            int sampleRate = fingerprintConfiguration.SampleRate;
            int wdftSize = fingerprintConfiguration.SpectrogramConfig.WdftSize;
            int fingerprintSize = fingerprintConfiguration.SamplesPerFingerprint;
            double firstFingerprint = ((double)(fingerprintSize + wdftSize)) / sampleRate;
            return snipetLength + firstFingerprint;
        }
    }
}
