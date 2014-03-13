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
            var configuration = new CustomFingerprintConfiguration { NormalizeSignal = true };
            float[] samples = TestUtilities.GenerateRandomFloatArray((configuration.Overlap * configuration.WdftSize) + configuration.WdftSize); // 64 * 2048
            
            audioSamplesNormalizer.Setup(service => service.NormalizeInPlace(samples));
            logUtility.Setup(utility => utility.GenerateLogFrequenciesRanges(configuration)).Returns(new int[33]);
            fftService.Setup(service => service.FFTForward(samples, It.IsAny<int>(), configuration.WdftSize))
                      .Returns(TestUtilities.GenerateRandomFloatArray(2048));

            float[][] result = spectrumService.CreateLogSpectrogram(samples, configuration);

            audioSamplesNormalizer.Verify(service => service.NormalizeInPlace(samples), Times.Once());
            logUtility.Verify(utility => utility.GenerateLogFrequenciesRanges(configuration), Times.Once());

            Assert.AreEqual(configuration.WdftSize, result.Length);
            Assert.AreEqual(32, result[0].Length);
        }

        [TestMethod]
        public void CreateLogSpectrogramFromMinimalSamplesLengthTest()
        {
            var configuration = new CustomFingerprintConfiguration { NormalizeSignal = false };
            float[] samples = TestUtilities.GenerateRandomFloatArray(configuration.SamplesPerFingerprint + configuration.WdftSize); // 8192 + 2048

            logUtility.Setup(utility => utility.GenerateLogFrequenciesRanges(configuration)).Returns(new int[33]);
            fftService.Setup(service => service.FFTForward(samples, It.IsAny<int>(), configuration.WdftSize))
                      .Returns(TestUtilities.GenerateRandomFloatArray(2048));

            float[][] result = spectrumService.CreateLogSpectrogram(samples, configuration);

            audioSamplesNormalizer.Verify(service => service.NormalizeInPlace(samples), Times.Never());
            logUtility.Verify(utility => utility.GenerateLogFrequenciesRanges(configuration), Times.Once());

            Assert.AreEqual(configuration.FingerprintLength, result.Length); // 128
            Assert.AreEqual(32, result[0].Length);
        }

        [TestMethod]
        public void CreateLogSpectrogramFromSamplesLessThanFourierTransformWindowLength()
        {
            var configuration = new DefaultFingerprintConfiguration();
            float[] samples = TestUtilities.GenerateRandomFloatArray(configuration.WdftSize - 1);

            logUtility.Setup(utility => utility.GenerateLogFrequenciesRanges(configuration)).Returns(new int[33]);
            
            float[][] result = spectrumService.CreateLogSpectrogram(samples, configuration);

            logUtility.Verify(utility => utility.GenerateLogFrequenciesRanges(configuration), Times.Once());

            Assert.AreEqual(0, result.Length); // 128
        }

        [TestMethod]
        public void CutLogarithmizedSpectrumTest()
        {
            DefaultFingerprintConfiguration config = new DefaultFingerprintConfiguration();
            const int LogSpectrumLength = 1024;
            var stride = new StaticStride(0, 0);
            var logSpectrum = new float[LogSpectrumLength][];
            for (int i = 0; i < LogSpectrumLength; i++)
            {
                logSpectrum[i] = new float[32];
            }

            var cutLogarithmizedSpectrum = spectrumService.CutLogarithmizedSpectrum(logSpectrum, stride, config.FingerprintLength, config.Overlap);
            
            Assert.AreEqual(8, cutLogarithmizedSpectrum.Count);
        }

        [TestMethod]
        public void CutLogarithmizedSpectrumOfJustOneFingerprintTest()
        {
            DefaultFingerprintConfiguration config = new DefaultFingerprintConfiguration();
            int logSpectrumLength = config.FingerprintLength; // 128
            var stride = new StaticStride(0, 0);
            var logSpectrum = new float[logSpectrumLength][];
            for (int i = 0; i < logSpectrumLength; i++)
            {
                logSpectrum[i] = new float[32];
            }

            var cutLogarithmizedSpectrum = spectrumService.CutLogarithmizedSpectrum(logSpectrum, stride, config.FingerprintLength, config.Overlap);
            
            Assert.AreEqual(1, cutLogarithmizedSpectrum.Count);
        }

        [TestMethod]
        public void CutLogarithmizedSpectrumWithAnIncrementalStaticStride()
        {
            DefaultFingerprintConfiguration config = new DefaultFingerprintConfiguration();
            int logSpectrumLength = (config.FingerprintLength * 24) + config.Overlap;
            var stride = new IncrementalStaticStride(config.SamplesPerFingerprint / 2, config.SamplesPerFingerprint, 0);
            var logSpectrum = new float[logSpectrumLength][];
            for (int i = 0; i < logSpectrumLength; i++)
            {
                logSpectrum[i] = new float[32];
            }

            var cutLogarithmizedSpectrum = spectrumService.CutLogarithmizedSpectrum(logSpectrum, stride, config.FingerprintLength, config.Overlap);

            Assert.AreEqual(48, cutLogarithmizedSpectrum.Count);
        }

        [TestMethod]
        public void CutLogarithmizedSpectrumWithSpectrumWhichIsLessThanMinimalLengthOfOneFingerprintTest()
        {
            DefaultFingerprintConfiguration config = new DefaultFingerprintConfiguration();
            int logSpectrumLength = config.FingerprintLength - 1;
            var stride = new StaticStride(0, 0);
            var logSpectrum = new float[logSpectrumLength][];
            for (int i = 0; i < logSpectrumLength; i++)
            {
                logSpectrum[i] = new float[32];
            }

            var cutLogarithmizedSpectrum = spectrumService.CutLogarithmizedSpectrum(logSpectrum, stride, config.FingerprintLength, config.Overlap);

            Assert.AreEqual(0, cutLogarithmizedSpectrum.Count);
        }
    }
}
