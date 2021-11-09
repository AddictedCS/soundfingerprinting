namespace SoundFingerprinting.Command
{
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Data;

    /// <summary>
    ///    Source object which allows you to select the source to build the fingerprints from.
    /// </summary>
    public interface ISourceFrom
    {
        /// <summary>
        ///   Build fingerprints from a file.
        /// </summary>
        /// <param name="file">Full path to content file.</param>
        /// <returns>Configuration selector object.</returns>
        IWithFingerprintConfiguration From(string file);

        /// <summary>
        ///   Build fingerprints directly from audio samples.
        /// </summary>
        /// <param name="audioSamples">Audio samples to build the fingerprints from.</param>
        /// <returns>Configuration selector object.</returns>
        IWithFingerprintConfiguration From(AudioSamples audioSamples);

        /// <summary>
        ///  Build fingerprints directly from video frames.
        /// </summary>
        /// <param name="frames">Frames to build the fingerprints from.</param>
        /// <returns>Configuration selector object.</returns>
        IWithFingerprintConfiguration From(Frames frames);

        /// <summary>
        ///   Build fingerprints from a file.
        /// </summary>
        /// <param name="file">Full path to content file.</param>
        /// <param name="secondsToProcess">Number of seconds to process.</param>
        /// <param name="startAtSecond">Start at second.</param>
        /// <returns>Configuration selector object.</returns>
        IWithFingerprintConfiguration From(string file, double secondsToProcess, double startAtSecond);
    }
}