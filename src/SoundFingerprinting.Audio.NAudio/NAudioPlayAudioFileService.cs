namespace SoundFingerprinting.Audio.NAudio
{
    using global::NAudio.Wave;

    public class NAudioPlayAudioFileService : IPlayAudioFileService
    {
        public object PlayFile(string pathToFile)
        {
            var waveOut = new WaveOut();
            var waveStream = CreateInputStream(pathToFile);
            waveOut.Init(waveStream);
            waveOut.Play();
            return new PlayFileAttributes(waveOut, waveStream);
        }

        public void StopPlayingFile(object playFileAttributes)
        {
            var attributes = playFileAttributes as PlayFileAttributes;
            if (attributes != null)
            {
                attributes.WaveOut.Stop();
                attributes.WaveStream.Close();
            }
        }

        private WaveStream CreateInputStream(string fileName)
        {
            var mp3Reader = new Mp3FileReader(fileName);
            return new WaveChannel32(mp3Reader);
        }
    }
}
