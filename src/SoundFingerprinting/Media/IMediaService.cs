namespace SoundFingerprinting.Media
{
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Content;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Video;

    /// <summary>
    ///  Media service interface that is able to read either audio or video or both tracks from the source.
    /// </summary>
    public interface IMediaService : IAudioService, IVideoService
    {
        /// <summary>
        ///  Reads audio/video tracks from the file.
        /// </summary>
        /// <param name="pathToFile">Path to file.</param>
        /// <param name="configuration">Audio/Video track read configuration properties.</param>
        /// <param name="mediaType">Media type to read from the file.</param>
        /// <returns>An instance of the <see cref="AVTrack"/> class.</returns>
        /// <remarks>
        ///  You can specify MediaType.Audio to read audio only track. AVTrack will contain AudioTrack in the returned object.
        ///  To read both audio and video tracks you can specify mediaType as flags MediaType.Audio | MediaType.Video.
        /// </remarks>
        AVTrack ReadAVTrackFromFile(string pathToFile, AVTrackReadConfiguration configuration, MediaType mediaType = MediaType.Audio | MediaType.Video);

        /// <summary>
        ///  Reads audio/video tracks from the file.
        /// </summary>
        /// <param name="pathToFile">Path to file.</param>
        /// <param name="configuration">Audio/Video track read configuration properties.</param>
        /// <param name="seconds">Seconds to read.</param>
        /// <param name="startsAt">Start at second.</param>
        /// <param name="mediaType">Media type to read from the file.</param>
        /// <returns>An instance of the <see cref="AVTrack"/> class.</returns>
        /// <remarks>
        ///  You can specify MediaType.Audio to read audio only track. In this case returned <see cref="AVTrack"/> will contain <see cref="AVTrack.Audio"/> track only. <br/>
        ///  To read both audio and video tracks you can specify mediaType as flags MediaType.Audio | MediaType.Video.
        /// </remarks>
        AVTrack ReadAVTrackFromFile(string pathToFile,  AVTrackReadConfiguration configuration, double seconds, double startsAt, MediaType mediaType = MediaType.Audio | MediaType.Video);
    }
}