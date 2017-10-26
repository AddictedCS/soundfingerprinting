namespace SoundFingerprinting.Tests.Unit.Strides
{
    using NUnit.Framework;

    using SoundFingerprinting.Strides;

    [TestFixture]
    public class IncrementalRandomStrideTest
    {
        [Test]
        public void ShouldProvideCorrectFirstStride()
        {
            var firstStride = 10000;
            var stride = new IncrementalRandomStride(256, 512, firstStride, 0);

            int result = stride.FirstStride;

            Assert.AreEqual(firstStride, result);
        }
    }
}
