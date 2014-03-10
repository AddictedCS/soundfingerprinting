namespace SoundFingerprinting.Audio
{
    using System;
    using System.Collections.Generic;

    public interface IAudioService : IDisposable
    {
        bool IsRecordingSupported { get; }

        /// <summary>
        /// Gets the list of supported audio formats
        /// </summary>
        IReadOnlyCollection<string> SupportedFormats { get; }

        /// <summary>
        ///   Read audio from file at a specific frequency rate
        /// </summary>
        /// <param name = "pathToSourceFile">Filename to read from</param>
        /// <param name = "sampleRate">Sample rate</param>
        /// <param name = "seconds">Number of seconds to read</param>
        /// <param name = "startAt">Start reading at a specific second</param>
        /// <returns>Array with audio samples</returns>
        float[] ReadMonoFromFile(string pathToSourceFile, int sampleRate, int seconds, int startAt);

        /// <summary>
        ///   Read data from file
        /// </summary>
        /// <param name = "pathToSourceFile">Filename to read from</param>
        /// <param name = "sampleRate">Sample rate at which to read the file</param>
        /// <returns>Array with data</returns>
        float[] ReadMonoFromFile(string pathToSourceFile, int sampleRate);

        /// <summary>
        ///   Recode the file
        /// </summary>
        /// <param name = "pathToFile">Initial file</param>
        /// <param name = "pathToRecodedFile">Target file</param>
        /// <param name = "sampleRate">Target sample rate</param>
        void RecodeFileToMonoWave(string pathToFile, string pathToRecodedFile, int sampleRate);

        float[] ReadMonoFromUrlToFile(string streamUrl, string pathToFile, int sampleRate, int secondsToDownload);

        float[] ReadMonoFromMicrophoneToFile(string pathToFile, int sampleRate, int secondsToRecord);
    }
}