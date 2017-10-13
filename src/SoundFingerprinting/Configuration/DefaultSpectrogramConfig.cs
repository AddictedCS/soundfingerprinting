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
            Stride = new IncrementalStaticStride(1024);
            Window = new HanningWindow();
        }
    }
}
