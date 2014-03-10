namespace SoundFingerprinting.Audio.NAudio
{
    using global::NAudio.Wave;

    public class PlayFileAttributes
    {
        public PlayFileAttributes(WaveOut waveOut, WaveStream waveStream)
        {
            WaveOut = waveOut;
            WaveStream = waveStream;
        }

        public WaveOut WaveOut { get; private set; }

        public WaveStream WaveStream { get; private set; }
    }
}
