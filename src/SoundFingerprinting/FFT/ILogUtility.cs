namespace SoundFingerprinting.FFT
{
    using SoundFingerprinting.Configuration;

    internal interface ILogUtility
    {
        int[] GenerateLogFrequenciesRanges(int sampleRate, SpectrogramConfig config);

        int FrequencyToSpectrumIndex(float frequency, int sampleRate, int spectrumLength);
    }
}
