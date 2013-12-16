namespace SoundFingerprinting.Builder
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Dao.Entities;

    public interface IFingerprintCommand
    {
        IFingerprintingConfiguration Configuration { get; }

        IFingerprintCommand ForTrack(Track track);

        Task<List<Fingerprint>> Fingerprint();

        Task<List<SubFingerprint>> Encode();

        Task<Dictionary<SubFingerprint, List<HashBinMinHash>>> Hash();
    }
}