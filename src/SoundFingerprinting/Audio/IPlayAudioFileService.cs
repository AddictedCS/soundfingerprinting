namespace SoundFingerprinting.Audio
{
    public interface IPlayAudioFileService
    {
        /// <summary>
        ///   Play file
        /// </summary>
        /// <param name = "filename">Filename</param>
        /// <returns>Identifier for currently playing stream</returns>
        object PlayFile(string filename);

        /// <summary>
        /// Stop playing stream
        /// </summary>
        /// <param name="stream">Identifier of the stream to be stopped</param>
        void StopPlayingFile(object stream);
    }
}
