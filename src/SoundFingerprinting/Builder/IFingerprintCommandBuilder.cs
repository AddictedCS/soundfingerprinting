namespace SoundFingerprinting.Builder
{
    using SoundFingerprinting.Command;
    
    /// <summary>
    ///  Acoustic fingerprinting command builder
    /// </summary>
    public interface IFingerprintCommandBuilder
    {
        /// <summary>
        ///   Start building acoustic fingerprinting command
        /// </summary>
        /// <returns>Source selector</returns>
        ISourceFrom BuildFingerprintCommand();
    }
}