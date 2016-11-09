namespace SoundFingerprinting.Query
{
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
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

        public List<ResultEntry> GetBestCandidates(IDictionary<IModelReference, int> hammingSimilarites, int numberOfCandidatesToReturn, IModelService modelService)
        {
            return hammingSimilarites.OrderByDescending(e => e.Value)
                                     .Take(numberOfCandidatesToReturn)
                                     .Select(e => new ResultEntry
                                         {
                                             Track = modelService.ReadTrackByReference(e.Key), MatchedFingerprints = e.Value
                                         })
                                    .ToList();
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
