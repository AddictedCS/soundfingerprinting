namespace SoundFingerprinting.Audio.NAudio
{
    using SoundFingerprinting.Infrastructure;

    public class NAudioWaveFileUtility : IWaveFileUtility
    {
        private const int Mono = 1;
        private readonly INAudioFactory naudioFactory;

        public NAudioWaveFileUtility() : this(DependencyResolver.Current.Get<INAudioFactory>())
        {
            // no op
        }

        internal NAudioWaveFileUtility(INAudioFactory naudioFactory)
        {
            this.naudioFactory = naudioFactory;
        }

        public void WriteSamplesToFile(float[] samples, int sampleRate, string destination)
        {
            using (var writer = naudioFactory.GetWriter(destination, sampleRate, Mono))
            {
                writer.WriteSamples(samples, 0, samples.Length);
            }
        }
    }
}