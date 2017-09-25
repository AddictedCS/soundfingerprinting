namespace SoundFingerprinting.Tests.Unit.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    using NUnit.Framework;

    using SoundFingerprinting.Utils;

    [TestFixture]
    public class FingerprintDescriptorBenchmark
    {
        private readonly Random random = new Random();

        private readonly FingerprintDescriptor fingerprintDescriptor = new FingerprintDescriptor();
        private readonly FastFingerprintDescriptor fastFingerprintDescriptor  = new FastFingerprintDescriptor();

        [Test]
        public void ShouldFindTopWaveletsFaster()
        {
            // Warm-Up
            RunTest(GetPoolOfFingerprints(1000, 128, 32), fingerprintDescriptor);

            long a = BenchMark(10000, fingerprintDescriptor);

            // Warm-Up
            RunTest(GetPoolOfFingerprints(1000, 128, 32), fastFingerprintDescriptor);

            long b = BenchMark(10000, fastFingerprintDescriptor);

            Console.WriteLine("Fingerprint Descriptor Runs: {0} ms", a);
            Console.WriteLine("Fast Fingerprint Descriptor Runs: {0} ms", b);
            Console.WriteLine("Ratio: {0}x", (double)a / b);

            Assert.IsTrue(a > b);
        }

        private long BenchMark(int runs, FingerprintDescriptor descriptor)
        {
            var pool = GetPoolOfFingerprints(runs, 128, 32);
            var stopWatch = Stopwatch.StartNew();
            RunTest(pool, descriptor);
            stopWatch.Stop();
            return stopWatch.ElapsedMilliseconds;
        }

        private void RunTest(IEnumerable<float[]> pool, FingerprintDescriptor descriptor)
        {
            const int TopWavelets = 200;
            foreach (var floats in pool)
            {
                descriptor.ExtractTopWavelets(floats, TopWavelets, RangeUtils.GetRange(floats.Length));
            }
        }

        private IEnumerable<float[]> GetPoolOfFingerprints(int count, int rows, int cols)
        {
            var pool = new List<float[]>();
            for (int i = 0; i < count; i++)
            {
                float[] fingerprint = new float[rows * cols];
                for (int j = 0; j < rows * cols; ++j)
                {
                    fingerprint[j] = (float)random.NextDouble();
                }

                pool.Add(fingerprint);
            }

            return pool;
        }
    }
}
