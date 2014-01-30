namespace SoundFingerprinting.FFT
{
    using SoundFingerprinting.Configuration;

    internal interface ILogUtility
    {
        int[] GenerateLogFrequenciesRanges(IFingerprintConfiguration configuration);

        int FrequencyToSpectrumIndex(float frequency, int sampleRate, int spectrumLength);
    }
}
