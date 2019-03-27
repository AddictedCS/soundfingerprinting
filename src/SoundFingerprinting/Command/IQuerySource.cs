namespace SoundFingerprinting.Command
{
    using System.Collections.Generic;
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Data;

    public interface IQuerySource
    {
        /// <summary>
        ///   Source is an audio file
        /// </summary>
        /// <param name="pathToAudioFile">Full path to audio file</param>
        /// <returns>Configuration selector</returns>
        IWithQueryConfiguration From(string pathToAudioFile);

        /// <summary>
        ///   Source is an audio file with parametrized <paramref name="startAtSecond"/> and <paramref name="secondsToProcess"/>
        /// </summary>
        /// <param name="pathToAudioFile">Full path to audio file</param>
        /// <param name="secondsToProcess">Total number of seconds to fingerprint for querying</param>
        /// <param name="startAtSecond">Start at second</param>
        /// <returns>Configuration selector</returns>
        IWithQueryConfiguration From(string pathToAudioFile, double secondsToProcess, double startAtSecond);

        /// <summary>
        ///   Source is an audio samples object
        /// </summary>
        /// <param name="audioSamples">Audio samples to build the fingerprints from</param>
        /// <returns>Configuration selector</returns>
        IWithQueryConfiguration From(AudioSamples audioSamples);

        /// <summary>
        ///   Create query from previously created fingerprints
        /// </summary>
        /// <param name="hashedFingerprints">List of fingerprints</param>
        /// <returns>Configuration selector. Keep in mind that all the configuration options related to fingerprint creation will be disregarded.</returns>
        IWithQueryConfiguration From(IEnumerable<HashedFingerprint> hashedFingerprints);
    }
}