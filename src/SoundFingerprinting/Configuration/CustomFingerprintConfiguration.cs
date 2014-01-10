namespace SoundFingerprinting.Configuration
{
    using SoundFingerprinting.Strides;
    
    public class CustomFingerprintConfiguration : IFingerprintConfiguration
    {
        public CustomFingerprintConfiguration()
        {
            DefaultFingerprintConfiguration defaultFingerprint = new DefaultFingerprintConfiguration();
            SamplesPerFingerprint = defaultFingerprint.SamplesPerFingerprint;
            Overlap = defaultFingerprint.Overlap;
            WdftSize = defaultFingerprint.WdftSize;
            MinFrequency = defaultFingerprint.MinFrequency;
            MaxFrequency = defaultFingerprint.MaxFrequency;
            TopWavelets = defaultFingerprint.TopWavelets;
            SampleRate = defaultFingerprint.SampleRate;
            LogBase = defaultFingerprint.LogBase;
            FingerprintLength = defaultFingerprint.FingerprintLength;
            Stride = defaultFingerprint.Stride;
            LogBins = defaultFingerprint.LogBins;
            NormalizeSignal = defaultFingerprint.NormalizeSignal;
            UseDynamicLogBase = defaultFingerprint.UseDynamicLogBase;
            NumberOfLSHTables = defaultFingerprint.NumberOfLSHTables;
            NumberOfMinHashesPerTable = defaultFingerprint.NumberOfMinHashesPerTable;
        }

        public int SamplesPerFingerprint { get; set; }

        public int Overlap { get; set; }

        public int WdftSize { get; set; }

        public int MinFrequency { get; set; }

        public int MaxFrequency { get; set; }

        public int TopWavelets { get; set; }

        public int SampleRate { get; set; }

        public double LogBase { get; set; }

        public int LogBins { get; private set; }

        public int FingerprintLength { get; set; }

        public IStride Stride { get; set; }

        public bool NormalizeSignal { get; set; }

        public bool UseDynamicLogBase { get; set; }

        public int NumberOfLSHTables { get; set; }

        public int NumberOfMinHashesPerTable { get; set; }
    }
}