namespace SoundFingerprinting.Audio
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class AudioService : IAudioService
    {
        public abstract IReadOnlyCollection<string> SupportedFormats { get; }

        public abstract void Dispose();

        public abstract float[] ReadMonoFromFile(string pathToFile, int sampleRate, int secondsToRead, int startAtSecond);

        public float[] ReadMonoFromFile(string pathToFile, int sampleRate)
        {
            return ReadMonoFromFile(pathToFile, sampleRate, 0, 0);
        }

        protected float[] ConcatenateChunksOfSamples(List<float[]> chunks)
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