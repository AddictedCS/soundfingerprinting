namespace SoundFingerprinting.Tests
{
    using System;

    using Audio;

    public static class TestUtilities
    {
        private static readonly Random Rand = new Random();

        public static AudioSamples GenerateRandomAudioSamples(int length)
        {
            return new AudioSamples(GenerateRandomFloatArray(length), string.Empty, 5512);
        }

        public static float[] GenerateRandomFloatArray(int length)
        {
            float[] result = new float[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = (float)Rand.NextDouble() * 32767;
            }

            return result;
        }

        public static float[] GenerateRandomSingleArray(int length)
        {
            float[] d = new float[length];
            for (int i = 0; i < length; i++)
            {
                d[i] = (float)(32767 - (Rand.NextDouble() * 65535));
            }

            return d;
        }
    }
}