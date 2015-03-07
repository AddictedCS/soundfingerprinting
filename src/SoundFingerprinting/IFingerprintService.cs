namespace SoundFingerprinting
{
    using System.Collections.Generic;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;

    public interface IFingerprintService
    {
        List<SpectralImage> CreateSpectralImages(AudioSamples samples, FingerprintConfiguration configuration); //TODO does this method belong to fingerprint service?

        List<Fingerprint> CreateFingerprints(AudioSamples samples, FingerprintConfiguration configuration);
    }
}