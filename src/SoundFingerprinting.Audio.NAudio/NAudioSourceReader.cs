namespace SoundFingerprinting.Audio.NAudio
{
    using System;

    using global::NAudio.Wave;

    using global::NAudio.Wave.SampleProviders;

    internal class NAudioSourceReader : INAudioSourceReader
    {
        private const int Mono = 1;
 
        private readonly INAudioFactory naudioFactory;
        private readonly ISamplesAggregator samplesAggregator;

        internal NAudioSourceReader(ISamplesAggregator samplesAggregator, INAudioFactory naudioFactory)
        {
            this.samplesAggregator = samplesAggregator;
            this.naudioFactory = naudioFactory;
        }

        public float[] ReadMonoFromSource(string source, int sampleRate, double secondsToRead, double startAtSecond, int resamplerQuality)
        {
            using (var stream = naudioFactory.GetStream(source))
            {
                SeekToSecondInCaseIfRequired(startAtSecond, stream);
                using (var resampler = naudioFactory.GetResampler(stream, sampleRate, Mono, resamplerQuality))
                {
                    var waveToSampleProvider = new WaveToSampleProvider(resampler);
                    return samplesAggregator.ReadSamplesFromSource(new NAudioSamplesProviderAdapter(waveToSampleProvider), secondsToRead, sampleRate);
                }
            }
        }

        private void SeekToSecondInCaseIfRequired(double startAtSecond, WaveStream stream)
        {
            if (startAtSecond > 0)
            {
                stream.CurrentTime = stream.CurrentTime.Add(TimeSpan.FromSeconds(startAtSecond));
            }
        }
    }
}