namespace SoundFingerprinting.Command
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using SoundFingerprinting.Data;

    public interface IFingerprintCommand
    {
        /// <summary>
        ///   Fingerprint the audio source
        /// </summary>
        /// <returns>Hashed fingerprints which can be stored in the data source</returns>
        Task<List<HashedFingerprint>> Hash();
    }
}