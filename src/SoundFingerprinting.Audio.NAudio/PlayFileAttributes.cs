namespace SoundFingerprinting.Audio.NAudio
{
    using global::NAudio.Wave;

    public class PlayFileAttributes
    {
        public PlayFileAttributes(IWavePlayer wavePlayer, WaveStream waveStream)
        {
            WavePlayer = wavePlayer;
            WaveStream = waveStream;
        }

        public IWavePlayer WavePlayer { get; private set; }

        public WaveStream WaveStream { get; private set; }
    }
}
