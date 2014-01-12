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
        /// <param name = "outputPathToFile">Target file</param>
        /// <param name = "targetSampleRate">Target sample rate</param>
        void RecodeFileToMonoWave(string pathToFile, string outputPathToFile, int targetSampleRate);

        float[] ReadMonoFromUrl(string urlToResource, int sampleRate, int secondsToDownload);

        float[] RecordFromMicrophoneToFile(string pathToFile, int sampleRate, int secondsToRecord);
    }
}
