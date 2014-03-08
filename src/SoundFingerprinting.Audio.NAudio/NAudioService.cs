namespace SoundFingerprinting.Audio.NAudio
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

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
            using (var reader = new MediaFoundationReader(pathToFile))
            {
                SeekToSecondInCaseIfRequired(startAtSecond, reader);
                using (
                    var resampler = new MediaFoundationResampler(
                        reader, WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1)))
                {
                    var pcmReader = new Pcm32BitToSampleProvider(resampler);
                    return samplesAggregator.ReadSamplesFromSource(pcmReader, secondsToRead, sampleRate, GetNextSamples);
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

        public void RecodeFileToMonoWave(string pathToFile, string pathToRecodedFile, int sampleRate)
        {
            using (var reader = new Mp3FileReader(pathToFile))
            {
                using (var resampler = new MediaFoundationResampler(reader, WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1)))
                {
                    WaveFileWriter.CreateWaveFile(pathToRecodedFile, resampler);
                }
            }
        }

        public float[] ReadMonoFromUrlToFile(string streamUrl, string pathToFile, int sampleRate, int secondsToDownload)
        {
            using (var reader = new MediaFoundationReader(streamUrl))
            {
                using (
                    var resampler = new MediaFoundationResampler(
                        reader, WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1)))
                {
                    var pcmReader = new Pcm32BitToSampleProvider(resampler);
                    var samples = samplesAggregator.ReadSamplesFromSource(pcmReader, secondsToDownload, sampleRate, GetNextSamples);
                    WriteSamplesToFile(pathToFile, new WaveFormat(sampleRate, 1), samples);
                    return samples;
                }
            }
        }

        public float[] ReadMonoFromMicrophoneToFile(string pathToFile, int sampleRate, int secondsToRecord)
        {
            var producer = new BlockingCollection<float[]>();
            var waveFormat = new WaveFormat(sampleRate, 1);
            var waveIn = new WaveInEvent { WaveFormat = waveFormat };
            
            waveIn.DataAvailable += (sender, e) =>
                {
                    var chunk = GetFloatSamplesFromByte(e.BytesRecorded, e.Buffer);
                    producer.Add(chunk);
                };

            waveIn.RecordingStopped += (sender, args) => producer.CompleteAdding();
            
            waveIn.StartRecording();

            float[] samples = samplesAggregator.ReadSamplesFromSource(producer, secondsToRecord, sampleRate, ConsumeRecordedSamples);

            waveIn.StopRecording();

            WriteSamplesToFile(pathToFile, waveFormat, samples);

            return samples;
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

        private int GetNextSamples(Pcm32BitToSampleProvider pcm32BitToSampleProvider, float[] buffer)
        {
            return pcm32BitToSampleProvider.Read(buffer, 0, buffer.Length) * 4;
        }

        private int ConsumeRecordedSamples(BlockingCollection<float[]> producer, float[] buffer)
        {
            var samples = producer.Take();
            Array.Copy(samples, buffer, samples.Length);
            return samples.Length;
        }

        private float[] GetFloatSamplesFromByte(int bytesRecorded, byte[] buffer)
        {
            float[] chunk = new float[bytesRecorded / 2];

            for (int index = 0; index < bytesRecorded; index += 2)
            {
                short sample = (short)((buffer[index + 1] << 8) | buffer[index + 0]);
                float sample32 = sample / 32768f;
                chunk[index / 2] = sample32;
            }

            return chunk;
        }

        private void WriteSamplesToFile(string pathToFile, WaveFormat waveFormat, float[] samples)
        {
            using (WaveFileWriter writer = new WaveFileWriter(pathToFile, waveFormat))
            {
                writer.WriteSamples(samples, 0, samples.Length);
            }
        }
    }
}
