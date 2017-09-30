namespace SoundFingerprinting.Tests.Unit.Benchmarks
{
    using System;
    using System.Collections.Generic;

    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Running;

    using NUnit.Framework;

    using SoundFingerprinting.Utils;

    [TestFixture]
    public class FingerprintDescriptorBenchmark
    {
        private readonly Random random = new Random();

        private readonly FingerprintDescriptor fingerprintDescriptor = new FingerprintDescriptor();
        private readonly FastFingerprintDescriptor fastFingerprintDescriptor  = new FastFingerprintDescriptor();

        [Test]
        public void RunBenchmark()
        {
            var summary = BenchmarkRunner.Run<FingerprintDescriptorBenchmark>();
            Console.WriteLine(summary.ToString());
        }

        [Benchmark]
        public void NaiveFingerprintDescriptor()
        {
            this.RunMultipleTimes(10, this.fingerprintDescriptor);
        }

        [Benchmark]
        public void FastFingerprintDescriptor()
        {
            this.RunMultipleTimes(10, this.fastFingerprintDescriptor);
        }

        private void RunMultipleTimes(int runs, FingerprintDescriptor descriptor)
        {
            var pool = this.GetPoolOfFingerprints(runs, 128, 32);
            this.RunTest(pool, descriptor);
        }

        private void RunTest(IEnumerable<float[][]> pool, FingerprintDescriptor descriptor)
        {
            const int TopWavelets = 200;
            foreach (var floats in pool)
            {
                descriptor.ExtractTopWavelets(floats, TopWavelets);
            }
        }

        private IEnumerable<float[][]> GetPoolOfFingerprints(int count, int rows, int cols)
        {
            var pool = new List<float[][]>();
            for (int i = 0; i < count; i++)
            {
                float[][] fingerprint = new float[rows][];
                for (int j = 0; j < rows; ++j)
                {
                    fingerprint[j] = new float[cols];
                    for (int k = 0; k < cols; ++k)
                    {
                        fingerprint[j][k] = (float)this.random.NextDouble();
                    }
                }

                pool.Add(fingerprint);
            }

            return pool;
        }
    }
}
