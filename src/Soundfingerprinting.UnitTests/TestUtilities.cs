namespace Soundfingerprinting.UnitTests
{
    using System;

    public static class TestUtilities
    {
        private static readonly Random Rand = new Random();

        public static float[] GenerateRandomFloatArray(int length)
        {
            float[] result = new float[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = (float)Rand.NextDouble() * 32767;
            }

            return result;
        }

        public static float[] GenerateRandomInputFloatArray(int length)
        {
            float[] result = new float[length];
            for (int i = 0; i < length; i++)
            {
                float r = (float)Rand.NextDouble();
                result[i] = (r < 0.33) ? 0 : (r < 0.66) ? 1 : -1;
            }

            return result;
        }

        public static float[] GenerateRandomOutputFloatArray(int length)
        {
            float[] result = new float[length];
            for (int i = 0; i < length; i++)
            {
                float r = (float)Rand.NextDouble();
                result[i] = (r < 0.5) ? 0 : 1;
            }

            return result;
        }

        public static float[] GenerateRandomDoubleArray(int length)
        {
            float[] d = new float[length];
            for (int i = 0; i < length; i++)
            {
                d[i] = (float)(32767 - (Rand.NextDouble() * 65535));
            }

            return d;
        }

        public static byte[] GenerateRandomInputByteArray(int length)
        {
            byte[] b = new byte[length];
            for (int i = 0; i < length; i++)
            {
                float d = (float)Rand.NextDouble();
                b[i] = (d < 0.33) ? (byte)255 /*-1*/ : (d < 0.66) ? (byte)0 : (byte)1;
            }

            return b;
        }

        public static byte[] GenerateRandomByteArray(int length)
        {
            byte[] b = new byte[length];
            for (int i = 0; i < length; i++)
            {
                b[i] = (byte)Rand.Next(255);
            }

            return b;
        }
    }
}