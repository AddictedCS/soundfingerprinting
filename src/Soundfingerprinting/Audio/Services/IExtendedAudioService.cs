namespace Soundfingerprinting.Audio.Services
{
    public interface IExtendedAudioService : IAudioService
    {
        /// <summary>
        ///   Play file
        /// </summary>
        /// <param name = "filename">Filename</param>
        int PlayFile(string filename);

        /// <summary>
        /// Stop playing stream
        /// </summary>
        /// <param name="stream"></param>
        void StopPlayingFile(int stream);

        /// <summary>
        ///   Recode the file
        /// </summary>
        /// <param name = "pathToFile">Initial file</param>
        /// <param name = "outputPathToFile">Target file</param>
        /// <param name = "targetSampleRate">Target sample rate</param>
        void RecodeTheFile(string pathToFile, string outputPathToFile, int targetSampleRate);
    }
}
