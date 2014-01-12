namespace SoundFingerprinting.Tests.Unit.FFT.FFTW
{
    using System;
    using System.Runtime.InteropServices;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SoundFingerprinting.FFT.FFTW;

    [TestClass]
    public class CachedFFTWServiceTest : AbstractTest
    {
        private CachedFFTWService cachedFFTWService;

        private Mock<FFTWService> fftwService;

        [TestInitialize]
        public void SetUp()
        {
            fftwService = new Mock<FFTWService>(MockBehavior.Strict);

            cachedFFTWService = new CachedFFTWService(fftwService.Object);
        }

        [TestCleanup]
        public void TearDown()
        {
            fftwService.VerifyAll();
        }

        [TestMethod]
        public void FFTWAlgorithmCalledSeveralTimesWithCachedFFTWPlans()
        {
            const int FFTLength = 2048;
            const int NumberOfInvocations = 5;
            float[] signal = TestUtilities.GenerateRandomFloatArray(FFTLength * NumberOfInvocations);

            IntPtr input = Marshal.AllocHGlobal(4 * 2048);
            IntPtr output = Marshal.AllocHGlobal(8 * 1024);
            IntPtr plan = Marshal.AllocHGlobal(8 * 1024);

            fftwService.Setup(service => service.GetInput(FFTLength)).Returns(input);
            fftwService.Setup(service => service.GetOutput(FFTLength)).Returns(output);
            fftwService.Setup(service => service.GetFFTPlan(FFTLength, input, output)).Returns(plan);
            fftwService.Setup(service => service.Execute(plan));

            for (int i = 0; i < NumberOfInvocations; i++)
            {
                cachedFFTWService.FFTForward(signal, FFTLength * i, FFTLength);
            }

            fftwService.Verify(service => service.GetInput(FFTLength), Times.Once());
            fftwService.Verify(service => service.GetOutput(FFTLength), Times.Once());
            fftwService.Verify(service => service.GetFFTPlan(FFTLength, input, output), Times.Once());
            fftwService.Verify(service => service.Execute(plan), Times.Exactly(NumberOfInvocations));

            Marshal.FreeHGlobal(input);
            Marshal.FreeHGlobal(output);
            Marshal.FreeHGlobal(plan);
        }
    }
}
