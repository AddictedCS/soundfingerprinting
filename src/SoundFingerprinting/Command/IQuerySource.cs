namespace SoundFingerprinting.Command
{
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Data;

    /// <summary>
    ///  Contract for querying the <see cref="IModelService"/>.
    /// </summary>
    public interface IQuerySource
    {
        /// <summary>
        ///   Source is a file.
        /// </summary>
        /// <param name="pathToAudioFile">Full path to the file.</param>
        /// <returns>Configuration selector.</returns>
        /// <remarks>
        ///  Make sure provided <see cref="IAudioService"/> supports provided file format.
        /// </remarks>
        IWithQueryConfiguration From(string pathToAudioFile);

        /// <summary>
        ///   Source is an audio file with parametrized <paramref name="startAtSecond"/> and <paramref name="secondsToProcess"/>.
        /// </summary>
        /// <param name="pathToAudioFile">Full path to audio file.</param>
        /// <param name="secondsToProcess">Total number of seconds to fingerprint for querying.</param>
        /// <param name="startAtSecond">Start at second.</param>
        /// <returns>Configuration selector.</returns>
        /// <remarks>
        ///  Make sure provided <see cref="IAudioService"/> supports provided file format.
        /// </remarks>
        IWithQueryConfiguration From(string pathToAudioFile, double secondsToProcess, double startAtSecond);

        /// <summary>
        ///   Source is an audio samples object.
        /// </summary>
        /// <param name="audioSamples">Audio samples to build the fingerprints from.</param>
        /// <returns>Configuration selector.</returns>
        /// <remarks>
        ///  Make sure provided audio samples object adheres to algorithm configuration parameters (i.e., SampleRate has to be 5512).
        /// </remarks>
        IWithQueryConfiguration From(AudioSamples audioSamples);

        /// <summary>
        ///   Create query from previously created fingerprints.
        /// </summary>
        /// <param name="hashes">Previously created fingerprints.</param>
        /// <returns>Configuration selector. Keep in mind that all the configuration options related to fingerprint creation will be disregarded.</returns>
        IWithQueryConfiguration From(Hashes hashes);
    }
}