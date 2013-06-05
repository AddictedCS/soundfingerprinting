namespace Soundfingerprinting.UnitTests.Fingerprinting.Tests.Windows
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Soundfingerprinting.Windows;

    [TestClass]
    public class HanningWindowTest : BaseTest
    {
        private IWindowFunction windowFunction;

        [TestInitialize]
        public void SetUp()
        {
            windowFunction = new HanningWindow();
        }

        [TestMethod]
        public void WindowInPlaceTest()
        {
            const int Length = 128 * 64;
            float[] outerspace = TestUtilities.GenerateRandomDoubleArray(Length);
            float[] outerspaceCopy = new float[outerspace.Length];
            outerspace.CopyTo(outerspaceCopy, 0);

            windowFunction.WindowInPlace(outerspace, outerspace.Length);
            WeightByHanningWindow(outerspaceCopy);

            for (int i = 0; i < outerspace.Length; i++)
            {
                Assert.AreEqual(true, (outerspace[i] - outerspaceCopy[i]) < 0.00001);
            }
        }

        private void WeightByHanningWindow(float[] outerspace)
        {
            for (int i = 0; i < outerspace.Length; i++)
            {
                outerspace[i] *= (float)(0.5 * (1 - Math.Cos(Math.PI * 2 * i / (outerspace.Length - 1))));
            }
        }
    }
}