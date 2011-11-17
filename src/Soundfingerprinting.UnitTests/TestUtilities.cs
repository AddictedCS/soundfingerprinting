// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;

namespace Soundfingerprinting.UnitTests
{
    public static class TestUtilities
    {
        public static Random Rand = new Random();

        /// <summary>
        ///   Generates random float array [0..32767]
        /// </summary>
        /// <param name = "length">Length of the array</param>
        /// <returns></returns>
        public static float[] GenerateRandomFloatArray(int length)
        {
            float[] result = new float[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = (float) Rand.NextDouble()*32767;
            }
            return result;
        }

        /// <summary>
        ///   Generate random input float array [0, 1, -1]
        /// </summary>
        /// <param name = "length">Length of the array</param>
        /// <returns></returns>
        public static float[] GenerateRandomInputFloatArray(int length)
        {
            float[] result = new float[length];
            for (int i = 0; i < length; i++)
            {
                float r = (float) Rand.NextDouble();
                result[i] = (r < 0.33) ? 0 : (r < 0.66) ? 1 : -1;
            }
            return result;
        }

        /// <summary>
        ///   Generate random output float array [0, 1]
        /// </summary>
        /// <param name = "length">Length of the array</param>
        /// <returns></returns>
        public static float[] GenerateRandomOutputFloatArray(int length)
        {
            float[] result = new float[length];
            for (int i = 0; i < length; i++)
            {
                float r = (float) Rand.NextDouble();
                result[i] = (r < 0.5) ? 0 : 1;
            }
            return result;
        }

        /// <summary>
        ///   Generate random float array [-32767..32768]
        /// </summary>
        /// <param name = "length">Length of the array</param>
        /// <returns>[-32767..32768]</returns>
        public static float[] GenerateRandomDoubleArray(int length)
        {
            float[] d = new float[length];
            for (int i = 0; i < length; i++)
            {
                d[i] = (float) (32767 - Rand.NextDouble()*65535);
            }
            return d;
        }

        /// <summary>
        ///   Generate random input byte array [0, -1, 1]
        /// </summary>
        /// <param name = "length">Length of the array</param>
        /// <returns>Byte array</returns>
        public static byte[] GenerateRandomInputByteArray(int length)
        {
            byte[] b = new byte[length];
            for (int i = 0; i < length; i++)
            {
                float d = (float) Rand.NextDouble();
                b[i] = (d < 0.33) ? (byte) 255 /*-1*/ : (d < 0.66) ? (byte) 0 : (byte) 1;
            }
            return b;
        }

        /// <summary>
        ///   Generate random byte array [0 255]
        /// </summary>
        /// <param name = "length">Length of the array</param>
        /// <returns>Byte array</returns>
        public static byte[] GenerateRandomByteArray(int length)
        {
            byte[] b = new byte[length];
            for (int i = 0; i < length; i++)
            {
                b[i] = (byte) Rand.Next(255);
            }
            return b;
        }
    }
}