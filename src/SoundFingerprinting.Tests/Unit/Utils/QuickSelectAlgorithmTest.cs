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
        [Test]
        public void ShouldProperlyTestAccuracy()
        {
            var random = new Random();
            float a = (float)random.NextDouble();
            float b = a + 0.0000001f;

			Assert.That(Math.Abs(a).CompareTo(Math.Abs(b)) < 0, Is.True);
        }

        [Test]
        public void ShouldGenerateSameSignatureDuringMultipleRuns()
        {
            const int count = 4096;
            const int topWavelets = 200;
            var random = new Random();
            for (int run = 0; run < 10000; ++run)
            {
                (float[] a, float[] b) = GetTwoRandomCopies(count, random);
                ushort[] indexes1 = Range(0, count);
                ushort[] indexes2 = Range(0, count);

                int akth = QuickSelectAlgorithm.Find(topWavelets - 1, a, indexes1, 0, a.Length - 1);
                int bkth = QuickSelectAlgorithm.Find(topWavelets - 1, b, indexes2, 0, b.Length - 1);

				Assert.That(bkth, Is.EqualTo(akth));
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
			Assert.That(adistinct.Count, Is.EqualTo(0), "Not matched: " + string.Join(",", adistinct.Union(bdistinct)));
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

                int kth = QuickSelectAlgorithm.Find(topWavelets - 1, floats, indexes, 0, floats.Length - 1);

				Assert.That(kth, Is.EqualTo(topWavelets - 1));
                for (int i = 0; i < topWavelets; ++i)
                {
                    for (int j = topWavelets; j < floats.Length; ++j)
                    {
						Assert.That(Math.Abs(floats[i]).CompareTo(floats[j]), Is.EqualTo(1), $"{floats[i]} < {floats[j]} at i:{i}, j:{j}");
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
                int value = QuickSelectAlgorithm.Find(i, values, Enumerable.Range(0, 10).Select(k => (ushort)k).ToArray(), 0, values.Length - 1);

				Assert.That(i, Is.EqualTo(value));
            }
        }

        [Test]
        public void SelectNthSmallestShouldMatchFullSortAcrossAllPositions()
        {
            var rng = new Random(42);
            for (int trial = 0; trial < 50; ++trial)
            {
                var original = Enumerable.Range(0, 128).Select(_ => rng.NextDouble()).ToArray();
                var sorted = original.OrderBy(x => x).ToArray();

                for (int k = 0; k < original.Length; ++k)
                {
                    var copy = (double[])original.Clone();
                    var picked = QuickSelectAlgorithm.SelectNthSmallest(copy, k);
                    Assert.That(picked, Is.EqualTo(sorted[k]), $"trial {trial} k {k}");
                    Assert.That(copy[k], Is.EqualTo(sorted[k]), "value at index k must be the kth smallest after partition");
                }
            }
        }

        [Test]
        public void SelectNthSmallestShouldEnforcePartitionInvariant()
        {
            // every element before k must be <= values[k], every element after must be >= values[k]
            var rng = new Random(7);
            var values = Enumerable.Range(0, 64).Select(_ => rng.NextDouble()).ToArray();
            const int k = 31;

            var picked = QuickSelectAlgorithm.SelectNthSmallest(values, k);

            for (int i = 0; i < k; ++i)
            {
                Assert.That(values[i], Is.LessThanOrEqualTo(picked), $"prefix index {i}");
            }

            for (int i = k + 1; i < values.Length; ++i)
            {
                Assert.That(values[i], Is.GreaterThanOrEqualTo(picked), $"suffix index {i}");
            }
        }

        [Test]
        public void SelectNthSmallestHandlesSingletonAndDuplicateInputs()
        {
            Assert.That(QuickSelectAlgorithm.SelectNthSmallest(new[] { 0.42 }, 0), Is.EqualTo(0.42));

            var allSame = Enumerable.Repeat(0.5, 17).ToArray();
            Assert.That(QuickSelectAlgorithm.SelectNthSmallest(allSame, 8), Is.EqualTo(0.5));

            var twoVals = new[] { 0.1, 0.9 };
            Assert.That(QuickSelectAlgorithm.SelectNthSmallest((double[])twoVals.Clone(), 0), Is.EqualTo(0.1));
            Assert.That(QuickSelectAlgorithm.SelectNthSmallest((double[])twoVals.Clone(), 1), Is.EqualTo(0.9));
        }

        private Tuple<float[], float[]> GetTwoRandomCopies(int count, Random random)
        {
            float[] a = GenerateRandomArray(count, random);
            return Tuple.Create(a, (float[]) a.Clone());
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
