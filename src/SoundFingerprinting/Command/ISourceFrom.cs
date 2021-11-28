namespace SoundFingerprinting.Command
{
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Content;
    using SoundFingerprinting.Data;

    /// <summary>
    ///   Source object which allows you to select the source to build the fingerprints from.
    /// </summary>
    public interface ISourceFrom
    {
        /// <summary>
        ///   Build fingerprints from a file.
        /// </summary>
        /// <param name="file">Full path to content file.</param>
        /// <param name="mediaType">Media type to generate hashes for.</param>
        /// <returns>Configuration selector object.</returns>
        IWithFingerprintConfiguration From(string file, MediaType mediaType = MediaType.Audio);
        
        /// <summary>
        ///   Build fingerprints from a file.
        /// </summary>
        /// <param name="file">Full path to content file.</param>
        /// <param name="secondsToProcess">Number of seconds to process.</param>
        /// <param name="startAtSecond">Start at second.</param>
        /// <param name="mediaType">Media type to generate hashes for.</param>
        /// <returns>Configuration selector object.</returns>
        IWithFingerprintConfiguration From(string file, double secondsToProcess, double startAtSecond, MediaType mediaType = MediaType.Audio);

        /// <summary>
        ///   Build fingerprints directly from audio samples.
        /// </summary>
        /// <param name="audioSamples">Audio samples to build the fingerprints from.</param>
        /// <returns>Configuration selector object.</returns>
        /// <remarks>
        ///  Returned <see cref="AVHashes"/> will contain only audio hashes <see cref="AVHashes.Audio"/>.
        /// </remarks>
        IWithFingerprintConfiguration From(AudioSamples audioSamples);

        /// <summary>
        ///  Build fingerprints directly from video frames.
        /// </summary>
        /// <param name="frames">Frames to build the fingerprints from.</param>
        /// <returns>Configuration selector object.</returns>
        /// <remarks>
        ///  Returned <see cref="AVHashes"/> will contain only audio hashes <see cref="AVHashes.Video"/>.
        /// </remarks>
        IWithFingerprintConfiguration From(Frames frames);

        /// <summary>
        ///  Build fingerprints from an instance of <see cref="AVTrack"/>.
        /// </summary>
        /// <param name="avTrack">Audio/Video track.</param>
        /// <returns>Configuration selector object.</returns>
        IWithFingerprintConfiguration From(AVTrack avTrack);
    }
}