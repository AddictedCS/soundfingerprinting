namespace SoundFingerprinting.Tests.Unit.FFT
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using NUnit.Framework;

    using SoundFingerprinting.FFT;

    [TestFixture]
    public class LomontFFTTest
    {
        private readonly LomontFFT lomontFFT = new LomontFFT();

        private readonly IComparer floatComparer = Comparer<float>.Create(
            (a, b) =>
                {
                    if (Math.Abs(a - b) < 0.001)
                    {
                        return 0;
                    }

                    return a.CompareTo(b);
                });
        [Test]
        public void SineWaveTest()
        {
            var sineWave = new[] { 0, 0.707f, 1, 0.707f, 0, -0.707f, -1, -0.707f };
            unsafe
            {
                float* input = stackalloc float[sineWave.Length];
                for (int i = 0; i < sineWave.Length; ++i)
                {
                    input[i] = sineWave[i];
                }

                lomontFFT.RealFFT(input, true, sineWave.Length);
                lomontFFT.RealFFT(input, false, sineWave.Length);

                for (int i = 0; i < sineWave.Length; ++i)
                {
                    sineWave[i] = input[i];
                }
            }

            CollectionAssert.AreEqual(new[] { 0, 0.707f, 1, 0.707f, 0, -0.707f, -1, -0.707f }, sineWave, floatComparer);
        }

        [Test]
        public void SineWaveTest2()
        {
            var sineWave = new[] { 0, 0.707f, 1, 0.707f, 0, -0.707f, -1, -0.707f };
            unsafe
            {
                float* input = stackalloc float[sineWave.Length];
                for (int i = 0; i < sineWave.Length; ++i)
                {
                    input[i] = sineWave[i];
                }

                lomontFFT.RealFFT(input, true, sineWave.Length);

                for (int i = 0; i < sineWave.Length; ++i)
                {
                    sineWave[i] = input[i];
                }
            }

            CollectionAssert.AreEqual(new[] { 0, 0, 0, 4f, 0, 0, 0, 0 }, sineWave, floatComparer);
        }
    }
}
