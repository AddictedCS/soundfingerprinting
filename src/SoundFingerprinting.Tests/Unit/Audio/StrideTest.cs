namespace SoundFingerprinting.Tests.Unit.Audio
{
    using System;
    using NUnit.Framework;

    using SoundFingerprinting.Strides;

    [TestFixture]
    public class StrideClassesTest
    {
        [Test]
        public void IncrementalStaticStrideTest()
        {
            IncrementalStaticStride incrementalStatic = new IncrementalStaticStride(5115);
            Assert.That(incrementalStatic.NextStride, Is.EqualTo(5115));
        }

        [Test]
        public void IncrementalRandomStrideTest()
        {
            const int min = 0;
            const int max = 253;
            var randomStride = new IncrementalRandomStride(min, max);
            const int count = 1024;
            for (int i = 0; i < count; i++)
            {
                int skip = randomStride.NextStride;
                Assert.That(skip <= max, Is.True);
                Assert.That(skip >= min, Is.True);
            }
        }

        [Test]
        public void ShouldThrowOnInvalidArguments()
        {
            Assert.That((, Throws.TypeOf<ArgumentException>()) => new IncrementalRandomStride(100, 0));
        }
    }
}
