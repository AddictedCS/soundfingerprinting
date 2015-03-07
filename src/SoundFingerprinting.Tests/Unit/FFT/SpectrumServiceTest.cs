namespace SoundFingerprinting.Tests.Unit.FFT
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.FFT;
    using SoundFingerprinting.Strides;

    [TestClass]
    public class SpectrumServiceTest : AbstractTest
    {
        private DerivedSpectrumService spectrumService;

        private Mock<IFFTService> fftService;

        private Mock<IAudioSamplesNormalizer> audioSamplesNormalizer;

        private Mock<ILogUtility> logUtility;

        [TestInitialize]
        public void SetUp()
        {
            fftService = new Mock<IFFTService>(MockBehavior.Strict);
            audioSamplesNormalizer = new Mock<IAudioSamplesNormalizer>(MockBehavior.Strict);
            logUtility = new Mock<ILogUtility>(MockBehavior.Strict);
            spectrumService = new DerivedSpectrumService(fftService.Object, logUtility.Object, audioSamplesNormalizer.Object);
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
            var configuration = new CustomSpectrogramConfig { NormalizeSignal = true, ImageLength = 2048 };
            var samples = TestUtilities.GenerateRandomAudioSamples((configuration.Overlap * configuration.WdftSize) + configuration.WdftSize); // 64 * 2048
            
            audioSamplesNormalizer.Setup(service => service.NormalizeInPlace(samples.Samples));
            logUtility.Setup(utility => utility.GenerateLogFrequenciesRanges(5512, configuration)).Returns(new int[33]);
            fftService.Setup(service => service.FFTForward(samples.Samples, It.IsAny<int>(), configuration.WdftSize))
                      .Returns(TestUtilities.GenerateRandomFloatArray(2048));

            var result = spectrumService.CreateLogSpectrogram(samples, configuration);

            audioSamplesNormalizer.Verify(service => service.NormalizeInPlace(samples.Samples), Times.Once());
            logUtility.Verify(utility => utility.GenerateLogFrequenciesRanges(5512, configuration), Times.Once());

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(configuration.WdftSize, result[0].Image.Length);
            Assert.AreEqual(32, result[0].Image[0].Length);
        }

        [TestMethod]
        public void CreateLogSpectrogramFromMinimalSamplesLengthTest()
        {
            var configuration = new CustomSpectrogramConfig { NormalizeSignal = false };
            var samples = TestUtilities.GenerateRandomAudioSamples(FingerprintConfiguration.Default.SamplesPerFingerprint + configuration.WdftSize); // 8192 + 2048

            logUtility.Setup(utility => utility.GenerateLogFrequenciesRanges(5512, configuration)).Returns(new int[33]);
            fftService.Setup(service => service.FFTForward(samples.Samples, It.IsAny<int>(), configuration.WdftSize))
                      .Returns(TestUtilities.GenerateRandomFloatArray(2048));

            var result = spectrumService.CreateLogSpectrogram(samples, configuration);

            audioSamplesNormalizer.Verify(service => service.NormalizeInPlace(samples.Samples), Times.Never());
            logUtility.Verify(utility => utility.GenerateLogFrequenciesRanges(5512, configuration), Times.Once());

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(configuration.ImageLength, result[0].Image.Length); // 128
        }

        [TestMethod]
        public void CreateLogSpectrogramFromSamplesLessThanFourierTransformWindowLength()
        {
            var configuration = SpectrogramConfig.Default;
            var samples = TestUtilities.GenerateRandomAudioSamples(configuration.WdftSize - 1);

            var result = spectrumService.CreateLogSpectrogram(samples, configuration);

            Assert.AreEqual(0, result.Count); 
        }

        [TestMethod]
        public void CutLogarithmizedSpectrumTest()
        {
            var stride = new StaticStride(0, 0);
            var config = new CustomSpectrogramConfig { Stride = stride };
            const int LogSpectrumLength = 1024;
            var logSpectrum = new float[LogSpectrumLength][];
            for (int i = 0; i < LogSpectrumLength; i++)
            {
                logSpectrum[i] = new float[32];
            }

            var cutLogarithmizedSpectrum = spectrumService.CutLogarithmizedSpectrum(logSpectrum, 5512, config);
            
            Assert.AreEqual(8, cutLogarithmizedSpectrum.Count);
        }

        [TestMethod]
        public void CutLogarithmizedSpectrumOfJustOneFingerprintTest()
        {
            var stride = new StaticStride(0, 0);
            var config = new CustomSpectrogramConfig { Stride = stride };
            int logSpectrumLength = config.ImageLength; // 128
            var logSpectrum = new float[logSpectrumLength][];
            for (int i = 0; i < logSpectrumLength; i++)
            {
                logSpectrum[i] = new float[32];
            }

            var cutLogarithmizedSpectrum = spectrumService.CutLogarithmizedSpectrum(logSpectrum, 5512, config);
            
            Assert.AreEqual(1, cutLogarithmizedSpectrum.Count);
        }

        [TestMethod]
        public void CutLogarithmizedSpectrumWithAnIncrementalStaticStride()
        {
            var stride = new IncrementalStaticStride(FingerprintConfiguration.Default.SamplesPerFingerprint / 2, FingerprintConfiguration.Default.SamplesPerFingerprint, 0);
            var config = new CustomSpectrogramConfig { Stride = stride };
            int logSpectrumLength = (config.ImageLength * 24) + config.Overlap;
            var logSpectrum = new float[logSpectrumLength][];
            for (int i = 0; i < logSpectrumLength; i++)
            {
                logSpectrum[i] = new float[32];
            }

            var cutLogarithmizedSpectrum = spectrumService.CutLogarithmizedSpectrum(logSpectrum, 5512, config);

            Assert.AreEqual(48, cutLogarithmizedSpectrum.Count);
        }

        [TestMethod]
        public void CutLogarithmizedSpectrumWithSpectrumWhichIsLessThanMinimalLengthOfOneFingerprintTest()
        {
            var stride = new StaticStride(0, 0);
            var config = new CustomSpectrogramConfig { Stride = stride };
            int logSpectrumLength = config.ImageLength - 1;
            var logSpectrum = new float[logSpectrumLength][];
            for (int i = 0; i < logSpectrumLength; i++)
            {
                logSpectrum[i] = new float[32];
            }

            var cutLogarithmizedSpectrum = spectrumService.CutLogarithmizedSpectrum(logSpectrum, 5512, config);

            Assert.AreEqual(0, cutLogarithmizedSpectrum.Count);
        }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed. Suppression is OK here.")]
    internal class DerivedSpectrumService : SpectrumService
    {
        public DerivedSpectrumService(IFFTService fftService, ILogUtility logUtility, IAudioSamplesNormalizer audioSamplesNormalizer)
            : base(fftService, logUtility, audioSamplesNormalizer)
        {
        }

        public new List<SpectralImage> CutLogarithmizedSpectrum(
            float[][] logarithmizedSpectrum, int sampleRate, SpectrogramConfig configuration)
        {
            return base.CutLogarithmizedSpectrum(logarithmizedSpectrum, sampleRate, configuration);
        }
    }
}
