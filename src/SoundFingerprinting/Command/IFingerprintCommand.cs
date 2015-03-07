namespace SoundFingerprinting.Command
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.FFT;

    public interface IFingerprintCommand
    {
        FingerprintConfiguration FingerprintConfiguration { get; }

        Task<List<SpectralImage>> CreateSpectralImages(); 

        Task<List<bool[]>> Fingerprint();

        Task<List<HashData>> Hash();
    }
}