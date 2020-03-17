namespace SoundFingerprinting.Audio.NAudio.Play
{
    public class NAudioPlayAudioFileService : IPlayAudioFileService
    {
        private readonly INAudioPlayAudioFactory audioFactory;

        public NAudioPlayAudioFileService(): this(new NAudioPlayAudioFactory())
        {
        }

        internal NAudioPlayAudioFileService(INAudioPlayAudioFactory audioFactory)
        {
            this.audioFactory = audioFactory;
        }

        public object PlayFile(string pathToFile)
        {
            var wavePlayer = audioFactory.CreateNewWavePlayer();
            var waveStream = audioFactory.CreateNewStreamFromFilename(pathToFile);
            wavePlayer.Init(waveStream);
            wavePlayer.Play();
            return new PlayFileAttributes(wavePlayer, waveStream);
        }

        public void StopPlayingFile(object playFileAttributes)
        {
            if (playFileAttributes is PlayFileAttributes attributes)
            {
                attributes.WavePlayer.Stop();
                attributes.WaveStream.Close();
                attributes.WavePlayer.Dispose();
                attributes.WaveStream.Dispose();
            }
        }
    }
}
