namespace SoundFingerprinting.FFT
{
    using System.Collections.Generic;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;

    internal interface ISpectrumService
    {
        List<Frame> CreateLogSpectrogram(AudioSamples audioSamples, SpectrogramConfig configuration);
    }
}