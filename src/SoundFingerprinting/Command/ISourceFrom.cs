namespace SoundFingerprinting.Command
{
    using SoundFingerprinting.Audio;

    /// <summary>
    ///    Source object which allows you to select the source to build the fingerprints from
    /// </summary>
    public interface ISourceFrom
    {
        /// <summary>
        ///   Build fingerprints from an audio file
        /// </summary>
        /// <param name="pathToAudioFile">Full path to audio file</param>
        /// <returns>Configuration selector object</returns>
        IWithFingerprintConfiguration From(string pathToAudioFile);

        /// <summary>
        ///   Build fingerprints directly from audio samples. This is a handy method to build the fingerprints from sources like microphone or streaming URL
        /// </summary>
        /// <param name="audioSamples">Audio samples to build the fingerprints from</param>
        /// <returns>Configuration selector object</returns>
        IWithFingerprintConfiguration From(AudioSamples audioSamples);

        /// <summary>
        ///   Build fingerprints from an audio file
        /// </summary>
        /// <param name="pathToAudioFile">Full path to audio file</param>
        /// <param name="secondsToProcess">Number of seconds to process</param>
        /// <param name="startAtSecond">Start at second</param>
        /// <returns>Configuration selector object</returns>
        IWithFingerprintConfiguration From(string pathToAudioFile, double secondsToProcess, double startAtSecond);
    }
}