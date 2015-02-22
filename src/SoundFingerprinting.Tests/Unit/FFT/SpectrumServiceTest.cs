namespace SoundFingerprinting.Tests.Unit.FFT
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.FFT;
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
            audioSamplesNormalizer = new Mock<IAudioSamplesNormalizer>(MockBehavior.Strict);
            logUtility = new Mock<ILogUtility>(MockBehavior.Strict);
            spectrumService = new SpectrumService(fftService.Object, logUtility.Object, audioSamplesNormalizer.Object);
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
            var configuration = new CustomSpectrogramConfig { NormalizeSignal = true };
            float[] samples = TestUtilities.GenerateRandomFloatArray((configuration.Overlap * configuration.WdftSize) + configuration.WdftSize); // 64 * 2048
            
            audioSamplesNormalizer.Setup(service => service.NormalizeInPlace(samples));
            logUtility.Setup(utility => utility.GenerateLogFrequenciesRanges(5512, configuration)).Returns(new int[33]);
            fftService.Setup(service => service.FFTForward(samples, It.IsAny<int>(), configuration.WdftSize))
                      .Returns(TestUtilities.GenerateRandomFloatArray(2048));

            float[][] result = spectrumService.CreateLogSpectrogram(samples, 5512, configuration);

            audioSamplesNormalizer.Verify(service => service.NormalizeInPlace(samples), Times.Once());
            logUtility.Verify(utility => utility.GenerateLogFrequenciesRanges(5512, configuration), Times.Once());

            Assert.AreEqual(configuration.WdftSize, result.Length);
            Assert.AreEqual(32, result[0].Length);
        }

        [TestMethod]
        public void CreateLogSpectrogramFromMinimalSamplesLengthTest()
        {
            var configuration = new CustomSpectrogramConfig { NormalizeSignal = false };
            float[] samples = TestUtilities.GenerateRandomFloatArray(FingerprintConfiguration.Default.SamplesPerFingerprint + configuration.WdftSize); // 8192 + 2048

            logUtility.Setup(utility => utility.GenerateLogFrequenciesRanges(5512, configuration)).Returns(new int[33]);
            fftService.Setup(service => service.FFTForward(samples, It.IsAny<int>(), configuration.WdftSize))
                      .Returns(TestUtilities.GenerateRandomFloatArray(2048));

            float[][] result = spectrumService.CreateLogSpectrogram(samples, 5512, configuration);

            audioSamplesNormalizer.Verify(service => service.NormalizeInPlace(samples), Times.Never());
            logUtility.Verify(utility => utility.GenerateLogFrequenciesRanges(5512, configuration), Times.Once());

            Assert.AreEqual(configuration.ImageLength, result.Length); // 128
            Assert.AreEqual(32, result[0].Length);
        }

        [TestMethod]
        public void CreateLogSpectrogramFromSamplesLessThanFourierTransformWindowLength()
        {
            var configuration = SpectrogramConfig.Default;
            float[] samples = TestUtilities.GenerateRandomFloatArray(configuration.WdftSize - 1);

            logUtility.Setup(utility => utility.GenerateLogFrequenciesRanges(5512, configuration)).Returns(new int[33]);
            
            float[][] result = spectrumService.CreateLogSpectrogram(samples, 5512, configuration);

            logUtility.Verify(utility => utility.GenerateLogFrequenciesRanges(5512, configuration), Times.Once());

            Assert.AreEqual(0, result.Length); // 128
        }

        [TestMethod]
        public void CutLogarithmizedSpectrumTest()
        {
            var config = SpectrogramConfig.Default;
            const int LogSpectrumLength = 1024;
            var stride = new StaticStride(0, 0);
            var logSpectrum = new float[LogSpectrumLength][];
            for (int i = 0; i < LogSpectrumLength; i++)
            {
                logSpectrum[i] = new float[32];
            }

            var cutLogarithmizedSpectrum = spectrumService.CutLogarithmizedSpectrum(logSpectrum, 5512, stride, config);
            
            Assert.AreEqual(8, cutLogarithmizedSpectrum.Count);
        }

        [TestMethod]
        public void CutLogarithmizedSpectrumOfJustOneFingerprintTest()
        {
            var config = SpectrogramConfig.Default;
            int logSpectrumLength = config.ImageLength; // 128
            var stride = new StaticStride(0, 0);
            var logSpectrum = new float[logSpectrumLength][];
            for (int i = 0; i < logSpectrumLength; i++)
            {
                logSpectrum[i] = new float[32];
            }

            var cutLogarithmizedSpectrum = spectrumService.CutLogarithmizedSpectrum(logSpectrum, 5512, stride, config);
            
            Assert.AreEqual(1, cutLogarithmizedSpectrum.Count);
        }

        [TestMethod]
        public void CutLogarithmizedSpectrumWithAnIncrementalStaticStride()
        {
            var config = SpectrogramConfig.Default;
            int logSpectrumLength = (config.ImageLength * 24) + config.Overlap;
            var stride = new IncrementalStaticStride(FingerprintConfiguration.Default.SamplesPerFingerprint / 2, FingerprintConfiguration.Default.SamplesPerFingerprint, 0);
            var logSpectrum = new float[logSpectrumLength][];
            for (int i = 0; i < logSpectrumLength; i++)
            {
                logSpectrum[i] = new float[32];
            }

            var cutLogarithmizedSpectrum = spectrumService.CutLogarithmizedSpectrum(logSpectrum, 5512, stride, config);

            Assert.AreEqual(48, cutLogarithmizedSpectrum.Count);
        }

        [TestMethod]
        public void CutLogarithmizedSpectrumWithSpectrumWhichIsLessThanMinimalLengthOfOneFingerprintTest()
        {
            var config = SpectrogramConfig.Default;
            int logSpectrumLength = config.ImageLength - 1;
            var stride = new StaticStride(0, 0);
            var logSpectrum = new float[logSpectrumLength][];
            for (int i = 0; i < logSpectrumLength; i++)
            {
                logSpectrum[i] = new float[32];
            }

            var cutLogarithmizedSpectrum = spectrumService.CutLogarithmizedSpectrum(logSpectrum, 5512, stride, config);

            Assert.AreEqual(0, cutLogarithmizedSpectrum.Count);
        }
    }
}
