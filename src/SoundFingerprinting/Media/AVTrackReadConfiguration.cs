namespace SoundFingerprinting.Media
{
    using SoundFingerprinting.Video;

    /// <summary>
    ///  Class that contains all the required properties to the <see cref="IMediaService"/> to read both audio and video tracks.
    /// </summary>
    public class AVTrackReadConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AVTrackReadConfiguration"/> class.
        /// </summary>
        /// <param name="audioConfig">Audio track read configuration.</param>
        /// <param name="videoConfig">Video track read configuration.</param>
        public AVTrackReadConfiguration(AudioTrackReadConfiguration audioConfig, VideoTrackReadConfiguration videoConfig)
        {
            AudioConfig = audioConfig;
            VideoConfig = videoConfig;
        }

        /// <summary>
        ///  Gets audio track read configuration.
        /// </summary>
        public AudioTrackReadConfiguration AudioConfig { get; }

        /// <summary>
        ///  Gets video track read configuration.
        /// </summary>
        public VideoTrackReadConfiguration VideoConfig { get; }
    }
}