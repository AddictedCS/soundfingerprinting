namespace SoundFingerprinting.Video
{
    using SoundFingerprinting.Data;
    
    /// <summary>
    ///  Video service interface that is able to read video track from the source.
    /// </summary>
    public interface IVideoService
    {
        /// <summary>
        ///  Reads frames from underlying file.
        /// </summary>
        /// <param name="pathToFile">Path to media file.</param>
        /// <param name="configuration">Video track read configuration.</param>
        /// <returns>Instance of the <see cref="Frames"/> class, read from the file.</returns>
        Frames ReadFramesFromFile(string pathToFile, VideoTrackReadConfiguration configuration);

        /// <summary>
        ///  Reads frames from underlying file.
        /// </summary>
        /// <param name="pathToFile">Path to media file.</param>
        /// <param name="configuration">Video track read configuration.</param>
        /// <param name="seconds">Seconds to read.</param>
        /// <param name="startsAt">Start at second.</param>
        /// <returns>Instance of the <see cref="Frames"/> class, read from the file.</returns>
        Frames ReadFramesFromFile(string pathToFile, VideoTrackReadConfiguration configuration, double seconds, double startsAt);
    }
}