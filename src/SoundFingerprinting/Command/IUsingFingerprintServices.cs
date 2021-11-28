namespace SoundFingerprinting.Command
{
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Media;
    using SoundFingerprinting.Video;

    /// <summary>
    /// Fingerprint services selector.
    /// </summary>
    public interface IUsingFingerprintServices : IFingerprintCommand
    {
        /// <summary>
        ///  Sets the audio service used in fingerprinting the source.
        /// </summary>
        /// <param name="audioService">Audio service used to read <see cref="AudioSamples"/> from the <see cref="ISourceFrom"/>.</param>
        /// <returns>Fingerprint command.</returns>
        /// <remarks>
        ///  Setting audio service only will allow generating hashes only from audio tracks.
        /// </remarks>
        IFingerprintCommand UsingServices(IAudioService audioService);
 
        /// <summary>
        ///  Sets the video services used in fingerprinting the source.
        /// </summary>
        /// <param name="videoService">Video service used to read <see cref="Frames"/> from the <see cref="ISourceFrom"/>.</param>
        /// <returns>Fingerprint command.</returns>
        IFingerprintCommand UsingServices(IVideoService videoService);

        /// <summary>
        ///  Sets media service used in fingerprinting the source.
        /// </summary>
        /// <param name="mediaService">Instance of <see cref="IMediaService"/> implementing class, that will be used to read either <see cref="AudioSamples"/> or <see cref="Frames"/> or both from the <see cref="ISourceFrom"/>.</param>
        /// <returns>Fingerprint command.</returns>
        /// <remarks>
        ///  Setting an implementation of <see cref="IMediaService"/> will allow generating hashes for both audio and video tracks.
        /// </remarks>
        IFingerprintCommand UsingServices(IMediaService mediaService);
    }
}