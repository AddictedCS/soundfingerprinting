namespace SoundFingerprinting.Audio
{
    using System.IO;
    using System;
    using System.Collections.Generic;

    public class SoundFingerprintingAudioService : AudioService
    {
        private static readonly HashSet<int> AcceptedSampleRates = new HashSet<int> { 5512, 11025, 22050, 44100, 48000 };
        private static readonly HashSet<int> AcceptedBitsPerSample = new HashSet<int> { 8, 16, 24, 32 };
        private static readonly HashSet<int> AcceptedChannels = new HashSet<int> { 1, 2 };

        private static int waveHeaderLength = 44;

        private readonly ILowPassFilter lowPassFilter;
        private readonly IAudioSamplesNormalizer audioSamplesNormalizer;

        public SoundFingerprintingAudioService(): this(new LowPassFilter(), new AudioSamplesNormalizer())
        {
            // no op
        }

        internal SoundFingerprintingAudioService(ILowPassFilter lowPassFilter, IAudioSamplesNormalizer audioSamplesNormalizer)
        {
            this.lowPassFilter = lowPassFilter;
            this.audioSamplesNormalizer = audioSamplesNormalizer;
        }

        public override AudioSamples ReadMonoSamplesFromFile(string pathToSourceFile, int sampleRate, double seconds, double startAt)
        {
            var format = WaveFormat.FromFile(pathToSourceFile);
            CheckInputFileFormat(format, startAt);
            float[] samples = ToSamples(pathToSourceFile, format, seconds, startAt);
            float[] monoSamples = ToMonoSamples(samples, format);
            float[] downsampled = ToTargetSampleRate(monoSamples, format.SampleRate, sampleRate);
            audioSamplesNormalizer.NormalizeInPlace(downsampled);
            return new AudioSamples(downsampled, pathToSourceFile, sampleRate);
        }

        public override float GetLengthInSeconds(string pathToSourceFile)
        {
            var format = WaveFormat.FromFile(pathToSourceFile);
            CheckInputFileFormat(format, 0);
            return format.LengthInSeconds;
        }

        public override IReadOnlyCollection<string> SupportedFormats
        {
            get
            {
                return new[] { ".wav" };
            }
        }

        private static void CheckInputFileFormat(WaveFormat format, double startsAt)
        {
            if (!AcceptedSampleRates.Contains(format.SampleRate))
                throw new ArgumentException($"Sample rate of the given file is not supported {format}. Supported sample rates {string.Join(",", AcceptedSampleRates)}. "
                                            + $"Submit a github request if you need a different sample rate to be supported.");
            if (!AcceptedBitsPerSample.Contains(format.BitsPerSample))
                throw new ArgumentException($"Bad file format {format}. Bits per sample ({format.BitsPerSample}) is less than accepted range.");
            if (!AcceptedChannels.Contains(format.Channels))
                throw new ArgumentException($"Bad file format {format}. Number of channels is not in the accepted range.");
            if (startsAt > format.LengthInSeconds)
                throw new ArgumentException($"Could not start reading from {startsAt} second, since input file is shorter {format.LengthInSeconds}");
        }
        
        private float[] ToSamples(string pathToFile, WaveFormat format, double seconds, double startsAt)
        {
            using (var stream = new FileStream(pathToFile, FileMode.Open))
            {
                stream.Seek(waveHeaderLength, SeekOrigin.Begin);
                int samplesToSeek = (int)(startsAt * format.SampleRate * format.Channels);
                int bytesPerSample = format.BitsPerSample / 8;
                stream.Seek(bytesPerSample * samplesToSeek, SeekOrigin.Current);
                return GetInts(stream, format, seconds, startsAt);
            }
        }

        private float[] ToMonoSamples(float[] samples, WaveFormat format)
        {
            if (format.Channels == 1) return samples;

            float[] mono = new float[samples.Length / 2];
            for (int i = 0, k = 0; i < samples.Length - 1; i += 2, k++)
            {
                int left = i;
                int right = i + 1;
                mono[k] = (samples[left] + samples[right]) / 2;
            }

            return mono;
        }

        private float[] ToTargetSampleRate(float[] monoSamples, int sourceSampleRate, int sampleRate)
        {
            return lowPassFilter.FilterAndDownsample(monoSamples, sourceSampleRate, sampleRate);
        }

        private float[] GetInts(Stream reader, WaveFormat format, double seconds, double startsAt)
        {
            int bytesPerSample = format.BitsPerSample / 8;
            long samplesCount = GetSamplesToRead(format, seconds, startsAt);

            byte[] buffer = new byte[bytesPerSample];

            int normalizer = bytesPerSample == 1 ? 127 : bytesPerSample == 2 ? Int16.MaxValue : bytesPerSample == 3 ? (int)Math.Pow(2, 24) / 2 - 1 : Int32.MaxValue;

            int samplesOffset = 0;
            float[] samples = new float[samplesCount];
            while (reader.CanRead && samplesOffset < samplesCount)
            {
                int read = reader.Read(buffer, 0, bytesPerSample);
                if (read != bytesPerSample) return samples;

                if (bytesPerSample == 1)
                {
                    samples[samplesOffset] = (float) buffer[0] / normalizer;
                }
                else if (bytesPerSample == 2)
                {
                    short sample = (short)(buffer[0] | buffer[1] << 8);
                    samples[samplesOffset] = (float)sample / normalizer;
                }
                else if (bytesPerSample == 3)
                {
                    int sample = buffer[0] | buffer[1] << 8 | buffer[2] << 16;
                    samples[samplesOffset] = (float)sample / normalizer;
                }
                else
                {
                    int sample = buffer[0] | buffer[1] << 8 | buffer[2] << 16 | buffer[3] << 24;
                    samples[samplesOffset] = (float)sample / normalizer;
                }

                samples[samplesOffset] = Math.Min(1, samples[samplesOffset]);
                samplesOffset++;
            }

            return samples;
        }

        private long GetSamplesToRead(WaveFormat format, double seconds, double startsAt)
        {
            int samplesPerSecond = format.SampleRate * format.Channels;
            int requestedSamples = Math.Abs(seconds) < 0.001 ? int.MaxValue : (int)seconds * samplesPerSecond;
            int bytesPerSample = format.BitsPerSample / 8;
            int samplesInInput = (int)format.Length / bytesPerSample - (int)startsAt * samplesPerSecond;

            if (samplesInInput > requestedSamples)
            {
                return requestedSamples;
            }

            return samplesInInput;
        }
    }
}
