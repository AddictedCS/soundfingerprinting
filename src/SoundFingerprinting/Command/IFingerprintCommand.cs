namespace SoundFingerprinting.Command
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;

    public interface IFingerprintCommand
    {
        IFingerprintConfiguration FingerprintConfiguration { get; }

        Task<List<bool[]>> Fingerprint();

        Task<List<FingerprintRawData>> FingerprintRaw();

        Task<List<HashData>> Hash();
    }
}