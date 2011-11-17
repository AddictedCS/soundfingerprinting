// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Soundfingerprinting.AudioProxies.Strides;

namespace Soundfingerprinting.UnitTests.AudioProxies.Tests
{
    /// <summary>
    ///   Test stride class
    /// </summary>
    [TestClass]
    public class StrideClassesTest : BaseTest
    {
        /// <summary>
        ///   Test static stride
        /// </summary>
        [TestMethod]
        public void StaticStrideClassTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            const int value = 5115;
            StaticStride stride = new StaticStride(value);
            Assert.AreEqual(value, stride.GetStride());

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Test random stride class
        /// </summary>
        [TestMethod]
        public void RandomStrideClassTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            const int min = 0;
            const int max = 253;
            RandomStride randomStride = new RandomStride(min, max);
            const int count = 1024;
            for (int i = 0; i < count; i++)
            {
                int skip = randomStride.GetStride();
                Assert.IsTrue(skip <= max);
                Assert.IsTrue(skip >= min);
            }

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Test RandomStride class with bad Min/Max
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof (ArgumentException))]
        public void RandomStrideClassBadMinMaxTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            RandomStride randomStride = new RandomStride(253, 0);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }
    }
}