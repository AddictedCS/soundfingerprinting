namespace SoundFingerprinting
{
    using System.Collections.Generic;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.FFT;

    public interface IFingerprintService
    {
        List<SpectralImage> CreateSpectralImages(float[] samples, FingerprintConfiguration configuration);

        List<bool[]> CreateFingerprints(float[] samples, FingerprintConfiguration configuration);
    }
}