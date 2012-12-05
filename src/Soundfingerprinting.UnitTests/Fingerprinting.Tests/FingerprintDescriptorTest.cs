namespace Soundfingerprinting.UnitTests.Fingerprinting.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Soundfingerprinting.Fingerprinting;

    [TestClass]
    public class FingerprintDescriptorTest : BaseTest
    {
        private const int TopWavelets = 200;

        private IFingerprintDescriptor fingerprintDescriptor;

        [TestInitialize]
        public void SetUp()
        {
            fingerprintDescriptor = new FingerprintDescriptor();
        }

        [TestCleanup]
        public void TearDown()
        {
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
            Assert.AreEqual(actual.Length, expected.Length);
            for (int i = 0; i < actual.Length; i++)
            {
                Assert.AreEqual(actual[i], expected[i]);
            }
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

            var query =
                concatenated.Select((value, index) => new KeyValuePair<int, double>(index, value)).OrderByDescending(
                    pair => Math.Abs(pair.Value));

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
