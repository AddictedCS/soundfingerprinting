namespace SoundFingerprinting.FFT
{
    using System.Collections.Generic;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;

    public interface ISpectrumService
    {
        List<SpectralImage> CreateLogSpectrogram(AudioSamples audioSamples, SpectrogramConfig configuration);

        float[][] CreateSpectrogram(AudioSamples samples, int overlap, int wdftSize);
    }
}