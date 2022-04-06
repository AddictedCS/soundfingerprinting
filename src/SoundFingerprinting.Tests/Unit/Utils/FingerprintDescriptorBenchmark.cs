namespace SoundFingerprinting.Tests.Unit.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;
    using NLog.Extensions.Logging;
    using NUnit.Framework;
    using SoundFingerprinting.Utils;

    [TestFixture]
    public class FingerprintDescriptorBenchmark
    {
        private readonly ILogger<FingerprintDescriptorBenchmark> logger = new NLogLoggerFactory().CreateLogger<FingerprintDescriptorBenchmark>();
        private readonly Random random = new ();

        private readonly FingerprintDescriptor fingerprintDescriptor = new ();
        private readonly FastFingerprintDescriptor fastFingerprintDescriptor  = new ();

        [Test]
        public void ShouldFindTopWaveletsFaster()
        {
            // Warm-Up
            RunTest(GetPoolOfFingerprints(1000, 128, 32), fingerprintDescriptor);

            long a = BenchMark(10000, fingerprintDescriptor);

            // Warm-Up
            RunTest(GetPoolOfFingerprints(1000, 128, 32), fastFingerprintDescriptor);

            long b = BenchMark(10000, fastFingerprintDescriptor);

            logger.LogInformation("Fingerprint Descriptor Runs: {0} ms", a);
            logger.LogInformation("Fast Fingerprint Descriptor Runs: {0} ms", b);
            logger.LogInformation("Ratio: {0}x", (double)a / b);

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
            const int topWavelets = 200;
            foreach (var floats in pool)
            {
                descriptor.ExtractTopWavelets(floats, topWavelets, RangeUtils.GetRange(floats.Length));
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
