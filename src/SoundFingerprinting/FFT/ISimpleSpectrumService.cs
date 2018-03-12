namespace SoundFingerprinting.FFT
{
    using SoundFingerprinting.Audio;

    interface ISimpleSpectrumService
    {
        float[][] CreateSpectrogram(AudioSamples samples, int overlap, int wdftSize);
    }
}
