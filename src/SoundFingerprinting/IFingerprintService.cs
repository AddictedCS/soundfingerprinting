namespace SoundFingerprinting
{
    using System.Collections.Generic;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.FFT;

    public interface IFingerprintService
    {
        List<SpectralImage> CreateSpectralImages(float[] samples, IFingerprintConfiguration configuration);

        List<bool[]> CreateFingerprints(float[] samples, IFingerprintConfiguration configuration);
    }
}