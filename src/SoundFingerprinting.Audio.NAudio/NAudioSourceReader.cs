namespace SoundFingerprinting.Audio.NAudio
{
    using global::NAudio.Wave;

    using global::NAudio.Wave.SampleProviders;

    using SoundFingerprinting.Infrastructure;

    internal class NAudioSourceReader : INAudioSourceReader
    {
        private const int Mono = 1;
 
        private readonly INAudioFactory naudioFactory;
        private readonly ISamplesAggregator samplesAggregator;

        public NAudioSourceReader()
            : this(DependencyResolver.Current.Get<ISamplesAggregator>(), DependencyResolver.Current.Get<INAudioFactory>())
        {
            // no op
        }

        internal NAudioSourceReader(ISamplesAggregator samplesAggregator, INAudioFactory naudioFactory)
        {
            this.samplesAggregator = samplesAggregator;
            this.naudioFactory = naudioFactory;
        }

        public float[] ReadMonoFromSource(string source, int sampleRate, int secondsToRead, int startAtSecond)
        {
            using (var stream = naudioFactory.GetStream(source))
            {
                SeekToSecondInCaseIfRequired(startAtSecond, stream);
                using (var resampler = naudioFactory.GetResampler(stream, sampleRate, Mono))
                {
                    var waveToSampleProvider = new WaveToSampleProvider(resampler);
                    return
                        samplesAggregator.ReadSamplesFromSource(
                            new NAudioSamplesProviderAdapter(waveToSampleProvider), secondsToRead, sampleRate);
                }
            }
        }

        private void SeekToSecondInCaseIfRequired(int startAtSecond, WaveStream stream)
        {
            if (startAtSecond > 0)
            {
                int actualSampleRate = stream.WaveFormat.SampleRate;
                int bitsPerSample = stream.WaveFormat.BitsPerSample;
                stream.Seek(actualSampleRate * bitsPerSample / 8 * startAtSecond, System.IO.SeekOrigin.Begin);
            }
        }
    }
}