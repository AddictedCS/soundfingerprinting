namespace SoundFingerprinting.Tests.Unit.FFT
{
    using Moq;

    using NUnit.Framework;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.FFT;
    using SoundFingerprinting.Strides;

    [TestFixture]
    public class SpectrumServiceTest : AbstractTest
    {
        private SpectrumService spectrumService;
        private Mock<IFFTServiceUnsafe> fftService;
        private Mock<ILogUtility> logUtility;

        [SetUp]
        public void SetUp()
        {
            fftService = new Mock<IFFTServiceUnsafe>(MockBehavior.Loose);
            logUtility = new Mock<ILogUtility>(MockBehavior.Strict);
            spectrumService = new SpectrumService(fftService.Object, logUtility.Object);
        }

        [TearDown]
        public void TearDown()
        {
            logUtility.VerifyAll();
        }
        
        [Test]
        public void CreateLogSpectrogramTest()
        {
            var configuration = new DefaultSpectrogramConfig { ImageLength = 2048 };
            var samples = TestUtilities.GenerateRandomAudioSamples((configuration.Overlap * configuration.WdftSize) + configuration.WdftSize); // 64 * 2048
            this.SetupFftService(configuration);

            var result = spectrumService.CreateLogSpectrogram(samples, configuration);

            logUtility.Verify(utility => utility.GenerateLogFrequenciesRanges(SampleRate, configuration), Times.Once());
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(configuration.ImageLength, result[0].Rows);
            Assert.AreEqual(configuration.LogBins, result[0].Cols);
        }

        [Test]
        public void CreateLogSpectrogramFromMinimalSamplesLengthTest()
        {
            var configuration = new DefaultSpectrogramConfig();
            var samples = TestUtilities.GenerateRandomAudioSamples(new DefaultFingerprintConfiguration().SamplesPerFingerprint + configuration.WdftSize); // 8192 + 2048
            this.SetupFftService(configuration);

            var result = spectrumService.CreateLogSpectrogram(samples, configuration);

            logUtility.Verify(utility => utility.GenerateLogFrequenciesRanges(SampleRate, configuration), Times.Once());
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(configuration.ImageLength, result[0].Rows);
        }

        [Test]
        public void ShouldCreateCorrectNumberOfSubFingerprints()
        {
            var configuration = new DefaultSpectrogramConfig { Stride = new StaticStride(0) };
            const int TenMinutes = 10 * 60;
            var samples = TestUtilities.GenerateRandomAudioSamples(TenMinutes * SampleRate);
            this.SetupFftService(configuration);

            var result = spectrumService.CreateLogSpectrogram(samples, configuration);

            Assert.AreEqual((TenMinutes * SampleRate) / (configuration.ImageLength * configuration.Overlap), result.Count);
        }

        [Test]
        public void CreateLogSpectrogramFromSamplesLessThanFourierTransformWindowLength()
        {
            var configuration = new DefaultSpectrogramConfig();
            var samples = TestUtilities.GenerateRandomAudioSamples(configuration.WdftSize - 1);

            var result = spectrumService.CreateLogSpectrogram(samples, configuration);

            Assert.AreEqual(0, result.Count); 
        }

        [Test]
        public void CutLogarithmizedSpectrumTest()
        {
            var configuration = new DefaultSpectrogramConfig { Stride = new StaticStride(0, 0) };
            const int LogSpectrumLength = 1024;
            var logSpectrum = GetLogSpectrum(LogSpectrumLength);

            var cutLogarithmizedSpectrum = spectrumService.CutLogarithmizedSpectrum(logSpectrum, SampleRate, configuration);
            
            Assert.AreEqual(8, cutLogarithmizedSpectrum.Count);
            double lengthOfOneFingerprint = (double)configuration.ImageLength * configuration.Overlap / SampleRate;
            for (int i = 0; i < cutLogarithmizedSpectrum.Count; i++)
            {
                Assert.IsTrue(System.Math.Abs(cutLogarithmizedSpectrum[i].StartsAt - (i * lengthOfOneFingerprint)) < Epsilon);
            }
        }
        
        [Test]
        public void CutLogarithmizedSpectrumOfJustOneFingerprintTest()
        {
            var stride = new StaticStride(0, 0);
            var configuration = new DefaultSpectrogramConfig { Stride = stride };
            int logSpectrumLength = configuration.ImageLength; // 128
            var logSpectrum = GetLogSpectrum(logSpectrumLength);

            var cutLogarithmizedSpectrum = spectrumService.CutLogarithmizedSpectrum(logSpectrum, SampleRate, configuration);
            
            Assert.AreEqual(1, cutLogarithmizedSpectrum.Count);
        }

        [Test]
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

        [Test]
        public void CutLogarithmizedSpectrumWithDefaultStride()
        {
            var config = new DefaultSpectrogramConfig();
            int logSpectrumlength = config.ImageLength * 10;
            var logSpectrum = GetLogSpectrum(logSpectrumlength);

            var cutLogarithmizedSpectrum = spectrumService.CutLogarithmizedSpectrum(logSpectrum, SampleRate, config);
            
            // Default stride between 2 consecutive images is 512, but because of rounding issues and the fact
            // that minimal step is 11.6 ms, timestamp is roughly .37155 sec
            const double TimestampOfFingerprints = (double)512 / SampleRate;
            Assert.AreEqual(145, cutLogarithmizedSpectrum.Count);
            for (int i = 0; i < cutLogarithmizedSpectrum.Count; i++)
            {
                Assert.IsTrue(System.Math.Abs(cutLogarithmizedSpectrum[i].StartsAt - (i * TimestampOfFingerprints)) < Epsilon);
            }
        }

        [Test]
        public void CutLogarithmizedSpectrumWithSpectrumWhichIsLessThanMinimalLengthOfOneFingerprintTest()
        {
            var stride = new StaticStride(0, 0);
            var config = new DefaultSpectrogramConfig { Stride = stride };
            int logSpectrumLength = config.ImageLength - 1;
            var logSpectrum = GetLogSpectrum(logSpectrumLength);

            var cutLogarithmizedSpectrum = spectrumService.CutLogarithmizedSpectrum(logSpectrum, SampleRate, config);

            Assert.AreEqual(0, cutLogarithmizedSpectrum.Count);
        }

        private void SetupFftService(DefaultSpectrogramConfig configuration)
        {
            logUtility.Setup(utility => utility.GenerateLogFrequenciesRanges(SampleRate, configuration))
                .Returns(new ushort[]
                        {
                            118, 125, 133, 141, 149, 158, 167, 177, 187, 198, 210, 223, 236, 250, 264, 280, 297, 314,
                            333, 352, 373, 395, 419, 443, 470, 497, 527, 558, 591, 626, 663, 702, 744,
                        });
        }

        private float[] GetLogSpectrum(int logSpectrumLength)
        {
            return new float[logSpectrumLength * 32];
        }
    }
}
