namespace SoundFingerprinting.Audio.NAudio.Play
{
    using SoundFingerprinting.Infrastructure;

    public class NAudioPlayAudioFileService : IPlayAudioFileService
    {
        private readonly INAudioPlayAudioFactory audioFactory;

        public NAudioPlayAudioFileService()
            : this(DependencyResolver.Current.Get<INAudioPlayAudioFactory>())
        {
        }

        private NAudioPlayAudioFileService(INAudioPlayAudioFactory audioFactory)
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
            var attributes = playFileAttributes as PlayFileAttributes;
            if (attributes != null)
            {
                attributes.WavePlayer.Stop();
                attributes.WaveStream.Close();
                attributes.WavePlayer.Dispose();
                attributes.WaveStream.Dispose();
            }
        }
    }
}
