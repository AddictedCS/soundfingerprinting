namespace SoundFingerprinting.Tests.Unit.Audio
{
    using System;

    using NUnit.Framework;

    using SoundFingerprinting.Strides;

    [TestFixture]
    public class StrideClassesTest : AbstractTest
    {
        [Test]
        public void StaticStrideClassTest()
        {
            const int Value = 5115;
            StaticStride stride = new StaticStride(Value);
            Assert.AreEqual(Value, stride.NextStride);
        }

        [Test]
        public void IncrementalStaticStrideTest()
        {
            IncrementalStaticStride incrementalStatic = new IncrementalStaticStride(5115);
            Assert.AreEqual(5115 - 8192, incrementalStatic.NextStride);
        }

        [Test]
        public void RandomStrideClassTest()
        {
            const int Min = 0;
            const int Max = 253;
            RandomStride randomStride = new RandomStride(Min, Max, 0);
            const int Count = 1024;
            for (int i = 0; i < Count; i++)
            {
                int skip = randomStride.NextStride;
                Assert.IsTrue(skip <= Max);
                Assert.IsTrue(skip >= Min);
            }
        }

        [Test]
        public void RandomStrideClassBadMinMaxTest()
        {
            Assert.Throws<ArgumentException>(() => new RandomStride(253, 0, 0));
        }
    }
}
