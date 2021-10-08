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
            Assert.AreEqual(5115, incrementalStatic.NextStride);
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
                Assert.IsTrue(skip <= max);
                Assert.IsTrue(skip >= min);
            }
        }

        [Test]
        public void ShouldThrowOnInvalidArguments()
        {
            Assert.Throws<ArgumentException>(() => new IncrementalRandomStride(100, 0));
        }
    }
}
