namespace SoundFingerprinting.Audio.NAudio.Play
{
    using global::NAudio.Wave;

    internal class NAudioPlayAudioFactory : INAudioPlayAudioFactory
    {
        public IWavePlayer CreateNewWavePlayer()
        {
            return new WaveOut();
        }

        public WaveStream CreateNewStreamFromFilename(string fileName)
        {
            var reader = new MediaFoundationReader(fileName);
            return new WaveChannel32(reader);
        }
    }
}