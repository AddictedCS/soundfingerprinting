namespace SoundFingerprinting.Audio.NAudio
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;

    using global::NAudio.Wave;
    using global::NAudio.Wave.SampleProviders;

    public class NAudioService : AudioService, IExtendedAudioService
    {
        private static readonly IReadOnlyCollection<string> NAudioSupportedFormats = new[] { ".mp3", ".wav" };

        private readonly SamplesAggregator samplesAggregator;

        private IWavePlayer waveOutDevice;
        private WaveStream mainOutputStream;

        public NAudioService()
        {
            samplesAggregator = new SamplesAggregator();
        }

        ~NAudioService()
        {
            Dispose(false);
        }

        public bool IsRecordingSupported
        {
            get
            {
                return true;
            }
        }

        public override IReadOnlyCollection<string> SupportedFormats
        {
            get
            {
                return NAudioSupportedFormats;
            }
        }

        public override float[] ReadMonoFromFile(string pathToFile, int sampleRate, int secondsToRead, int startAtSecond)
        {
            return ReadMonoFromSource(pathToFile, sampleRate, secondsToRead, startAtSecond, GetNextSamples);
        }

        public float[] ReadMonoFromUrlToFile(string streamUrl, string pathToFile, int sampleRate, int secondsToDownload)
        {
            float[] samples = ReadMonoFromSource(streamUrl, sampleRate, secondsToDownload, 0, GetNextStreamingSamples);
            WriteSamplesToFile(pathToFile, WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1), samples);
            return samples;
        }

        public float[] ReadMonoFromMicrophoneToFile(string pathToFile, int sampleRate, int secondsToRecord)
        {
            var producer = new BlockingCollection<float[]>();
            var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1);
            float[] samples;
            using (var waveIn = new WaveInEvent { WaveFormat = waveFormat })
            {
                waveIn.DataAvailable += (sender, e) =>
                    {
                        var chunk = GetFloatSamplesFromByte(e.BytesRecorded, e.Buffer);
                        producer.Add(chunk);
                    };

                waveIn.RecordingStopped += (sender, args) => producer.CompleteAdding();

                waveIn.StartRecording();

                samples = samplesAggregator.ReadSamplesFromSource(producer, secondsToRecord, sampleRate, GetNextSamplesFromContinuousBlockingQueue);

                waveIn.StopRecording();
            }

            WriteSamplesToFile(pathToFile, waveFormat, samples);
            return samples;
        }

        public void RecodeFileToMonoWave(string pathToFile, string pathToRecodedFile, int sampleRate)
        {
            using (var reader = new Mp3FileReader(pathToFile))
            {
                var ieeeFloatWaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1);
                using (var resampler = new MediaFoundationResampler(reader, ieeeFloatWaveFormat))
                {
                    WaveFileWriter.CreateWaveFile(pathToRecodedFile, resampler);
                }
            }
        }

        public int PlayFile(string filename)
        {
            waveOutDevice = new WaveOut();
            mainOutputStream = CreateInputStream(filename);
            waveOutDevice.Init(mainOutputStream);
            waveOutDevice.Play();
            return waveOutDevice.GetHashCode();
        }

        public void StopPlayingFile(int stream)
        {
            if (waveOutDevice != null)
            {
                waveOutDevice.Stop();
            }

            if (mainOutputStream != null)
            {
                mainOutputStream.Close();
                mainOutputStream = null;
            }

            if (waveOutDevice != null)
            {
                waveOutDevice.Dispose();
                waveOutDevice = null;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                if (waveOutDevice != null)
                {
                    waveOutDevice.Dispose();
                }

                if (mainOutputStream != null)
                {
                    mainOutputStream.Dispose();
                }
            }
        }

        private WaveStream CreateInputStream(string fileName)
        {
            var mp3Reader = new Mp3FileReader(fileName);
            return new WaveChannel32(mp3Reader);
        }

        private void SeekToSecondInCaseIfRequired(int startAtSecond, MediaFoundationReader reader)
        {
            if (startAtSecond > 0)
            {
                int actualSampleRate = reader.WaveFormat.SampleRate;
                int bitsPerSample = reader.WaveFormat.BitsPerSample;
                reader.Seek(actualSampleRate * bitsPerSample / 8 * startAtSecond, System.IO.SeekOrigin.Begin);
            }
        }

        private void WriteSamplesToFile(string pathToFile, WaveFormat waveFormat, float[] samples)
        {
            using (var writer = new WaveFileWriter(pathToFile, waveFormat))
            {
                writer.WriteSamples(samples, 0, samples.Length);
            }
        }

        private int GetNextSamples(SampleProviderConverterBase sampleProvider, float[] buffer)
        {
            return sampleProvider.Read(buffer, 0, buffer.Length) * 4;
        }

        private int GetNextStreamingSamples(SampleProviderConverterBase sampleProvider, float[] buffer)
        {
            int bytesRead = GetNextSamples(sampleProvider, buffer);

            while (bytesRead == 0)
            {
                Thread.Sleep(500); // lame but required to fill the buffer from continuous stream, either microphone or url
                bytesRead = GetNextSamples(sampleProvider, buffer);
            }

            return bytesRead;
        }

        private int GetNextSamplesFromContinuousBlockingQueue(BlockingCollection<float[]> producer, float[] buffer)
        {
            var samples = producer.Take();
            Array.Copy(samples, buffer, samples.Length);
            return samples.Length * 4;
        }

        private float[] GetFloatSamplesFromByte(int bytesRecorded, byte[] buffer)
        {
            int startIndex = 0;
            float[] chunk = new float[bytesRecorded / 4];
            int floatSampleIndex = 0;
            while (startIndex < bytesRecorded)
            {
                chunk[floatSampleIndex++] = BitConverter.ToSingle(buffer, startIndex);
                startIndex += 4;
            }

            return chunk;
        }

        private float[] ReadMonoFromSource(string pathToFile, int sampleRate, int secondsToRead, int startAtSecond, Func<SampleProviderConverterBase, float[], int> getNextSamples)
        {
            using (var reader = new MediaFoundationReader(pathToFile))
            {
                SeekToSecondInCaseIfRequired(startAtSecond, reader);
                var ieeeFloatWaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1);
                using (var resampler = new MediaFoundationResampler(reader, ieeeFloatWaveFormat))
                {
                    var waveToSampleProvider = new WaveToSampleProvider(resampler);
                    return samplesAggregator.ReadSamplesFromSource(waveToSampleProvider, secondsToRead, sampleRate, getNextSamples);
                }
            }
        }
    }
}
