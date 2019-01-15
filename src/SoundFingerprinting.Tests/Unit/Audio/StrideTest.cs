namespace SoundFingerprinting.Tests.Unit.Audio
{
    using System;

    using NUnit.Framework;

    using SoundFingerprinting.Strides;

    [TestFixture]
    public class StrideClassesTest
    {
        [Test]
        public void StaticStrideClassTest()
        {
            const int value = 5115;
            StaticStride stride = new StaticStride(value);
            Assert.AreEqual(8192 + value, stride.NextStride);
        }

        [Test]
        public void IncrementalStaticStrideTest()
        {
            IncrementalStaticStride incrementalStatic = new IncrementalStaticStride(5115);
            Assert.AreEqual(5115, incrementalStatic.NextStride);
        }

        [Test]
        public void RandomStrideClassTest()
        {
            const int min = 0;
            const int Max = 253;
            RandomStride randomStride = new RandomStride(min, Max, 0);
            const int count = 1024;
            for (int i = 0; i < count; i++)
            {
                int skip = randomStride.NextStride;
                Assert.IsTrue(skip <= 8192 + Max);
                Assert.IsTrue(skip >= 8192 + min);
            }
        }

        [Test]
        public void RandomStrideClassBadMinMaxTest()
        {
            Assert.Throws<ArgumentException>(() => new RandomStride(253, 0, 0));
        }
    }
}
