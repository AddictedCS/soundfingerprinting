namespace SoundFingerprinting.Audio
{
    public interface IPlayAudioFileService
    {
        object PlayFile(string pathToFile);

        void StopPlayingFile(object stream);
    }
}
