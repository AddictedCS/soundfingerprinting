// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Soundfingerprinting.Fingerprinting.ConstantQ;
using Soundfingerprinting.Fingerprinting.FFT;
using Soundfingerprinting.Fingerprinting.Windows;

namespace Soundfingerprinting.UnitTests.Fingerprinting.Tests.ConstantQ
{
    ///<summary>
    ///  This is a test class for SparKernelTest and is intended
    ///  to contain all SparKernelTest Unit Tests
    ///</summary>
    [TestClass]
    public class SparKernelTest : BaseTest
    {
        ///<summary>
        ///  A test for SparKernelVector
        ///</summary>
        [TestMethod]
        public void SparKernelVectorTest()
        {
            const float minFreq = 318;
            const float maxFreq = 2000;
            const int bins = 12;
            const float frequencyRate = 5512;
            const float threshold = 0.0054f;
            IWindowFunction function = new HanningWindow();
            SparKernel target = new SparKernel(minFreq, maxFreq, bins, frequencyRate, threshold, function);
            Complex[][] actual = target.SparKernelVector;
            Assert.IsNotNull(actual);
        }

        ///<summary>
        ///  A test for SparKernel Constructor
        ///</summary>
        [TestMethod]
        public void SparKernelConstructorTest()
        {
            const float minFreq = 318F;
            const float maxFreq = 2000F;
            const int bins = 12;
            const float frequencyRate = 5512F;
            const float threshold = 0.0054F;
            IWindowFunction function = new HanningWindow();
            SparKernel target = new SparKernel(minFreq, maxFreq, bins, frequencyRate, threshold, function);
            Assert.IsNotNull(target.SparKernelVector);
        }
    }
}