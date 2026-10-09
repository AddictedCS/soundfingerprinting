namespace SoundFingerprinting.Command
{
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Media;
    using SoundFingerprinting.Video;

    /// <summary>
    ///  Contract for services selection during realtime fingerprint command building.
    /// </summary>
    public interface IUsingRealtimeFingerprintServices : IRealtimeFingerprintCommand
    {
        /// <summary>
        ///  Sets the audio service used to read audio samples from the files of the source.
        /// </summary>
        /// <param name="audioService">Audio service.</param>
        /// <returns>Realtime fingerprint command.</returns>
        IRealtimeFingerprintCommand UsingServices(IAudioService audioService);

        /// <summary>
        ///  Sets the video service used to read frames from the files of the source.
        /// </summary>
        /// <param name="videoService">Video service.</param>
        /// <returns>Realtime fingerprint command.</returns>
        IRealtimeFingerprintCommand UsingServices(IVideoService videoService);

        /// <summary>
        ///  Sets the media service used to read audio samples and frames from the files of the source.
        /// </summary>
        /// <param name="mediaService">Media service.</param>
        /// <returns>Realtime fingerprint command.</returns>
        IRealtimeFingerprintCommand UsingServices(IMediaService mediaService);

        /// <summary>
        ///  Sets the realtime media service used to read tracks directly from a URL.
        /// </summary>
        /// <param name="realtimeMediaService">Realtime media service.</param>
        /// <returns>Realtime fingerprint command.</returns>
        IRealtimeFingerprintCommand UsingServices(IRealtimeMediaService realtimeMediaService);
    }
}
