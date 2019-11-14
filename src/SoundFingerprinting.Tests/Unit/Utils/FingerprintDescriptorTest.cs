namespace SoundFingerprinting.Tests.Unit.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using NUnit.Framework;

    using SoundFingerprinting.Utils;

    [TestFixture]
    public class FingerprintDescriptorTest
    {
        private const int TopWavelets = 200;

        private IFingerprintDescriptor fingerprintDescriptor;

        private IFingerprintEncoder fingerprintEncoder;

        [SetUp]
        public void SetUp()
        {
            fingerprintDescriptor = new FingerprintDescriptor();
            fingerprintEncoder = new FingerprintEncoder();
        }

        [Test]
        public void ExtractTopWaveletsText()
        {
            float[] frames = TestUtilities.GenerateRandomFloatArray(128 * 32);
            float[] copy = (float[])frames.Clone();
            bool[] actual = fingerprintDescriptor.ExtractTopWavelets(frames, TopWavelets, RangeUtils.GetRange(128 * 32)).ToBools();
            bool[] expected = ExtractTopWaveletsTested(copy, TopWavelets);

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void ExtractTopWaveletWorksCorrectly()
        {
            float[] frames = { 5, 6, 1, 8, 9, 2, 0, -4, 6, -10, 7, 3, 1, 0, 2, 5, -11, 0, 5, 13, 7, 6, 3, 2 };
            bool[] expected =
                {
                    false, false, false, false, false, false, true, false, true, false, false, false, false, false,
                    false, false, false, false, false, true, false, false, false, false, false, false, false, false,
                    false, false, false, false, false, true, false, false, false, false, true, false, false, false,
                    false, false, false, false, false, false
                };

            bool[] encodedFingerprint = fingerprintDescriptor.ExtractTopWavelets(frames, 5, RangeUtils.GetRange(frames.Length)).ToBools();

            CollectionAssert.AreEqual(expected, encodedFingerprint);
        }

        [Test]
        public void EncodeFingerprintWorksAsExpected()
        {
            float[] framesSpectrumPowers = { 2, 4, 8, 9, 1, 3, 5 };
            ushort[] indexes = { 3, 2, 6, 1, 5, 0, 4 };
            bool[] expected = { false, false, false, false, true, false, true, false, false, false, false, false, false, false };

            bool[] encodedFingerprint = fingerprintEncoder.EncodeFingerprint(framesSpectrumPowers, indexes, 2).ToBools();

            CollectionAssert.AreEqual(expected, encodedFingerprint);
        }

        private bool[] ExtractTopWaveletsTested(float[] frames, int topWavelets)
        {
            var query = frames.Select((value, index) => new KeyValuePair<int, float>(index, value))
                                    .OrderByDescending(pair => Math.Abs(pair.Value));

            if (topWavelets >= frames.Length)
            {
                throw new ArgumentException("TopWavelets cannot exceed the length of frames array");
            }

            // Negative Numbers = 01
            // Positive Numbers = 10
            // Zeros            = 00
            // Concatenated float array
            bool[] result = new bool[frames.Length * 2]; 
            int topW = 0;
            foreach ((int key, float f) in query)
            {
                if (++topW > topWavelets)
                {
                    break;
                }

                double value = f;
                if (value > 0)
                {
                    result[key * 2] = true;
                }
                else if (value < 0)
                {
                    result[(key * 2) + 1] = true;
                }
            }

            return result;
        }
    }
}
