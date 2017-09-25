namespace SoundFingerprinting.Tests.Unit
{
    using System.Collections.Generic;
    using System.Linq;

    using Moq;

    using NUnit.Framework;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.FFT;
    using SoundFingerprinting.Utils;
    using SoundFingerprinting.Wavelets;

    [TestFixture]
    public class FingerprintServiceTest : AbstractTest
    {
        private FingerprintService fingerprintService;

        private Mock<IFingerprintDescriptor> fingerprintDescriptor;

        private Mock<ISpectrumService> spectrumService;

        private Mock<IWaveletDecomposition> waveletDecomposition;

        private Mock<IAudioSamplesNormalizer> audioSamplesNormalizer;

        [SetUp]
        public void SetUp()
        {
            fingerprintDescriptor = new Mock<IFingerprintDescriptor>(MockBehavior.Strict);
            spectrumService = new Mock<ISpectrumService>(MockBehavior.Strict);
            waveletDecomposition = new Mock<IWaveletDecomposition>(MockBehavior.Strict);
            audioSamplesNormalizer = new Mock<IAudioSamplesNormalizer>(MockBehavior.Strict);
            fingerprintService = new FingerprintService(
                spectrumService.Object,
                waveletDecomposition.Object,
                fingerprintDescriptor.Object,
                audioSamplesNormalizer.Object);
        }

        [TearDown]
        public void TearDown()
        {
            fingerprintDescriptor.VerifyAll();
            spectrumService.VerifyAll();
            waveletDecomposition.VerifyAll();
            audioSamplesNormalizer.VerifyAll();
        }

        [Test]
        public void CreateFingerprints()
        {
            const int TenSeconds = 5512 * 10;
            var samples = TestUtilities.GenerateRandomAudioSamples(TenSeconds);
            var fingerprintConfig = new DefaultFingerprintConfiguration();
            var dividedLogSpectrum = GetDividedLogSpectrum();
            spectrumService.Setup(service => service.CreateLogSpectrogram(samples, It.IsAny<DefaultSpectrogramConfig>())).Returns(dividedLogSpectrum);
            waveletDecomposition.Setup(service => service.DecomposeImageInPlace(It.IsAny<float[]>(), 128, 32));
            fingerprintDescriptor.Setup(descriptor => descriptor.ExtractTopWavelets(It.IsAny<float[]>(), fingerprintConfig.TopWavelets)).Returns(new EncodedFingerprintSchema(8192).SetTrueAt(0 , 1));

            var fingerprints = fingerprintService.CreateFingerprints(samples, fingerprintConfig)
                                                 .OrderBy(f => f.SequenceNumber)
                                                 .ToList();

            Assert.AreEqual(dividedLogSpectrum.Count, fingerprints.Count);
            for (int index = 0; index < fingerprints.Count; index++)
            {
                Assert.AreEqual(dividedLogSpectrum[index].StartsAt, fingerprints[index].StartsAt, Epsilon);
            }
        }

        [Test]
        public void AudioSamplesAreNormalized()
        {
            const int TenSeconds = 5512 * 10;
            var samples = TestUtilities.GenerateRandomAudioSamples(TenSeconds);
            var fingerprintConfig = new DefaultFingerprintConfiguration { NormalizeSignal = true };
            var dividedLogSpectrum = GetDividedLogSpectrum();
            spectrumService.Setup(service => service.CreateLogSpectrogram(samples, It.IsAny<DefaultSpectrogramConfig>())).Returns(dividedLogSpectrum);
            waveletDecomposition.Setup(service => service.DecomposeImageInPlace(It.IsAny<float[]>(), 128, 32));
            fingerprintDescriptor.Setup(descriptor => descriptor.ExtractTopWavelets(It.IsAny<float[]>(), fingerprintConfig.TopWavelets)).Returns(new EncodedFingerprintSchema(8192));

            audioSamplesNormalizer.Setup(normalizer => normalizer.NormalizeInPlace(samples.Samples));

            fingerprintService.CreateFingerprints(samples, fingerprintConfig);
        }

        [Test]
        public void SilenceIsNotFingerprinted()
        {
            var samples = TestUtilities.GenerateRandomAudioSamples(5512 * 10);
            var configuration = new DefaultFingerprintConfiguration();
            var dividedLogSpectrum = GetDividedLogSpectrum();

            spectrumService.Setup(service => service.CreateLogSpectrogram(samples, It.IsAny<DefaultSpectrogramConfig>())).Returns(dividedLogSpectrum);

            waveletDecomposition.Setup(decomposition => decomposition.DecomposeImageInPlace(It.IsAny<float[]>(), 128, 32));
            fingerprintDescriptor.Setup(
                descriptor => descriptor.ExtractTopWavelets(It.IsAny<float[]>(), configuration.TopWavelets)).Returns(
                    new EncodedFingerprintSchema(1024));

            var rawFingerprints = fingerprintService.CreateFingerprints(samples, configuration);

            Assert.IsTrue(rawFingerprints.Count == 0);
        }

        private List<SpectralImage> GetDividedLogSpectrum()
        {
            var dividedLogSpectrum = new List<SpectralImage>();
            for (int index = 0; index < 4; index++)
            {
                dividedLogSpectrum.Add(new SpectralImage(TestUtilities.GenerateRandomFloatArray(4096), 128, 32, 0.928 * index, index));
            }

            return dividedLogSpectrum;
        }
    }
}