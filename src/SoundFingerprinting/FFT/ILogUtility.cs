namespace SoundFingerprinting.FFT
{
    using SoundFingerprinting.Configuration;

    internal interface ILogUtility
    {
        ushort[] GenerateLogFrequenciesRanges(int sampleRate, SpectrogramConfig config);

        ushort FrequencyToSpectrumIndex(float frequency, int sampleRate, int spectrumLength);
    }
}
