namespace SoundFingerprinting
{
    using System.Collections.Generic;

    using SoundFingerprinting.Configuration;

    public interface IFingerprintService
    {
        List<bool[]> CreateFingerprints(float[] samples, IFingerprintingConfiguration fingerprintingConfiguration);
    }
}