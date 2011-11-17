// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Soundfingerprinting.Fingerprinting;

namespace Soundfingerprinting.UnitTests.Fingerprinting.Tests.Windows
{
    /// <summary>
    ///   Hanning window test
    /// </summary>
    [TestClass]
    public class HanningWindowTest : BaseTest
    {
        /// <summary>
        ///   Local copy of the algorithm
        /// </summary>
        /// <param name = "outerspace">Array to be weighted</param>
        private static void WeightByHanningWindow(float[] outerspace)
        {
            for (int i = 0; i < outerspace.Length; i++)
            {
                outerspace[i] *= (float) (0.5*(1 - Math.Cos(Math.PI*2*i/(outerspace.Length - 1))));
            }
        }

        /// <summary>
        ///   Apply window function
        /// </summary>
        [TestMethod]
        public void WindowInPlaceTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            FingerprintManager manager = new FingerprintManager();
            const int length = 128*64;
            float[] outerspace = TestUtilities.GenerateRandomDoubleArray(length);
            float[] outerspaceCopy = new float[outerspace.Length];
            outerspace.CopyTo(outerspaceCopy, 0);

            manager.WindowFunction.WindowInPlace(outerspace, outerspace.Length);
            WeightByHanningWindow(outerspaceCopy);

            for (int i = 0; i < outerspace.Length; i++)
                Assert.AreEqual(true, (outerspace[i] - outerspaceCopy[i]) < 0.00001);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Weight by Hanning window using small array as a parameter
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof (ArgumentException))]
        public void WeightByHanningWindowSmallArrayTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            FingerprintManager manager = new FingerprintManager();
            manager.WindowFunction.WindowInPlace(new float[1], 1);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }
    }
}