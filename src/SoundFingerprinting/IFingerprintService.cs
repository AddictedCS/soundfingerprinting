namespace SoundFingerprinting
{
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;

    public interface IFingerprintService
    {
        /// <summary>
        ///  Create hashes from audio samples
        /// </summary>
        /// <param name="samples">Audio samples</param>
        /// <param name="configuration">Fingerprinting configuration to use when creating hashes</param>
        /// <returns>Instance of Hashes class</returns>
        Hashes CreateFingerprintsFromAudioSamples(AudioSamples samples, FingerprintConfiguration configuration);

        /// <summary>
        ///  Create hashes from frames
        /// </summary>
        /// <param name="imageFrames">Frames (i.e. image frames)</param>
        /// <param name="configuration">Fingerprinting configuration to use when creating hashes</param>
        /// <returns>Instance of Hashes class</returns>
        Hashes CreateFingerprintsFromImageFrames(Frames imageFrames, FingerprintConfiguration configuration);
    }
}