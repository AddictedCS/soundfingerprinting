namespace SoundFingerprinting.Audio
{
    using System.Collections.Generic;

    /// <summary>
    ///  Audio service used to read and resample audio from a datasource
    /// </summary>
    public interface IAudioService
    {
        /// <summary>
        /// Gets list of supported audio formats (i.e. wav, mp3)
        /// </summary>
        IReadOnlyCollection<string> SupportedFormats { get; }

        /// <summary>
        ///  Reads mono samples from an audio file at provided sample rate
        /// </summary>
        /// <param name="pathToSourceFile">Path to audio file</param>
        /// <param name="sampleRate">Target sample rate</param>
        /// <param name="seconds">Seconds to read</param>
        /// <param name="startAt">Start at second</param>
        /// <returns>Audio sample</returns>
        AudioSamples ReadMonoSamplesFromFile(string pathToSourceFile, int sampleRate, double seconds, double startAt);

        /// <summary>
        ///  Read mono samples from file (full file)
        /// </summary>
        /// <param name="pathToSourceFile">Path to audio source to read from</param>
        /// <param name="sampleRate">Target sample rate</param>
        /// <returns>Audio samples</returns>
        AudioSamples ReadMonoSamplesFromFile(string pathToSourceFile, int sampleRate);

        /// <summary>
        ///  Returns a usually accurate estimate for the length of the file
        /// </summary>
        /// <param name="pathToSourceFile">Path to audio source to get it's length from</param>
        /// <returns>Length in seconds</returns>
        float GetLengthInSeconds(string pathToSourceFile);
    }
}