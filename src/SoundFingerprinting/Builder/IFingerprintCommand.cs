namespace SoundFingerprinting.Builder
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using SoundFingerprinting.Configuration;

    public interface IFingerprintCommand
    {
        IFingerprintingConfiguration Configuration { get; }

        Task<List<bool[]>> Fingerprint();

        Task<List<HashData>> Hash();
    }
}