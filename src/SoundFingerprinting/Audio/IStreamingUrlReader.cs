namespace SoundFingerprinting.Audio
{
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