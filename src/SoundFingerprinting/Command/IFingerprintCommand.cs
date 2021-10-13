namespace SoundFingerprinting.Command
{
    using System.Threading.Tasks;

    using SoundFingerprinting.Data;

    /// <summary>
    ///  Fingerprint command contract.
    /// </summary>
    public interface IFingerprintCommand
    {
        /// <summary>
        ///   Generate hashes.
        /// </summary>
        /// <returns>Hashed fingerprints which can be stored in the data source.</returns>
        Task<Hashes> Hash();
    }
}