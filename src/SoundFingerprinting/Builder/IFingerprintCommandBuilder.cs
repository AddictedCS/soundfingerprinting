namespace SoundFingerprinting.Builder
{
    using SoundFingerprinting.Command;

    public interface IFingerprintCommandBuilder
    {
        /// <summary>
        /// Start building the fingerprinting command
        /// </summary>
        /// <returns>Source selector</returns>
        ISourceFrom BuildFingerprintCommand();
    }
}