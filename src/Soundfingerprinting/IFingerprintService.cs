namespace Soundfingerprinting
{
    using System.Collections.Generic;

    using Soundfingerprinting.Configuration;

    public interface IFingerprintService
    {
        List<bool[]> CreateFingerprints(float[] samples, IFingerprintingConfiguration fingerprintingConfiguration);
    }
}