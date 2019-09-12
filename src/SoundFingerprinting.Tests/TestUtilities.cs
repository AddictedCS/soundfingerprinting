namespace SoundFingerprinting.Tests
{
    using System;
    using System.Collections.Generic;

    using Audio;

    using SoundFingerprinting.Query;

    internal static class TestUtilities
    {
        public static AudioSamples GenerateRandomAudioSamples(int length)
        {
            return new AudioSamples(GenerateRandomFloatArray(length), string.Empty, 5512);
        }

        public static float[] GenerateRandomFloatArray(int length, int seed = 0)
        {
            var random = new Random(seed == 0 ? (int)DateTime.Now.Ticks << 4 : seed);
            float[] result = new float[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = (float)random.NextDouble() * 32767;
            }

            return result;
        }

        public static float[] GenerateRandomSingleArray(int length, int seed = 0)
        {
            var random = new Random(seed == 0 ? (int)DateTime.Now.Ticks << 4 : seed);
            float[] d = new float[length];
            for (int i = 0; i < length; i++)
            {
                d[i] = (float)(32767 - (random.NextDouble() * 65535));
            }

            return d;
        }

        public static IEnumerable<MatchedWith> GetMatchedWith(float[] queryAt, float[] resultAt)
        {
            for (int i = 0; i < queryAt.Length; ++i)
            {
                yield return new MatchedWith(queryAt[i], resultAt[i], 100);
            }
        }
    }
}