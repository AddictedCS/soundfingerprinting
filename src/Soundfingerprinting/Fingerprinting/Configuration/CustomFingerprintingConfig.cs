namespace Soundfingerprinting.Fingerprinting.Configuration
{
    using Soundfingerprinting.AudioProxies.Strides;

    public class CustomFingerprintingConfig : IFingerprintingConfig
    {
        public CustomFingerprintingConfig()
        {
            DefaultFingerprintingConfig defaultFingerprinting = new DefaultFingerprintingConfig();
            SamplesPerFingerprint = defaultFingerprinting.SamplesPerFingerprint;
            Overlap = defaultFingerprinting.Overlap;
            WdftSize = defaultFingerprinting.WdftSize;
            MinFrequency = defaultFingerprinting.MinFrequency;
            MaxFrequency = defaultFingerprinting.MaxFrequency;
            TopWavelets = defaultFingerprinting.TopWavelets;
            SampleRate = defaultFingerprinting.SampleRate;
            LogBase = defaultFingerprinting.LogBase;
            FingerprintLength = defaultFingerprinting.FingerprintLength;
            Stride = defaultFingerprinting.Stride;
            LogBins = defaultFingerprinting.LogBins;
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
    }
}