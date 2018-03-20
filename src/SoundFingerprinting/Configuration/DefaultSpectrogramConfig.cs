namespace SoundFingerprinting.Configuration
{
    using SoundFingerprinting.Windows;

    internal class DefaultSpectrogramConfig : SpectrogramConfig
    {
        public DefaultSpectrogramConfig()
        {
            Overlap = 64;
            WdftSize = 2048;
            FrequencyRange = Configs.FrequencyRanges.Default;
            LogBase = 2;
            LogBins = 32;
            ImageLength = 128;
            UseDynamicLogBase = true;
            Stride = Configs.FingerprintStrides.Default;
            Window = new HanningWindow();
            ScalingFunction = (value, max) =>
            {
                float scaled = System.Math.Min(value / max, 1);
                int domain = 255;
                float c = (float)(1 / System.Math.Log(1 + domain));
                return (float)(c * System.Math.Log(1 + scaled * domain));
            };
        }
    }
}
