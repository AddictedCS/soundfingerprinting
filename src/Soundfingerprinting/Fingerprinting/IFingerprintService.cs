namespace Soundfingerprinting.Fingerprinting
{
    using System.Collections.Generic;

    using Soundfingerprinting.Fingerprinting.Configuration;

    public interface IFingerprintService
    {
        List<bool[]> CreateFingerprints(float[] samples, IFingerprintingConfiguration fingerprintingConfiguration);
    }
}