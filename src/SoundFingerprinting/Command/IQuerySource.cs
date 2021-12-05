namespace SoundFingerprinting.Command
{
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Content;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Media;
    using SoundFingerprinting.Video;

    /// <summary>
    ///  Contract for querying the <see cref="IModelService"/>.
    /// </summary>
    public interface IQuerySource
    {
        /// <summary>
        ///   Build query fingerprints from a file.
        /// </summary>
        /// <param name="file">Full path to content file.</param>
        /// <param name="mediaType">Media type to generate query hashes for.</param>
        /// <returns>Configuration selector.</returns>
        /// <remarks>
        ///  Make sure provided <see cref="IAudioService"/> supports provided file format.
        ///  If you provide MediaType.Video flag for <paramref name="mediaType"/> makes sure to provide <see cref="IVideoService"/> or <see cref="IMediaService"/> at <see cref="IUsingFingerprintServices"/> method builder to be able to read <see cref="Frames"/> from the provided file.
        /// </remarks>
        IWithQueryConfiguration From(string file, MediaType mediaType = MediaType.Audio);

        /// <summary>
        ///   Build query fingerprints an audio file with parametrized <paramref name="startAtSecond"/> and <paramref name="secondsToProcess"/>.
        /// </summary>
        /// <param name="file">Full path to audio file.</param>
        /// <param name="secondsToProcess">Total number of seconds to fingerprint for querying.</param>
        /// <param name="startAtSecond">Start at second.</param>
        /// <param name="mediaType">Media type to generate query hashes for.</param>
        /// <returns>Configuration selector.</returns>
        /// <remarks>
        ///  Make sure provided <see cref="IAudioService"/> supports provided file format.
        ///  If you provide MediaType.Video flag for <paramref name="mediaType"/> makes sure to provide <see cref="IVideoService"/> or <see cref="IMediaService"/> at <see cref="IUsingFingerprintServices"/> method builder to be able to read <see cref="Frames"/> from the provided file.
        /// </remarks>
        IWithQueryConfiguration From(string file, double secondsToProcess, double startAtSecond, MediaType mediaType = MediaType.Audio);

        /// <summary>
        ///   Build query fingerprints from an audio samples object.
        /// </summary>
        /// <param name="audioSamples">Audio samples to build the fingerprints from.</param>
        /// <returns>Configuration selector.</returns>
        /// <remarks>
        ///  Make sure provided audio samples object adheres to algorithm configuration parameters (i.e., SampleRate has to be 5512).
        /// </remarks>
        IWithQueryConfiguration From(AudioSamples audioSamples);

        /// <summary>
        ///  Build query fingerprints from frames.
        /// </summary>
        /// <param name="frames">Video frames to build fingerprints from.</param>
        /// <returns>Configuration selector.</returns>
        IWithQueryConfiguration From(Frames frames);

        /// <summary>
        ///  Build query fingerprints from an instance of <see cref="AVTrack"/> class.
        /// </summary>
        /// <param name="avTrack">AVTrack to build fingerprints from.</param>
        /// <returns>Configuration selector.</returns>
        IWithQueryConfiguration From(AVTrack avTrack);

        /// <summary>
        ///   Create query from previously created fingerprints.
        /// </summary>
        /// <param name="hashes">Previously created fingerprints.</param>
        /// <returns>Configuration selector.</returns>
        /// <remarks>
        ///  All the configuration options related to fingerprint creation will be disregarded.
        /// </remarks>
        IWithQueryConfiguration From(AVHashes hashes);
    }
}