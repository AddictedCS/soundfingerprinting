namespace SoundFingerprinting.Tests.Unit.Fingerprinting.Utils
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Utils;

    [TestClass]
    public class FingerprintDescriptorTest : AbstractTest
    {
        private readonly FingerprintDescriptor fingerprintDescriptor = new FingerprintDescriptor();

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

            for (int i = 0; i < encodedFingerprint.Length; i++)
            {
                Assert.AreEqual(expected[i], encodedFingerprint[i]);
            }
        }

        [TestMethod]
        public void EncodeFingerprintWorksAsExpected()
        {
            float[] concatenatedSpectrumPowers = new float[] { 2, 4, 8, 9, 1, 3, 5 };
            int[] indexes = new[] { 3, 2, 6, 1, 5, 0, 4 };
            bool[] expected = new[] { false, false, false, false, true, false, true, false, false, false, false, false, false, false };

            bool[] encodedFingerprint = fingerprintDescriptor.EncodeFingerprint(concatenatedSpectrumPowers, indexes, 2);

            for (int i = 0; i < encodedFingerprint.Length; i++)
            {
                Assert.AreEqual(expected[i], encodedFingerprint[i]);
            }
        }

        [TestMethod]
        public void DecodeFingerprintWorksAsExpected()
        {
            double[] expected = new double[] { 0, 0, 1, -1, 0, 0, 0 };
            
            double[] decoded = fingerprintDescriptor.DecodeFingerprint(new[] { false, false, false, false, true, false, false, true, false, false, false, false, false, false });

            for (int i = 0; i < decoded.Length; i++)
            {
                Assert.IsTrue(Math.Abs(expected[i] - decoded[i]) < Epsilon);
            }
        }
    }
}
