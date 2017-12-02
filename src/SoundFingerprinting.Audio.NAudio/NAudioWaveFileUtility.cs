namespace SoundFingerprinting.Audio.NAudio
{
    public class NAudioWaveFileUtility : IWaveFileUtility
    {
        private const int Mono = 1;
        private readonly INAudioFactory factory;

        public NAudioWaveFileUtility() : this(new NAudioFactory())
        {
            // no op
        }

        internal NAudioWaveFileUtility(INAudioFactory factory)
        {
            this.factory = factory;
        }

        public void WriteSamplesToFile(float[] samples, int sampleRate, string destination)
        {
            using (var writer = factory.GetWriter(destination, sampleRate, Mono))
            {
                writer.WriteSamples(samples, 0, samples.Length);
            }
        }

        public void RecodeFileToMonoWave(string source, string destination, int sampleRate, int resamplerQuality)
        {
            using (var stream = factory.GetStream(source))
            {
                using (var resampler = factory.GetResampler(stream, sampleRate, Mono, resamplerQuality))
                {
                    factory.CreateWaveFile(destination, resampler);
                }
            }
        }
    }
}