namespace SoundFingerprinting.Audio.Bass
{
    using Un4seen.Bass.Misc;

    public class BassWaveFileUtility : IWaveFileUtility
    {
        private const int FloatLength = 4;
        private const int BitsPerSample = FloatLength * 8;

        public void WriteSamplesToFile(float[] samples, int sampleRate, string destination)
        {
            var waveWriter = new WaveWriter(
                destination, BassConstants.NumberOfChannels, sampleRate, BitsPerSample, true);
            waveWriter.Write(samples, samples.Length * FloatLength);
            waveWriter.Close();
        }
    }
}