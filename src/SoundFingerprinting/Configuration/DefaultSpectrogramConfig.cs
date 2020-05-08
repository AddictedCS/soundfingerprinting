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
        }
    }
}
