namespace SoundFingerprinting.Audio
{
    using System;
    using System.Linq;

    internal class AudioSamplesNormalizer : IAudioSamplesNormalizer
    {
        private const float MinRms = 0.1f;

        private const float MaxRms = 3;

        public void NormalizeInPlace(float[] samples)
        {
            double squares = samples.AsParallel().Aggregate<float, double>(0, (current, t) => current + (t * t));
            Normalize(samples, squares, 0, samples.Length);
        }

        public void NormalizeInPlace(float[] samples, int sampleRate, int windowInSeconds)
        {
            int windowLength = sampleRate * windowInSeconds;
            int slices = samples.Length / windowLength;
            for (int i = 0; i < slices; ++i)
            {
                int start = i * windowLength;
                double squares = 0d;
                for (int j = start; j < windowLength; ++j)
                {
                    squares += samples[j] * samples[j];
                }

                Normalize(samples, squares, start, start + windowLength);
            }
        }

        private static void Normalize(float[] samples, double squares, int start, int end)
        {
            float rms = (float)Math.Sqrt(squares / (end - start)) * 10;

            if (rms < MinRms)
                rms = MinRms;

            if (rms > MaxRms)
                rms = MaxRms;

            for (int i = start; i < end; ++i)
            {
                samples[i] /= rms;
                samples[i] = Math.Min(samples[i], 1);
                samples[i] = Math.Max(samples[i], -1);
            }
        }
    }
}
