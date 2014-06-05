namespace SoundFingerprinting.Audio.NAudio
{
    using System.Collections.Concurrent;
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

        public bool IsRecordingSupported
        {
            get
            {
                return true;
            }
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
            return ReadMonoFromSource(streamingUrl, sampleRate, secondsToDownload, startAtSecond: 0);
        }

        public float[] ReadMonoSamplesFromMicrophone(int sampleRate, int secondsToRecord)
        {
            var producer = new BlockingCollection<float[]>();
            var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, Mono);
            float[] samples;
            using (var waveIn = new WaveInEvent { WaveFormat = waveFormat })
            {
                waveIn.DataAvailable += (sender, e) =>
                    {
                        var chunk = SamplesConverter.GetFloatSamplesFromByte(e.BytesRecorded, e.Buffer);
                        producer.Add(chunk);
                    };

                waveIn.RecordingStopped += (sender, args) => producer.CompleteAdding();

                waveIn.StartRecording();

                samples = samplesAggregator.ReadSamplesFromSource(
                    new BlockingQueueSamplesProvider(producer), secondsToRecord, sampleRate);

                waveIn.StopRecording();
            }

            return samples;
        }

        public void WriteSamplesToWaveFile(string pathToFile, float[] samples, int sampleRate)
        {
            using (var writer = naudioFactory.GetWriter(pathToFile, sampleRate, Mono))
            {
                writer.WriteSamples(samples, 0, samples.Length);
            }
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
    }
}
