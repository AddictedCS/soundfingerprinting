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
        /// <returns>An instance of <see cref="AVHashes"/> class.</returns>
        Task<AVHashes> Hash();
    }
}