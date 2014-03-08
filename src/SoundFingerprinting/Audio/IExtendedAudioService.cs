namespace SoundFingerprinting.Audio
{
    public interface IExtendedAudioService : IAudioService
    {
        bool IsRecordingSupported { get; }

        /// <summary>
        ///   Play file
        /// </summary>
        /// <param name = "filename">Filename</param>
        /// <returns>Identifier for currently playing stream</returns>
        int PlayFile(string filename);

        /// <summary>
        /// Stop playing stream
        /// </summary>
        /// <param name="stream">Identifier of the stream to be stopped</param>
        void StopPlayingFile(int stream);

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
