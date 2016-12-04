namespace SoundFingerprinting.Audio
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class SamplesAggregator : ISamplesAggregator
    {
        internal const int DefaultBufferLengthInSeconds = 60;

        private const int BlockAlign = 4;

        public float[] ReadSamplesFromSource(ISamplesProvider samplesProvider, double secondsToRead, int sampleRate)
        {
            var buffer = GetBuffer(secondsToRead, sampleRate);
            int totalBytesToRead = GetTotalBytesToRead(secondsToRead, sampleRate), totalBytesRead = 0;
            var chunks = new List<float[]>();

            while (totalBytesRead < totalBytesToRead)
            {
                int bytesRead = samplesProvider.GetNextSamples(buffer);

                if (bytesRead < 0)
                {
                    throw new AudioServiceException("Number of read bytes is negative. Check your audio provider.");
                }

                if (bytesRead == 0)
                {
                    break;
                }

                totalBytesRead += bytesRead;

                float[] chunk = GetNewChunkFromBuffer(totalBytesRead, totalBytesToRead, bytesRead, buffer);

                chunks.Add(chunk);
            }

            if (totalBytesRead < GetExactNumberOfBytesToRead(secondsToRead, sampleRate))
            {
                throw new AudioServiceException("Could not read requested number of seconds " + secondsToRead + ", audio file is not that long");
            }

            return ConcatenateChunksOfSamples(chunks);
        }

        private float[] GetNewChunkFromBuffer(int totalBytesRead, int totalBytesToRead, int bytesRead, float[] buffer)
        {
            float[] chunk;
            if (totalBytesRead > totalBytesToRead)
            {
                var chunkLength = (totalBytesToRead - (totalBytesRead - bytesRead)) / BlockAlign;
                chunk = CopyFromBufferToNewChunk(chunkLength, buffer);
            }
            else
            {
                var chunkLength = bytesRead / BlockAlign;
                chunk = CopyFromBufferToNewChunk(chunkLength, buffer);
            }

            return chunk;
        }

        private float[] CopyFromBufferToNewChunk(int chunkLength, float[] buffer)
        {
            float[] chunk = new float[chunkLength];
            Buffer.BlockCopy(buffer, 0, chunk, 0, chunkLength * sizeof(float));
            return chunk;
        }

        private int GetTotalBytesToRead(double secondsToRead, int sampleRate)
        {
            if (Math.Abs(secondsToRead) < 0.0001)
            {
                return int.MaxValue;
            }

            return GetExactNumberOfBytesToRead(secondsToRead, sampleRate);
        }

        private float[] GetBuffer(double secondsToRead, int sampleRate)
        {
            return new float[GetBufferLength(secondsToRead, sampleRate)];
        }

        private float[] ConcatenateChunksOfSamples(IReadOnlyList<float[]> chunks)
        {
            if (chunks.Count == 1)
            {
                return chunks[0];
            }

            float[] samples = new float[chunks.Sum(a => a.Length)];
            int startAt = 0;
            foreach (float[] chunk in chunks)
            {
                Buffer.BlockCopy(chunk, 0, samples, startAt * sizeof(float), chunk.Length * sizeof(float));
                startAt += chunk.Length;
            }

            return samples;
        }

        private int GetBufferLength(double secondsToRead, int sampleRate)
        {
            if (secondsToRead > 0 && secondsToRead < DefaultBufferLengthInSeconds)
            {
                return GetExactNumberOfBytesToRead(secondsToRead, sampleRate) / BlockAlign;
            }

            return sampleRate * DefaultBufferLengthInSeconds;
        }

        private int GetExactNumberOfBytesToRead(double secondsToRead, int sampleRate)
        {
            return (int)(secondsToRead * sampleRate) / BlockAlign * BlockAlign * BlockAlign;
        }
    }
}
