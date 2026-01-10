namespace SoundFingerprinting.Tests.Unit.FFT
{
    using Moq;

    using NUnit.Framework;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.FFT;
    using SoundFingerprinting.Strides;

    [TestFixture]
    public class SpectrumServiceTest
    {
        private const int SampleRate = 5512;
        private const double Epsilon = 0.0001;
        
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
            SetupFftService(configuration);

            var result = spectrumService.CreateLogSpectrogram(samples, configuration);

            logUtility.Verify(utility => utility.GenerateLogFrequenciesRanges(SampleRate, configuration), Times.Once());
			Assert.That(result, Has.Count.EqualTo(1));
			Assert.Multiple(() =>
			{
				Assert.That(result[0].Rows, Is.EqualTo(configuration.ImageLength));
				Assert.That(result[0].Cols, Is.EqualTo(configuration.LogBins));
			});
		}

        [Test]
        public void CreateLogSpectrogramFromMinimalSamplesLengthTest()
        {
            var configuration = new DefaultSpectrogramConfig();
            var samples = TestUtilities.GenerateRandomAudioSamples(new DefaultFingerprintConfiguration().SamplesPerFingerprint + configuration.WdftSize); // 8192 + 2048
            SetupFftService(configuration);

            var result = spectrumService.CreateLogSpectrogram(samples, configuration);

            logUtility.Verify(utility => utility.GenerateLogFrequenciesRanges(SampleRate, configuration), Times.Once());
			Assert.That(result, Has.Count.EqualTo(1));
			Assert.That(result[0].Rows, Is.EqualTo(configuration.ImageLength));
        }

        [Test]
        public void CreateLogSpectrumFromTwoEntries()
        {
            int stride = 256;
            var configuration = new DefaultSpectrogramConfig
            {
                Stride = new IncrementalStaticStride(stride)
            };
            
            var samples = TestUtilities.GenerateRandomAudioSamples(new DefaultFingerprintConfiguration().SamplesPerFingerprint + configuration.WdftSize + stride);
            SetupFftService(configuration);

            var result = spectrumService.CreateLogSpectrogram(samples, configuration);

            logUtility.Verify(utility => utility.GenerateLogFrequenciesRanges(SampleRate, configuration), Times.Once());
			Assert.That(result, Has.Count.EqualTo(2));
        }

        [Test]
        public void ShouldCreateCorrectNumberOfSubFingerprints()
        {
            var configuration = new DefaultSpectrogramConfig { Stride = new IncrementalStaticStride(8192) };
            const int tenMinutes = 10 * 60;
            var samples = TestUtilities.GenerateRandomAudioSamples(tenMinutes * SampleRate);
            SetupFftService(configuration);

            var result = spectrumService.CreateLogSpectrogram(samples, configuration);

			Assert.That(result, Has.Count.EqualTo((tenMinutes * SampleRate) / (configuration.ImageLength * configuration.Overlap)));
        }

        [Test]
        public void CreateLogSpectrogramFromSamplesLessThanFourierTransformWindowLength()
        {
            var configuration = new DefaultSpectrogramConfig();
            var samples = TestUtilities.GenerateRandomAudioSamples(configuration.WdftSize - 1);

            var result = spectrumService.CreateLogSpectrogram(samples, configuration);

			Assert.That(result.Count, Is.EqualTo(0)); 
        }

        [Test]
        public void CutLogarithmizedSpectrumTest()
        {
            var configuration = new DefaultSpectrogramConfig { Stride = new IncrementalStaticStride(8192) };
            const int logSpectrumLength = 1024;
            var logSpectrum = GetLogSpectrum(logSpectrumLength);

            var cutLogarithmizedSpectrum = spectrumService.CutLogarithmizedSpectrum(logSpectrum, SampleRate, configuration);

			Assert.That(cutLogarithmizedSpectrum, Has.Count.EqualTo(8));
            double lengthOfOneFingerprint = (double)configuration.ImageLength * configuration.Overlap / SampleRate;
            for (int i = 0; i < cutLogarithmizedSpectrum.Count; i++)
            {
				Assert.That(System.Math.Abs(cutLogarithmizedSpectrum[i].StartsAt - (i * lengthOfOneFingerprint)) < Epsilon, Is.True);
            }
        }
        
        [Test]
        public void CutLogarithmizedSpectrumOfJustOneFingerprintTest()
        {
            var stride = new IncrementalStaticStride(8192);
            var configuration = new DefaultSpectrogramConfig { Stride = stride };
            int logSpectrumLength = configuration.ImageLength; // 128
            var logSpectrum = GetLogSpectrum(logSpectrumLength);

            var cutLogarithmizedSpectrum = spectrumService.CutLogarithmizedSpectrum(logSpectrum, SampleRate, configuration);

			Assert.That(cutLogarithmizedSpectrum, Has.Count.EqualTo(1));
        }

        [Test]
        public void CutLogarithmizedSpectrumWithAnIncrementalStaticStride()
        {
            var stride = new IncrementalStaticStride(new DefaultFingerprintConfiguration().SamplesPerFingerprint / 2);
            var config = new DefaultSpectrogramConfig { Stride = stride };
            int logSpectrumLength = (config.ImageLength * 24) + config.Overlap;
            var logSpectrum = GetLogSpectrum(logSpectrumLength);

            var cutLogarithmizedSpectrum = spectrumService.CutLogarithmizedSpectrum(logSpectrum, SampleRate, config);

			Assert.That(cutLogarithmizedSpectrum, Has.Count.EqualTo(48));
            double lengthOfOneFingerprint = (double)config.ImageLength * config.Overlap / SampleRate;
            for (int i = 0; i < cutLogarithmizedSpectrum.Count; i++)
            {
				Assert.That(System.Math.Abs(cutLogarithmizedSpectrum[i].StartsAt - (i * lengthOfOneFingerprint / 2)) < Epsilon, Is.True);
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
            const double timestampOfFingerprints = (double)512 / SampleRate;
			Assert.That(cutLogarithmizedSpectrum, Has.Count.EqualTo(145));
            for (int i = 0; i < cutLogarithmizedSpectrum.Count; i++)
            {
				Assert.That(System.Math.Abs(cutLogarithmizedSpectrum[i].StartsAt - (i * timestampOfFingerprints)) < Epsilon, Is.True);
            }
        }

        [Test]
        public void CutLogarithmizedSpectrumWithSpectrumWhichIsLessThanMinimalLengthOfOneFingerprintTest()
        {
            var stride = new IncrementalStaticStride(8192);
            var config = new DefaultSpectrogramConfig { Stride = stride };
            int logSpectrumLength = config.ImageLength - 1;
            var logSpectrum = GetLogSpectrum(logSpectrumLength);

            var cutLogarithmizedSpectrum = spectrumService.CutLogarithmizedSpectrum(logSpectrum, SampleRate, config);

			Assert.That(cutLogarithmizedSpectrum.Count, Is.EqualTo(0));
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
