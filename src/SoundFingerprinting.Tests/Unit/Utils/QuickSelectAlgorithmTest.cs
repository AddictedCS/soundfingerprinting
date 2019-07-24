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
        private readonly QuickSelectAlgorithm algorithm = new QuickSelectAlgorithm();

        [Test]
        public void ShouldProperlyTestAccuracy()
        {
            var random = new Random();
            float a = (float)random.NextDouble();
            float b = a + 0.0000001f;

            Assert.IsTrue(Math.Abs(a).CompareTo(Math.Abs(b)) < 0);
        }

        [Test]
        public void ShouldGenerateSameSignatureDuringMultipleRuns()
        {
            const int count = 4096;
            const int topWavelets = 200;
            var random = new Random();
            for (int run = 0; run < 50000; ++run)
            {
                (float[] a, float[] b) = GetTwoRandomCopies(count, random);

                ushort[] indexes1 = Range(0, count);
                ushort[] indexes2 = Range(0, count);

                int akth = algorithm.Find(topWavelets - 1, a, indexes1, 0, a.Length - 1);
                int bkth = algorithm.Find(topWavelets - 1, b, indexes2, 0, b.Length - 1);

                Assert.AreEqual(akth, bkth);
                AssertFingerprintsAreSame(topWavelets, a, b);
            }
        }

        private static void AssertFingerprintsAreSame(int topWavelets, float[] a, float[] b)
        {
            var aset = new HashSet<float>();
            var bset = new HashSet<float>();

            for (int i = 0; i < topWavelets; ++i)
            {
                aset.Add(a[i]);
                bset.Add(b[i]);
            }

            var adistinct = aset.Except(bset).ToList();
            var bdistinct = bset.Except(aset);
            Assert.AreEqual(0, adistinct.Count, "Not matched: " + string.Join(",", adistinct.Union(bdistinct)));
        }

        [Test]
        public void ShouldFindTop200Element()
        {
            const int count = 4096;
            const int topWavelets = 200;

            var random = new Random();
            for (int run = 0; run < 10; ++run)
            {
                float[] floats = GenerateRandomArray(count, random);
                ushort[] indexes = Enumerable.Range(0, count).Select(i => (ushort)i).ToArray();

                int kth = algorithm.Find(topWavelets - 1, floats, indexes, 0, floats.Length - 1);

                Assert.AreEqual(topWavelets - 1, kth);
                for (int i = 0; i < topWavelets; ++i)
                {
                    for (int j = topWavelets; j < floats.Length; ++j)
                    {
                        Assert.AreEqual(1, Math.Abs(floats[i]).CompareTo(floats[j]), $"{floats[i]} < {floats[j]} at i:{i}, j:{j}");
                    }
                }
            }
        }

        [Test]
        public void ShouldProperlySelectCorrectKthOrderStatistic()
        {
            for (int i = 0; i < 10; ++i)
            {
                float[] values = { 3, 4, 5, 1, 6, 7, 8, 9, 2, 0 };
                int value = algorithm.Find(i, values, Enumerable.Range(0, 10).Select(k => (ushort)k).ToArray(), 0, values.Length - 1);

                Assert.AreEqual(value, i);
            }
        }

        private (float[], float[]) GetTwoRandomCopies(int count, Random random)
        {
            float[] a = GenerateRandomArray(count, random);
            return (a, (float[]) a.Clone());
        }
        
        private float[] GenerateRandomArray(int count, Random random)
        {
            float[] rand = new float[count];
            for (int i = 0; i < count; ++i)
            {
                rand[i] = (float)random.NextDouble();
                rand[i] *= random.NextDouble() > 0.5 ? -1 : 1;
            }

            return rand;
        }

        private static ushort[] Range(int from, int to)
        {
            return Enumerable.Range(from, to).Select(i => (ushort)i).ToArray();
        }
    }
}
