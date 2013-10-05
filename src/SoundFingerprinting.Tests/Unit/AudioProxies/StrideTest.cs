namespace SoundFingerprinting.Tests.Unit.AudioProxies
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Strides;

    [TestClass]
    public class StrideClassesTest : AbstractTest
    {
        [TestMethod]
        public void StaticStrideClassTest()
        {
            const int Value = 5115;
            StaticStride stride = new StaticStride(Value);
            Assert.AreEqual(Value, stride.GetNextStride());
        }

        [TestMethod]
        public void IncrementalStaticStrideTest()
        {
            IncrementalStaticStride incrementalStatic = new IncrementalStaticStride(5115, SamplesPerFingerprint);
            Assert.AreEqual(5115 - SamplesPerFingerprint, incrementalStatic.GetNextStride());
        }

        [TestMethod]
        public void RandomStrideClassTest()
        {
            const int Min = 0;
            const int Max = 253;
            RandomStride randomStride = new RandomStride(Min, Max);
            const int Count = 1024;
            for (int i = 0; i < Count; i++)
            {
                int skip = randomStride.GetNextStride();
                Assert.IsTrue(skip <= Max);
                Assert.IsTrue(skip >= Min);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RandomStrideClassBadMinMaxTest()
        {
// ReSharper disable ObjectCreationAsStatement
            new RandomStride(253, 0);
// ReSharper restore ObjectCreationAsStatement
        }
    }
}