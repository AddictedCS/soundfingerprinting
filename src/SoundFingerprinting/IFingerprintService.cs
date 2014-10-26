namespace SoundFingerprinting
{
    using System.Collections.Generic;

    using SoundFingerprinting.Configuration;

    public interface IFingerprintService
    {
        List<float[][]> CreateSpectralImages(float[] samples, IFingerprintConfiguration fingerprintConfiguration);

        List<bool[]> CreateFingerprints(float[] samples, IFingerprintConfiguration fingerprintConfiguration);
    }
}