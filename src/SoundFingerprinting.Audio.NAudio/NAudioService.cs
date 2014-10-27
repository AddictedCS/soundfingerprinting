namespace SoundFingerprinting.Audio.NAudio
{
    using System.Collections.Generic;

    using global::NAudio.Wave;

    using global::NAudio.Wave.SampleProviders;

    using SoundFingerprinting.Infrastructure;

    public class NAudioService : IAudioService
    {
        private const int Mono = 1;

        private static readonly IReadOnlyCollection<string> NAudioSupportedFormats = new[] { ".mp3", ".wav" };

        private readonly ISamplesAggregator samplesAggregator;

        private readonly INAudioFactory naudioFactory;

        public NAudioService()
            : this(DependencyResolver.Current.Get<ISamplesAggregator>(), DependencyResolver.Current.Get<INAudioFactory>())
        {
            // no op
        }

        internal NAudioService(ISamplesAggregator samplesAggregator, INAudioFactory naudioFactory)
        {
            this.samplesAggregator = samplesAggregator;
            this.naudioFactory = naudioFactory;
        }

        public IReadOnlyCollection<string> SupportedFormats
        {
            get
            {
                return NAudioSupportedFormats;
            }
        }

        public float[] ReadMonoSamplesFromFile(string pathToSourceFile, int sampleRate)
        {
            return ReadMonoSamplesFromFile(pathToSourceFile, sampleRate, seconds: 0, startAt: 0);
        }

        public float[] ReadMonoSamplesFromFile(string pathToSourceFile, int sampleRate, int seconds, int startAt)
        {
            return ReadMonoFromSource(pathToSourceFile, sampleRate, seconds, startAt);
        }

        public float[] ReadMonoSamplesFromStreamingUrl(string streamingUrl, int sampleRate, int secondsToDownload)
        {
            // When reading directly from URL NAudio 1.7.1 disregards Mono resampler parameter, thus reading stereo samples
            // End result has to be converted to Mono in order to comply to interface requirements
            // The issue has been addressed here: http://stackoverflow.com/questions/22385783/aac-stream-resampled-incorrectly though not yet resolved
            float[] stereoSamples = ReadMonoFromSource(streamingUrl, sampleRate, secondsToDownload * 2 /*for stereo request twice as much data as for mono*/, startAtSecond: 0);
            return ConvertStereoSamplesToMono(stereoSamples);
        }
      
        public void RecodeFileToMonoWave(string pathToFile, string pathToRecodedFile, int sampleRate)
        {
            using (var stream = naudioFactory.GetStream(pathToFile))
            {
                using (var resampler = naudioFactory.GetResampler(stream, sampleRate, Mono))
                {
                    naudioFactory.CreateWaveFile(pathToRecodedFile, resampler);
                }
            }
        }

        private float[] ReadMonoFromSource(string pathToSource, int sampleRate, int secondsToRead, int startAtSecond)
        {
            using (var stream = naudioFactory.GetStream(pathToSource))
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

        private float[] ConvertStereoSamplesToMono(IList<float> stereoSamples)
        {
            float[] monoSamples = new float[stereoSamples.Count / 2];
            for (int i = 0; i < stereoSamples.Count; i += 2)
            {
                float sum = stereoSamples[i] + stereoSamples[i + 1];
                if (sum > short.MaxValue)
                {
                    sum = short.MaxValue;
                }

                monoSamples[i / 2] = sum / 2;
            }

            return monoSamples;
        }
    }
}
