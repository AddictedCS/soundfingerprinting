namespace SoundFingerprinting.Utils
{
    using System.Collections.Generic;

    using SoundFingerprinting.Configuration;

    internal class TestRunnerConfig
    {
        private readonly int samplesPerFingerprint = new DefaultFingerprintConfiguration().SamplesPerFingerprint;

        public List<string> AudioFileFilters
        {
            get
            {
                return new List<string> { "*.wav" };
            }
        }

        public double[] Percentiles
        {
            get
            {
                return new[] { 0.8, 0.9, 0.95, 0.98 };
            }
        }

        public int SamplesPerFingerprint
        {
            get
            {
                return samplesPerFingerprint;
            }
        }

        public char StartAtsSeparator
        {
            get
            {
                return '|';
            }
        }
    }
}
