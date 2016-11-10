namespace SoundFingerprinting.Command
{
    using System.Threading.Tasks;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Query;

    public interface IQuerySource
    {
        /// <summary>
        /// Source is an audio file
        /// </summary>
        /// <param name="pathToAudioFile">Full path to audio file</param>
        /// <returns>Configuration selector</returns>
        IWithQueryAndFingerprintConfiguration From(string pathToAudioFile);

        /// <summary>
        /// Source is an audio file with parametrized <paramref name="startAtSecond"/> and <paramref name="secondsToProcess"/>
        /// </summary>
        /// <param name="pathToAudioFile">Full path to audio file</param>
        /// <param name="secondsToProcess">Total number of seconds to fingerprint for querying</param>
        /// <param name="startAtSecond">Start at second</param>
        /// <returns>Configuration selector</returns>
        IWithQueryAndFingerprintConfiguration From(string pathToAudioFile, int secondsToProcess, int startAtSecond);

        /// <summary>
        /// Source is an audio samples object
        /// </summary>
        /// <param name="audioSamples">Audio samples to build the fingerprints from</param>
        /// <returns>Configuration selector</returns>
        IWithQueryAndFingerprintConfiguration From(AudioSamples audioSamples);
    }
}