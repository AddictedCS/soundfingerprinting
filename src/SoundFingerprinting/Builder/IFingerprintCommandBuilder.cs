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
        /// <returns>Instance of <see cref="ISourceFrom"/> that allows selecting a source from which the fingerprints will be built.</returns>
        ISourceFrom BuildFingerprintCommand();
    }
}