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

            lomontFFT.RealFFT(sineWave, true);
            lomontFFT.RealFFT(sineWave, false);

            CollectionAssert.AreEqual(new float[] { 0, 0.707f, 1, 0.707f, 0, -0.707f, -1, -0.707f }, sineWave, floatComparer);
        }

        [Test]
        public void ForwardLomontFFTOnSineWave()
        {
            var sineWave = new[] { 0, 0.707f, 1, 0.707f, 0, -0.707f, -1, -0.707f };

            lomontFFT.RealFFT(sineWave, true);

            CollectionAssert.AreEqual(new float[] { 0, 0, 0, 4f, 0, 0, 0, 0 }, sineWave, floatComparer);
        }
    }
}
