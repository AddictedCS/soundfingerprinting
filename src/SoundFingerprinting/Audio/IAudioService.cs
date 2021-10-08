namespace SoundFingerprinting.Audio
{
    using System.Collections.Generic;

    /// <summary>
    ///  Audio service used to read and resample audio from a data source.
    /// </summary>
    public interface IAudioService
    {
        /// <summary>
        /// Gets list of supported audio formats.
        /// </summary>
        IReadOnlyCollection<string> SupportedFormats { get; }

        /// <summary>
        ///  Reads mono samples from an audio file.
        /// </summary>
        /// <param name="pathToSourceFile">Path to audio file.</param>
        /// <param name="sampleRate">Target sample rate.</param>
        /// <param name="seconds">Seconds to read.</param>
        /// <param name="startAt">Start at second.</param>
        /// <returns>Instance of <see cref="AudioSamples"/>.</returns>
        AudioSamples ReadMonoSamplesFromFile(string pathToSourceFile, int sampleRate, double seconds, double startAt);

        /// <summary>
        ///  Read mono samples from an audio file.
        /// </summary>
        /// <param name="pathToSourceFile">Path to audio source to read from.</param>
        /// <param name="sampleRate">Target sample rate.</param>
        /// <returns>Audio samples.</returns>
        AudioSamples ReadMonoSamplesFromFile(string pathToSourceFile, int sampleRate);
        
        /// <summary>
        ///  Gets an estimate for the length of the file, measured in seconds.
        /// </summary>
        /// <param name="file">Path to audio source to get it's length from.</param>
        /// <returns>Length in seconds.</returns>
        float GetLengthInSeconds(string file);
    }
}