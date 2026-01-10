namespace SoundFingerprinting.Audio
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    ///  Simplest implementation of an audio service. This class should be used for testing purposes only.
    ///  If you are looking for production ready audio service, use FFmpegAudioService from <a href="https://www.nuget.org/packages/SoundFingerprinting.Emy/">SoundFingerprinting.Emy package</a>.
    ///  More details about installation and usage of FFmpegAudioService can be found <a href="https://github.com/AddictedCS/soundfingerprinting/wiki/Audio-Services">here</a>.
    /// </summary>
    public class SoundFingerprintingAudioService : AudioService
    {
        private static readonly HashSet<int> AcceptedSampleRates = new HashSet<int> { 5512, 8000, 11025, 16000, 22050, 44100, 48000, 96000 };
        private static readonly HashSet<int> AcceptedBitsPerSample = new HashSet<int> { 8, 16, 24, 32 };
        private static readonly HashSet<int> AcceptedChannels = new HashSet<int> { 1, 2 };

        private const int WaveHeaderLength = 44;

        private readonly ILowPassFilter lowPassFilter;
        private readonly IAudioSamplesNormalizer audioSamplesNormalizer;

        /// <summary>
        ///  Initializes a new instance of the <see cref="SoundFingerprintingAudioService"/> class.
        /// </summary>
        /// <remarks>
        ///  This class should be used only for testing purposes as it aliases the signal during down sampling process.
        /// </remarks>
        public SoundFingerprintingAudioService() : this(new LowPassFilter(), new AudioSamplesNormalizer())
        {
            // no op
        }

        internal SoundFingerprintingAudioService(ILowPassFilter lowPassFilter, IAudioSamplesNormalizer audioSamplesNormalizer)
        {
            this.lowPassFilter = lowPassFilter;
            this.audioSamplesNormalizer = audioSamplesNormalizer;
        }
        
        /// <inheritdoc cref="AudioService.SupportedFormats"/>
        public override IReadOnlyCollection<string> SupportedFormats => [".wav"];

        /// <inheritdoc cref="AudioService.ReadMonoSamplesFromFile(string,int,double,double)"/>
        public override AudioSamples ReadMonoSamplesFromFile(string pathToSourceFile, int sampleRate, double seconds, double startAt)
        {
            var format = WaveFormat.FromFile(pathToSourceFile);
            CheckInputFileFormat(format, startAt);
            float[] samples = ToSamples(pathToSourceFile, format, seconds, startAt);
            float[] monoSamples = ToMonoSamples(samples, format);
            float[] downSampled = ToTargetSampleRate(monoSamples, format.SampleRate, sampleRate);
            audioSamplesNormalizer.NormalizeInPlace(downSampled);
            return new AudioSamples(downSampled, pathToSourceFile, sampleRate);
        }

        /// <inheritdoc cref="AudioService.GetLengthInSeconds"/>
        public override float GetLengthInSeconds(string file)
        {
            var format = WaveFormat.FromFile(file);
            CheckInputFileFormat(format, 0);
            return format.LengthInSeconds;
        }

        private static void CheckInputFileFormat(WaveFormat format, double startsAt)
        {
            if (!AcceptedSampleRates.Contains(format.SampleRate))
            {
                throw new ArgumentException(
                    $"Sample rate of the given file is not supported {format}. Supported sample rates {string.Join(",", AcceptedSampleRates)}. "
                    + $"Submit a github request if you need a different sample rate to be supported.");
            }

            if (!AcceptedBitsPerSample.Contains(format.BitsPerSample))
            {
                throw new ArgumentException($"Bad file format {format}. Bits per sample ({format.BitsPerSample}) is less than accepted range.");
            }

            if (!AcceptedChannels.Contains(format.Channels))
            {
                throw new ArgumentException($"Bad file format {format}. Number of channels is not in the accepted range.");
            }

            if (startsAt > format.LengthInSeconds)
            {
                throw new ArgumentException($"Could not start reading from {startsAt} second, since input file is shorter {format.LengthInSeconds}");
            }
        }

        private float[] ToSamples(string pathToFile, WaveFormat format, double seconds, double startsAt)
        {
            using var stream = new FileStream(pathToFile, FileMode.Open, FileAccess.Read, FileShare.Read);
            stream.Seek(WaveHeaderLength, SeekOrigin.Begin);
            int samplesToSeek = (int)(startsAt * format.SampleRate * format.Channels);
            int bytesPerSample = format.BitsPerSample / 8;
            stream.Seek(bytesPerSample * samplesToSeek, SeekOrigin.Current);
            return GetInts(stream, format, seconds, startsAt);
        }

        private static float[] ToMonoSamples(float[] samples, WaveFormat format)
        {
            if (format.Channels == 1)
            {
                return samples;
            }

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
            float[] samples = new float[samplesCount];

            // Use buffered reading for better performance (read 4KB at a time)
            const int bufferSize = 4096;
            byte[] buffer = new byte[bufferSize];

            int samplesOffset = 0;
            int bufferOffset = 0;
            int bytesInBuffer = 0;

            while (samplesOffset < samplesCount)
            {
                // Refill buffer if needed
                if (bufferOffset >= bytesInBuffer)
                {
                    bytesInBuffer = reader.Read(buffer, 0, bufferSize);
                    bufferOffset = 0;
                    if (bytesInBuffer == 0)
                    {
                        break;
                    }
                }

                // Check if we have enough bytes for a complete sample
                if (bufferOffset + bytesPerSample > bytesInBuffer)
                {
                    // Handle partial sample at buffer boundary
                    int remaining = bytesInBuffer - bufferOffset;
                    byte[] temp = new byte[bytesPerSample];
                    Array.Copy(buffer, bufferOffset, temp, 0, remaining);
                    int additionalRead = reader.Read(temp, remaining, bytesPerSample - remaining);
                    if (additionalRead + remaining < bytesPerSample)
                    {
                        break;
                    }

                    samples[samplesOffset] = ConvertToFloat(temp, 0, bytesPerSample);
                    bufferOffset = bytesInBuffer; // Force buffer refill on next iteration
                }
                else
                {
                    samples[samplesOffset] = ConvertToFloat(buffer, bufferOffset, bytesPerSample);
                    bufferOffset += bytesPerSample;
                }

                samplesOffset++;
            }

            return samples;
        }

        private static float ConvertToFloat(byte[] buffer, int offset, int bytesPerSample)
        {
            float sample;
            switch (bytesPerSample)
            {
                case 1:
                    // 8-bit audio is unsigned (0-255), center at 128
                    sample = (buffer[offset] - 128) / 128f;
                    break;
                case 2:
                    short sample16 = (short)(buffer[offset] | buffer[offset + 1] << 8);
                    sample = sample16 / 32768f;
                    break;
                case 3:
                    // 24-bit audio needs sign extension
                    int sample24 = buffer[offset] | buffer[offset + 1] << 8 | buffer[offset + 2] << 16;
                    if ((sample24 & 0x800000) != 0)
                    {
                        sample24 |= unchecked((int)0xFF000000); // Sign extend
                    }

                    sample = sample24 / 8388608f;
                    break;
                default:
                    int sample32 = buffer[offset] | buffer[offset + 1] << 8 | buffer[offset + 2] << 16 | buffer[offset + 3] << 24;
                    sample = sample32 / 2147483648f;
                    break;
            }

            // Clamp to [-1, 1] range
            return Math.Max(-1f, Math.Min(1f, sample));
        }

        private static long GetSamplesToRead(WaveFormat format, double seconds, double startsAt)
        {
            long samplesPerSecond = format.SampleRate * format.Channels;
            long requestedSamples = Math.Abs(seconds) < 0.001 ? long.MaxValue : (long)(seconds * samplesPerSecond);
            int bytesPerSample = format.BitsPerSample / 8;
            long samplesInInput = format.Length / bytesPerSample - (long)(startsAt * samplesPerSecond);

            return Math.Min(samplesInInput, requestedSamples);
        }
    }
}
