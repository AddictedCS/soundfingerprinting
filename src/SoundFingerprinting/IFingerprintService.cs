namespace SoundFingerprinting
{
    using System.Collections.Generic;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.FFT;

    public interface IFingerprintService
    {
        List<SpectralImage> CreateSpectralImages(AudioSamples samples, FingerprintConfiguration configuration);

        List<bool[]> CreateFingerprints(AudioSamples samples, FingerprintConfiguration configuration);
    }
}