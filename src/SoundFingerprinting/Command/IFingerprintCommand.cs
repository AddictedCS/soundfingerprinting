namespace SoundFingerprinting.Command
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;

    public interface IFingerprintCommand
    {
        /// <summary>
        ///  Gets the configuration object used during fingerprinting audio source
        /// </summary>
        FingerprintConfiguration FingerprintConfiguration { get; }
 
        /// <summary>
        ///   Fingerprint the audio source
        /// </summary>
        /// <returns>Hashed fingerprints which can be stored in the data source</returns>
        Task<List<HashedFingerprint>> Hash();
    }
}