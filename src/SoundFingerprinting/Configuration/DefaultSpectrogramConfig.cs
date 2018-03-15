namespace SoundFingerprinting.Configuration
{
    using SoundFingerprinting.Strides;
    using SoundFingerprinting.Windows;

    internal class DefaultSpectrogramConfig : SpectrogramConfig
    {
        public DefaultSpectrogramConfig()
        {
            Overlap = 64;
            WdftSize = 2048;
            FrequencyRange = new FrequencyRange { Min = 318, Max = 2000 };
            LogBase = 2;
            LogBins = 32;
            ImageLength = 128;
            UseDynamicLogBase = true;
            NormalizeSignal = false;
            Stride = new IncrementalStaticStride(1536);
            Window = new HanningWindow();
            ScalingFunction = (value, max) =>
            {
                float scaled = System.Math.Min(value / max, 1); // scaled [0, 1]
                int domain = 255;
                float c = (float)(1 / System.Math.Log(1 + domain));
                return (float)(c * System.Math.Log(1 + scaled * domain));
            };
        }
    }
}
