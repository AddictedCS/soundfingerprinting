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
        /// <param name = "secondsToRead">Number of seconds to read</param>
        /// <param name = "startAtSecond">Start reading at a specific second</param>
        /// <returns>Array with audio samples</returns>
        float[] ReadMonoFromFile(string pathToFile, int sampleRate, int secondsToRead, int startAtSecond);

        /// <summary>
        ///   Read data from file
        /// </summary>
        /// <param name = "pathToFile">Filename to read from</param>
        /// <param name = "sampleRate">Sample rate at which to read the file</param>
        /// <returns>Array with data</returns>
        float[] ReadMonoFromFile(string pathToFile, int sampleRate);
    }
}