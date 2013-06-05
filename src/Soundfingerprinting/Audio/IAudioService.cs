namespace Soundfingerprinting.Audio
{
    using System;

    public interface IAudioService : IDisposable
    {
        /// <summary>
        ///   Read audio from file at a specific frequency rate
        /// </summary>
        /// <param name = "pathToFile">Filename to read from</param>
        /// <param name = "sampleRate">Sample rate</param>
        /// <param name = "millisecondsToRead">Milliseconds to read</param>
        /// <param name = "startAtMillisecond">Start at a specific millisecond</param>
        /// <returns>Array with data samples</returns>
        float[] ReadMonoFromFile(string pathToFile, int sampleRate, int millisecondsToRead, int startAtMillisecond);

        /// <summary>
        ///   Read data from file
        /// </summary>
        /// <param name = "pathToFile">Filename to read from</param>
        /// <param name = "sampleRate">Sample rate at which to read the file</param>
        /// <returns>Array with data</returns>
        float[] ReadMonoFromFile(string pathToFile, int sampleRate);
    }
}