namespace SoundFingerprinting.Audio
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class SamplesAggregator : ISamplesAggregator
    {
        public const int DefaultBufferLengthInSeconds = 20;

        private const int BlockAlign = 4;

        public float[] ReadSamplesFromSource(ISamplesProvider samplesProvider, int secondsToRead, int sampleRate)
        {
            var buffer = GetBuffer(secondsToRead, sampleRate);
            int totalBytesToRead = GetTotalBytesToRead(secondsToRead, sampleRate), totalBytesRead = 0;
            var chunks = new List<float[]>();

            while (totalBytesRead < totalBytesToRead)
            {
                int bytesRead = samplesProvider.GetNextSamples(buffer);

                if (bytesRead  < 0)
                {
                    throw new AudioServiceException("Number of bytes read is negative.");
                }

                if (bytesRead == 0)
                {
                    break;
                }

                totalBytesRead += bytesRead;

                float[] chunk;

                if (totalBytesRead > totalBytesToRead)
                {
                    chunk = new float[(totalBytesToRead - (totalBytesRead - bytesRead)) / BlockAlign];
                    Array.Copy(buffer, chunk, (totalBytesToRead - (totalBytesRead - bytesRead)) / BlockAlign);
                }
                else
                {
                    chunk = new float[bytesRead / BlockAlign];
                    Array.Copy(buffer, chunk, bytesRead / BlockAlign);
                }

                chunks.Add(chunk);
            }

            if (totalBytesRead < (secondsToRead * sampleRate * BlockAlign))
            {
                throw new AudioServiceException("Could not read requested number of seconds " + secondsToRead + ", audio file is not that long");
            }

            return ConcatenateChunksOfSamples(chunks);
        }

        private int GetTotalBytesToRead(int secondsToRead, int sampleRate)
        {
            if (secondsToRead == 0)
            {
                return int.MaxValue;
            }

            return secondsToRead * sampleRate * BlockAlign;
        }

        private float[] GetBuffer(int secondsToRead, int sampleRate)
        {
            return new float[GetBufferLength(sampleRate, secondsToRead)];
        }

        private float[] ConcatenateChunksOfSamples(IReadOnlyList<float[]> chunks)
        {
            if (chunks.Count == 1)
            {
                return chunks[0];
            }

            float[] samples = new float[chunks.Sum(a => a.Length)];
            int index = 0;
            foreach (float[] chunk in chunks)
            {
                Array.Copy(chunk, 0, samples, index, chunk.Length);
                index += chunk.Length;
            }

            return samples;
        }

        private int GetBufferLength(int sampleRate, int secondsToRead)
        {
            if (secondsToRead > 0 && secondsToRead < DefaultBufferLengthInSeconds)
            {
                return sampleRate * secondsToRead;
            }

            return sampleRate * DefaultBufferLengthInSeconds;
        }
    }
}
