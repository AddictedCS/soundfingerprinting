namespace SoundFingerprinting.Command
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;

    public interface IFingerprintCommand
    {
        FingerprintConfiguration FingerprintConfiguration { get; }
 
        Task<List<Fingerprint>> Fingerprint();
        
        Task<List<HashedFingerprint>> Hash();
    }
}