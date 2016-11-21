namespace SoundFingerprinting.Command
{
    using SoundFingerprinting.Audio;

    /// <summary>
    /// Fingerprint services selector
    /// </summary>
    public interface IUsingFingerprintServices
    {
        /// <summary>
        ///  Sets the audio service used in fingerprinting the source
        /// </summary>
        /// <param name="audioService">Audio service to use while fingerprinting</param>
        /// <returns>Fingerprint command</returns>
        IFingerprintCommand UsingServices(IAudioService audioService);
    }
}