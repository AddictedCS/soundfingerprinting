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
        /// Fingerprint the source
        /// </summary>
        /// <returns>Fingerprinted source</returns>
        /// <remarks>This method is solely used in Neural Hasher algorithm implementation. Use Hash method to get properly hashed fingerprints, which can be stored in available data source implementation</remarks>
        Task<List<Fingerprint>> Fingerprint();
        
        /// <summary>
        /// Fingerprint and hash the source
        /// </summary>
        /// <returns>Hashed fingerprints which can be stored in the data source</returns>
        Task<List<HashedFingerprint>> Hash();
    }
}