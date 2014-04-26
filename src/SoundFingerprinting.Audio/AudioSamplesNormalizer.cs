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

            float rms = (float)Math.Sqrt(squares / samples.Length) * 10;

            if (rms < MinRms)
            {
                rms = MinRms;
            }

            if (rms > MaxRms)
            {
                rms = MaxRms;
            }

            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] /= rms;
                samples[i] = Math.Min(samples[i], 1);
                samples[i] = Math.Max(samples[i], -1);
            }
        }
    }
}
