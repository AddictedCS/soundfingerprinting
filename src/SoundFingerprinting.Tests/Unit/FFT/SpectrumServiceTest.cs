namespace SoundFingerprinting.Tests.Unit.FFT
{
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.FFT;
    using SoundFingerprinting.Strides;

    [TestClass]
    public class SpectrumServiceTest : AbstractTest
    {
        private SpectrumService spectrumService;
        private Mock<IFFTService> fftService;
        private Mock<ILogUtility> logUtility;

        [TestInitialize]
        public void SetUp()
        {
            fftService = new Mock<IFFTService>(MockBehavior.Strict);
            logUtility = new Mock<ILogUtility>(MockBehavior.Strict);
            spectrumService = new SpectrumService(fftService.Object, logUtility.Object);
        }

        [TestCleanup]
        public void TearDown()
        {
            fftService.VerifyAll();
            logUtility.VerifyAll();
        }
        
        [TestMethod]
        public void CreateLogSpectrogramTest()
        {
            var configuration = new DefaultSpectrogramConfig { ImageLength = 2048 };
            var samples = TestUtilities.GenerateRandomAudioSamples((configuration.Overlap * configuration.WdftSize) + configuration.WdftSize); // 64 * 2048
            logUtility.Setup(utility => utility.GenerateLogFrequenciesRanges(SampleRate, configuration)).Returns(new int[33]);
            fftService.Setup(service => service.FFTForward(samples.Samples, It.IsAny<int>(), configuration.WdftSize, It.IsAny<float[]>()))
                      .Returns(TestUtilities.GenerateRandomFloatArray(2048));

            var result = spectrumService.CreateLogSpectrogram(samples, configuration);

            logUtility.Verify(utility => utility.GenerateLogFrequenciesRanges(SampleRate, configuration), Times.Once());
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(configuration.WdftSize, result[0].Image.Length);
            Assert.AreEqual(32, result[0].Image[0].Length);
        }

        [TestMethod]
        public void CreateLogSpectrogramFromMinimalSamplesLengthTest()
        {
            var configuration = new DefaultSpectrogramConfig { NormalizeSignal = false };
            var samples = TestUtilities.GenerateRandomAudioSamples(new DefaultFingerprintConfiguration().SamplesPerFingerprint + configuration.WdftSize); // 8192 + 2048
            logUtility.Setup(utility => utility.GenerateLogFrequenciesRanges(SampleRate, configuration)).Returns(new int[33]);
            fftService.Setup(service => service.FFTForward(samples.Samples, It.IsAny<int>(), configuration.WdftSize, It.IsAny<float[]>()))
                      .Returns(TestUtilities.GenerateRandomFloatArray(2048));

            var result = spectrumService.CreateLogSpectrogram(samples, configuration);

            logUtility.Verify(utility => utility.GenerateLogFrequenciesRanges(SampleRate, configuration), Times.Once());
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(configuration.ImageLength, result[0].Image.Length);
        }

        [TestMethod]
        public void CreateLogSpectrogramFromSamplesLessThanFourierTransformWindowLength()
        {
            var configuration = new DefaultSpectrogramConfig();
            var samples = TestUtilities.GenerateRandomAudioSamples(configuration.WdftSize - 1);

            var result = spectrumService.CreateLogSpectrogram(samples, configuration);

            Assert.AreEqual(0, result.Count); 
        }

        [TestMethod]
        public void CutLogarithmizedSpectrumTest()
        {
            var stride = new StaticStride(0, 0);
            var configuration = new DefaultSpectrogramConfig { Stride = stride };
            const int LogSpectrumLength = 1024;
            var logSpectrum = GetLogSpectrum(LogSpectrumLength);

            var cutLogarithmizedSpectrum = spectrumService.CutLogarithmizedSpectrum(logSpectrum, SampleRate, configuration);
            
            Assert.AreEqual(8, cutLogarithmizedSpectrum.Count);
            double lengthOfOneFingerprint = (double)configuration.ImageLength * configuration.Overlap / SampleRate;
            for (int i = 0; i < cutLogarithmizedSpectrum.Count; i++)
            {
                Assert.IsTrue(
                    System.Math.Abs(cutLogarithmizedSpectrum[i].StartsAt - (i * lengthOfOneFingerprint)) < Epsilon);
            }
        }
        
        [TestMethod]
        public void CutLogarithmizedSpectrumOfJustOneFingerprintTest()
        {
            var stride = new StaticStride(0, 0);
            var configuration = new DefaultSpectrogramConfig { Stride = stride };
            int logSpectrumLength = configuration.ImageLength; // 128
            var logSpectrum = GetLogSpectrum(logSpectrumLength);

            var cutLogarithmizedSpectrum = spectrumService.CutLogarithmizedSpectrum(logSpectrum, SampleRate, configuration);
            
            Assert.AreEqual(1, cutLogarithmizedSpectrum.Count);
        }

        [TestMethod]
        public void CutLogarithmizedSpectrumWithAnIncrementalStaticStride()
        {
            var stride = new IncrementalStaticStride(new DefaultFingerprintConfiguration().SamplesPerFingerprint / 2);
            var config = new DefaultSpectrogramConfig { Stride = stride };
            int logSpectrumLength = (config.ImageLength * 24) + config.Overlap;
            var logSpectrum = GetLogSpectrum(logSpectrumLength);

            var cutLogarithmizedSpectrum = spectrumService.CutLogarithmizedSpectrum(logSpectrum, SampleRate, config);
            
            Assert.AreEqual(48, cutLogarithmizedSpectrum.Count);
            double lengthOfOneFingerprint = (double)config.ImageLength * config.Overlap / SampleRate;
            for (int i = 0; i < cutLogarithmizedSpectrum.Count; i++)
            {
                Assert.IsTrue(System.Math.Abs(cutLogarithmizedSpectrum[i].StartsAt - (i * lengthOfOneFingerprint / 2)) < Epsilon);
            }
        }

        [TestMethod]
        public void CutLogarithmizedSpectrumWithDefaultStride()
        {
            var config = new DefaultSpectrogramConfig();
            int logSpectrumlength = config.ImageLength * 10;
            var logSpectrum = GetLogSpectrum(logSpectrumlength);

            var cutLogarithmizedSpectrum = spectrumService.CutLogarithmizedSpectrum(logSpectrum, SampleRate, config);
            
            // Default stride between 2 consecutive images is 1536, but because of rounding issues and the fact
            // that minimal step is 11.6 ms, timestamp is roughly .37155 sec
            const double TimestampOfFingerprints = (double)1536 / SampleRate;
            Assert.AreEqual(49, cutLogarithmizedSpectrum.Count);
            for (int i = 0; i < cutLogarithmizedSpectrum.Count; i++)
            {
                Assert.IsTrue(System.Math.Abs(cutLogarithmizedSpectrum[i].StartsAt - (i * TimestampOfFingerprints)) < Epsilon);
            }
        }

        [TestMethod]
        public void CutLogarithmizedSpectrumWithSpectrumWhichIsLessThanMinimalLengthOfOneFingerprintTest()
        {
            var stride = new StaticStride(0, 0);
            var config = new DefaultSpectrogramConfig { Stride = stride };
            int logSpectrumLength = config.ImageLength - 1;
            var logSpectrum = GetLogSpectrum(logSpectrumLength);

            var cutLogarithmizedSpectrum = spectrumService.CutLogarithmizedSpectrum(logSpectrum, SampleRate, config);

            Assert.AreEqual(0, cutLogarithmizedSpectrum.Count);
        }

        [TestMethod]
        public void ShouldExtractLogarithmicBandsCorrectly()
        {
            var utility = new LogUtility();
            int[] indexes = utility.GenerateLogFrequenciesRanges(5512, new DefaultSpectrogramConfig { UseDynamicLogBase = true });
            float[] spectrum = Enumerable.Range(0, 2048).Select(item => (float)item).ToArray();

            float[] bands = spectrumService.ExtractLogBins(spectrum, indexes, 32, 2048);

            Assert.AreEqual(32, bands.Length);
        }
       
        private float[][] GetLogSpectrum(int logSpectrumLength)
        {
            var logSpectrum = new float[logSpectrumLength][];
            for (int i = 0; i < logSpectrumLength; i++)
            {
                logSpectrum[i] = new float[32];
            }

            return logSpectrum;
        }
    }
}
