namespace SoundFingerprinting.Query
{
    using SoundFingerprinting.Configuration;

    internal class QueryMath : IQueryMath
    {
        public double AdjustSnippetLengthToConfigsUsedDuringFingerprinting(double snipetLength, FingerprintConfiguration fingerprintConfiguration)
        {
            int sampleRate = fingerprintConfiguration.SampleRate;
            int wdftSize = fingerprintConfiguration.SpectrogramConfig.WdftSize;
            int fingerprintSize = fingerprintConfiguration.SamplesPerFingerprint;
            double firstFingerprint = ((double)(fingerprintSize + wdftSize)) / sampleRate;
            return snipetLength + firstFingerprint;
        }
    }
}
