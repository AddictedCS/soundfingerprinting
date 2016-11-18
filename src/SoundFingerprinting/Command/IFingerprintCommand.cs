namespace SoundFingerprinting.Command
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;

    public interface IFingerprintCommand
    {
        /// <summary>
        /// Gets configuration object used during fingerprinting audio source
        /// </summary>
        FingerprintConfiguration FingerprintConfiguration { get; }
 
        /// <summary>
        /// Fingerprint and hash the source
        /// </summary>
        /// <returns>Hashed fingerprints which can be stored in the data source</returns>
        Task<List<HashedFingerprint>> Hash();
    }
}