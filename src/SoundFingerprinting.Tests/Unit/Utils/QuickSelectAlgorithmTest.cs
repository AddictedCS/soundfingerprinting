namespace SoundFingerprinting.Tests.Unit.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using NUnit.Framework;

    using SoundFingerprinting.Utils;

    [TestFixture]
    public class QuickSelectAlgorithmTest
    {
        private readonly Random random = new Random();

        private readonly QuickSelectAlgorithm algorithm = new QuickSelectAlgorithm();

        [Test]
        public void ShouldProperlyTestAccuracy()
        {
            float a = (float)random.NextDouble();
            float b = a + 0.0000001f;

            Assert.IsTrue(Math.Abs(a).CompareTo(Math.Abs(b)) < 0);
        }

        [Test]
        public void ShouldGenerateSameSignatureDuringMultipleRuns()
        {
            const int Count = 4096;
            const int TopWavelets = 200;

            for (int run = 0; run < 50000; ++run)
            {
                float[] a = GenerateRandomArray(Count);
                float[] b = (float[])a.Clone();

                ushort[] x = Enumerable.Range(0, Count).Select(i => (ushort)i).ToArray();
                ushort[] y = (ushort[])x.Clone();

                int akth = algorithm.Find(TopWavelets - 1, a, x, 0, a.Length - 1);
                int bkth = algorithm.Find(TopWavelets - 1, b, y, 0, b.Length - 1);

                Assert.AreEqual(akth, bkth);
                var aset = new HashSet<float>();
                var bset = new HashSet<float>();

                for (int i = 0; i < TopWavelets; ++i)
                {
                    aset.Add(a[i]);
                    bset.Add(b[i]);
                }

                var adistinct = aset.Except(bset);
                var bdistinct = bset.Except(aset);
                Assert.AreEqual(0, adistinct.Count(), "Not matched: " + string.Join(",", adistinct.Union(bdistinct).Select(f => (double)f).ToArray()));
            }
        }


        [Test]
        public void ShouldFindTop200Element()
        {
            const int Count = 4096;
            const int TopWavelets = 200;

            for (int run = 0; run < 10; ++run)
            {
                float[] floats = GenerateRandomArray(Count);
                ushort[] indexes = Enumerable.Range(0, Count).Select(i => (ushort)i).ToArray();

                int kth = algorithm.Find(TopWavelets - 1, floats, indexes, 0, floats.Length - 1);

                Assert.AreEqual(TopWavelets - 1, kth);
                for (int i = 0; i < TopWavelets; ++i)
                {
                    for (int j = TopWavelets; j < floats.Length; ++j)
                    {
                        Assert.AreEqual(
                            1,
                            Math.Abs(floats[i]).CompareTo(floats[j]),
                            string.Format("{0} < {1} at i:{2}, j:{3}", floats[i], floats[j], i, j));
                    }
                }
            }
        }

        [Test]
        public void ShouldProperlySelectCorrectKthOrderStatistic()
        {
            for (int i = 0; i < 10; ++i)
            {
                float[] values = new float[] { 3, 4, 5, 1, 6, 7, 8, 9, 2, 0 };
                int value = algorithm.Find(i, values, Enumerable.Range(0, 10).Select(k => (ushort)k).ToArray(), 0, values.Length - 1);

                Assert.AreEqual(value, i);
            }
        }

        private float[] GenerateRandomArray(int count)
        {
            float[] rand = new float[count];
            for (int i = 0; i < count; ++i)
            {
                rand[i] = (float)random.NextDouble();
                rand[i] *= random.NextDouble() > 0.5 ? -1 : 1;
            }

            return rand;
        }
    }
}
