namespace SoundFingerprinting.Audio
{
    using System.Collections.Generic;

    public interface IAudioService
    {
        IReadOnlyCollection<string> SupportedFormats { get; }

        float[] ReadMonoSamplesFromFile(string pathToSourceFile, int sampleRate, int seconds, int startAt);

        float[] ReadMonoSamplesFromFile(string pathToSourceFile, int sampleRate);
    }

    /// <summary>
    /// Reader which allows reading audio stream directly from a network stream
    /// </summary>
    /// <example>E.g of a streaming address: http://92.115.237.34:8000/maestro.m3u</example>
    public interface IStreamingUrlReader
    {
        /// <summary>
        /// Read mono samples from streaming url
        /// </summary>
        /// <param name="url">URL to read from</param>
        /// <param name="sampleRate">Target sample rate</param>
        /// <param name="secondsToRead">Seconds to read</param>
        /// <returns>32 bit samples array</returns>
        float[] ReadMonoSamples(string url, int sampleRate, int secondsToRead);
    }
}