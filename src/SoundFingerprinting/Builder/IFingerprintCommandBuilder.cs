namespace SoundFingerprinting.Builder
{
    using SoundFingerprinting.Command;

    public interface IFingerprintCommandBuilder
    {
        /// <summary>
        /// Start building the fingerprinting command
        /// </summary>
        /// <returns>SourceFrom object which allows you to select the source to build the fingerprints from</returns>
        ISourceFrom BuildFingerprintCommand();
    }
}