namespace SoundFingerprinting.Tests.Unit.Utils
{
    using System;
    using System.Linq;

    using NUnit.Framework;

    using SoundFingerprinting.Utils;

    [TestFixture]
    public class FastFingerprintDescriptorTest
    {
        private readonly Random random = new Random((int)DateTime.Now.Ticks << 4);

        [Test]
        public void ShouldFindTop200Element()
        {
            var descriptor = new FastFingerprintDescriptor();

            const int Count = 4096;
            float[] floats = Enumerable.Range(0, Count).Select(elem => elem % 2 == 0 ? (float)elem : (float)-elem).ToArray();
            const int TopWavelets = 200;
            int[] indexes = Enumerable.Range(0, Count).ToArray();
            int kth = descriptor.Find(
                TopWavelets - 1,
                floats,
                indexes,
                0,
                4095);

            Assert.AreEqual(TopWavelets - 1, kth);
            for (int i = 1; i < TopWavelets; ++i)
            {
                Assert.IsTrue(Math.Abs(floats[TopWavelets - i]).CompareTo(floats[TopWavelets + i - 1]) > 0);
                Assert.IsTrue(indexes[i - 1] > Count - TopWavelets);
            }
        }

        [Test]
        public void ShouldFindTop200BoundaryTest()
        {
            var descriptor = new FastFingerprintDescriptor();
            const int TopWavelets = 200;

            float[] topElements =
                Enumerable.Repeat(1, TopWavelets).Select(elem => (float)elem).ToList().Concat(
                    Enumerable.Repeat(0, 3896).Select(elem => (float)elem)).ToArray();

            float[] randomized = topElements.OrderBy(x => random.Next(0, topElements.Length)).ToArray();

            int kth = descriptor.Find(
                TopWavelets - 1,
                randomized,
                Enumerable.Range(0, randomized.Length).ToArray(),
                0,
                randomized.Length - 1);

            Assert.AreEqual(TopWavelets - 1, kth);

            for (int i = 0; i < TopWavelets; i++)
            {
                Assert.AreEqual(1, randomized[i]);
            }
        }
    }
}
