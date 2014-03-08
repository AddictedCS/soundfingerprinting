namespace SoundFingerprinting.Audio
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Audio.Bass;

    public abstract class AudioService : IAudioService
    {
        public const int DefaultSampleRate = 44100;

        public const int DefaultBufferLengthInSeconds = 20;

        public abstract IReadOnlyCollection<string> SupportedFormats { get; }

        public abstract float[] ReadMonoFromFile(string pathToFile, int sampleRate, int secondsToRead, int startAtSecond);

        public float[] ReadMonoFromFile(string pathToFile, int sampleRate)
        {
            return ReadMonoFromFile(pathToFile, sampleRate, 0, 0);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool isDisposing);

        protected float[] ReadSamplesFromSource(object source, int secondsToRead, int sampleRate)
        {
            float[] buffer = new float[sampleRate * DefaultBufferLengthInSeconds];
            int totalBytesToRead = secondsToRead == 0 ? int.MaxValue : secondsToRead * sampleRate * 4, totalBytesRead = 0;
            var chunks = new List<float[]>();

            while (totalBytesRead < totalBytesToRead)
            {
                int bytesRead = ReadNextSamples(source, buffer); // get re-sampled/mono data

                if (bytesRead == -1)
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
                    chunk = new float[(totalBytesToRead - (totalBytesRead - bytesRead)) / 4];
                    Array.Copy(buffer, chunk, (totalBytesToRead - (totalBytesRead - bytesRead)) / 4);
                }
                else
                {
                    chunk = new float[bytesRead / 4]; // each float contains 4 bytes
                    Array.Copy(buffer, chunk, bytesRead / 4);
                }

                chunks.Add(chunk);
            }

            if (totalBytesRead < (secondsToRead * sampleRate * 4))
            {
                throw new AudioServiceException("Could not read requested number of seconds " + secondsToRead + ", audio file is not that long");
            }

            return ConcatenateChunksOfSamples(chunks);
        }

        protected abstract int ReadNextSamples(object source, float[] buffer);

        private float[] ConcatenateChunksOfSamples(List<float[]> chunks)
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
    }
}