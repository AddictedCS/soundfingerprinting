namespace Soundfingerprinting.Fingerprinting
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Soundfingerprinting.Fingerprinting.Configuration;

    public interface IFingerprintService
    {
        Task<List<bool[]>> CreateFingerprints(float[] samples, IFingerprintingConfiguration fingerprintingConfiguration);
    }
}