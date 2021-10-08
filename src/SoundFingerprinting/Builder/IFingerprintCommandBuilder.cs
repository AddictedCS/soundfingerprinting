namespace SoundFingerprinting.Builder
{
    using SoundFingerprinting.Command;
    
    /// <summary>
    ///  Fingerprinting command builder.
    /// </summary>
    public interface IFingerprintCommandBuilder
    {
        /// <summary>
        ///   Start building fingerprinting command.
        /// </summary>
        /// <returns>Source selector.</returns>
        ISourceFrom BuildFingerprintCommand();
    }
}