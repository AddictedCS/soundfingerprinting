namespace SoundFingerprinting.Tests.Unit.FFT
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.FFT;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Strides;

    [TestClass]
    public class SpectrumServiceTest : AbstractTest
    {
        private ISpectrumService spectrumService;

        private Mock<IFFTService> fftService;

        private Mock<IAudioSamplesNormalizer> audioSamplesNormalizer;

        private Mock<ILogUtility> logUtility;

        [TestInitialize]
        public void SetUp()
        {
            fftService = new Mock<IFFTService>(MockBehavior.Strict);
            DependencyResolver.Current.Bind<IFFTService, IFFTService>(fftService.Object);

            audioSamplesNormalizer = new Mock<IAudioSamplesNormalizer>(MockBehavior.Strict);
            DependencyResolver.Current.Bind<IAudioSamplesNormalizer, IAudioSamplesNormalizer>(
                audioSamplesNormalizer.Object);

            logUtility = new Mock<ILogUtility>(MockBehavior.Strict);
            DependencyResolver.Current.Bind<ILogUtility, ILogUtility>(logUtility.Object);

            spectrumService = new SpectrumService();
        }

        [TestCleanup]
        public void TearDown()
        {
            fftService.VerifyAll();
            audioSamplesNormalizer.VerifyAll();
            logUtility.VerifyAll();
        }
        
        [TestMethod]
        public void CreateLogSpectrogramTest()
        {
            float[] samples = TestUtilities.GenerateRandomFloatArray(5512 * 10); // 10 seconds
            var configuration = new CustomFingerprintConfiguration { NormalizeSignal = true, };

            audioSamplesNormalizer.Setup(service => service.NormalizeInPlace(samples));
            logUtility.Setup(utility => utility.GenerateLogFrequenciesRanges(configuration)).Returns(new int[33]);
            fftService.Setup(service => service.FFTForward(samples, It.IsAny<int>(), configuration.WdftSize)).Returns(
                TestUtilities.GenerateRandomFloatArray(2048));

            float[][] result = spectrumService.CreateLogSpectrogram(samples, configuration);

            audioSamplesNormalizer.Verify(service => service.NormalizeInPlace(samples), Times.Once());
            logUtility.Verify(utility => utility.GenerateLogFrequenciesRanges(configuration), Times.Once());
            Assert.AreEqual(829, result.Length);
            Assert.AreEqual(32, result[0].Length);
        }

        [TestMethod]
        public void CutLogarithmizedSpectrumTest()
        {
            const int LogSpectrumLength = 829; // corresponds to 10 seconds input
            var stride = new StaticStride(0, 0);
            var logSpectrum = new float[LogSpectrumLength][];
            for (int i = 0; i < LogSpectrumLength; i++)
            {
                logSpectrum[i] = new float[32];
            }

            var cutLogarithmizedSpectrum = spectrumService.CutLogarithmizedSpectrum(logSpectrum, stride, 128, 64);
            
            Assert.AreEqual((int)(10 / 1.48 /*granularity on 1 fingerprint*/), cutLogarithmizedSpectrum.Count);
        }
    }
}
