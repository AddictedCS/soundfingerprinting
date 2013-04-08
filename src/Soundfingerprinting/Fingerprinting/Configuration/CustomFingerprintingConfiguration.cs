namespace Soundfingerprinting.Fingerprinting.Configuration
{
    using Soundfingerprinting.Audio.Strides;
    using Soundfingerprinting.Fingerprinting.Windows;

    public class CustomFingerprintingConfiguration : IFingerprintingConfiguration
    {
        public CustomFingerprintingConfiguration()
        {
            DefaultFingerprintingConfiguration defaultFingerprinting = new DefaultFingerprintingConfiguration();
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
            WindowFunction = defaultFingerprinting.WindowFunction;
            NormalizeSignal = defaultFingerprinting.NormalizeSignal;
            UseDynamicLogBase = defaultFingerprinting.UseDynamicLogBase;
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

        public IWindowFunction WindowFunction { get; set; }

        public bool NormalizeSignal { get; set; }

        public bool UseDynamicLogBase { get; set; }
    }
}