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
                return new List<string>() { "*.mp3" };
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
