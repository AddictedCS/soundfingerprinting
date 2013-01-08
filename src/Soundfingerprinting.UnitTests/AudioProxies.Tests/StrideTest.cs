namespace Soundfingerprinting.UnitTests.AudioProxies.Tests
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Soundfingerprinting.Audio.Strides;

    [TestClass]
    public class StrideClassesTest : BaseTest
    {
        [TestMethod]
        public void StaticStrideClassTest()
        {
            const int Value = 5115;
            StaticStride stride = new StaticStride(Value);
            Assert.AreEqual(Value, stride.StrideSize);
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
                int skip = randomStride.StrideSize;
                Assert.IsTrue(skip <= Max);
                Assert.IsTrue(skip >= Min);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RandomStrideClassBadMinMaxTest()
        {
            RandomStride randomStride = new RandomStride(253, 0);
        }
    }
}