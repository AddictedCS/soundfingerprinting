namespace SoundFingerprinting.Tests.Unit.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Utils;

    [TestClass]
    public class FingerprintDescriptorTest : AbstractTest
    {
        private const int TopWavelets = 200;

        private IFingerprintDescriptor fingerprintDescriptor;

        [TestInitialize]
        public void SetUp()
        {
            fingerprintDescriptor = new FingerprintDescriptor();
        }

        [TestMethod]
        public void ExtractTopWaveletesText()
        {
            float[][] frames = new float[128][];
            for (int i = 0; i < frames.Length; i++)
            {
                frames[i] = TestUtilities.GenerateRandomFloatArray(32);
            }

            bool[] actual = fingerprintDescriptor.ExtractTopWavelets(frames, TopWavelets);
            bool[] expected = ExtractTopWaveletsTested(frames, TopWavelets);

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ExtractTopWaveletWorksCorrectly()
        {
            float[][] frames = new float[2][];
            frames[0] = new float[] { 5, 6, 1, 8, 9, 2, 0, -4, 6, -10, 7, 3 };
            frames[1] = new float[] { 1, 0, 2, 5, -11, 0, 5, 13, 7, 6, 3, 2 };
            bool[] expected = new[]
                                  {
                                      false, false, false, false, false, false, true, false, true, false, false, false, false, false, false, false, false, false, false,
                                      true, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false,
                                      true, false, false, false, false, false, false, false, false, false
                                  };

            bool[] encodedFingerprint = fingerprintDescriptor.ExtractTopWavelets(frames, 5);

            CollectionAssert.AreEqual(expected, encodedFingerprint);
        }

        [TestMethod]
        public void EncodeFingerprintWorksAsExpected()
        {
            float[] concatenatedSpectrumPowers = new float[] { 2, 4, 8, 9, 1, 3, 5 };
            int[] indexes = new[] { 3, 2, 6, 1, 5, 0, 4 };
            bool[] expected = new[] { false, false, false, false, true, false, true, false, false, false, false, false, false, false };

            bool[] encodedFingerprint = fingerprintDescriptor.EncodeFingerprint(concatenatedSpectrumPowers, indexes, 2);

            CollectionAssert.AreEqual(expected, encodedFingerprint);
        }

        [TestMethod]
        public void DecodeFingerprintWorksAsExpected()
        {
            double[] expected = new double[] { 0, 0, 1, -1, 0, 0, 0 };

            double[] decoded = fingerprintDescriptor.DecodeFingerprint(new[] { false, false, false, false, true, false, false, true, false, false, false, false, false, false });

            CollectionAssert.AreEqual(expected, decoded);
        }

        private bool[] ExtractTopWaveletsTested(float[][] frames, int topWavelets)
        {
            int rows = frames.GetLength(0); /*128*/
            int cols = frames[0].Length; /*32*/

            double[] concatenated = new double[rows * cols]; /* 128 * 32 */
            for (int row = 0; row < rows; row++)
            {
                Array.Copy(frames[row], 0, concatenated, row * frames[row].Length, frames[row].Length);
            }

            var query = concatenated.Select((value, index) => new KeyValuePair<int, double>(index, value))
                                    .OrderByDescending(pair => Math.Abs(pair.Value));

            if (topWavelets >= concatenated.Length)
            {
                throw new ArgumentException("TopWaveletes cannot exceed the length of concatenated array");
            }

            // Negative Numbers = 01
            // Positive Numbers = 10
            // Zeros            = 00      
            bool[] result = new bool[concatenated.Length * 2]; /*Concatenated float array*/
            int topW = 0;
            foreach (var pair in query)
            {
                if (++topW > topWavelets)
                {
                    break;
                }

                int index = pair.Key;
                double value = pair.Value;
                if (value > 0)
                {
                    result[index * 2] = true;
                }
                else if (value < 0)
                {
                    result[(index * 2) + 1] = true;
                }
            }

            return result;
        }
    }
}
