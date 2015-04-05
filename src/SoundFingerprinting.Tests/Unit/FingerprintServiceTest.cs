namespace SoundFingerprinting.Tests.Unit
{
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.FFT;
    using SoundFingerprinting.Utils;
    using SoundFingerprinting.Wavelets;

    [TestClass]
    public class FingerprintServiceTest : AbstractTest
    {
        private FingerprintService fingerprintService;

        private Mock<IFingerprintDescriptor> fingerprintDescriptor;

        private Mock<ISpectrumService> spectrumService;

        private Mock<IWaveletDecomposition> waveletDecomposition;

        private Mock<IAudioSamplesNormalizer> audioSamplesNormalizer;

        [TestInitialize]
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

        [TestCleanup]
        public void TearDown()
        {
            fingerprintDescriptor.VerifyAll();
            spectrumService.VerifyAll();
            waveletDecomposition.VerifyAll();
            audioSamplesNormalizer.VerifyAll();
        }

        [TestMethod]
        public void CreateFingerprints()
        {
            const int TenSeconds = 5512 * 10;
            var samples = TestUtilities.GenerateRandomAudioSamples(TenSeconds);
            var configuration = SpectrogramConfig.Default;
            var fingerprintConfig = FingerprintConfiguration.Default;
            var dividedLogSpectrum = GetDividedLogSpectrum();
            spectrumService.Setup(service => service.CreateLogSpectrogram(samples, configuration)).Returns(dividedLogSpectrum);
            waveletDecomposition.Setup(service => service.DecomposeImageInPlace(It.IsAny<float[][]>()));
            fingerprintDescriptor.Setup(descriptor => descriptor.ExtractTopWavelets(It.IsAny<float[][]>(), fingerprintConfig.TopWavelets))
                .Returns(GenericFingerprint);

            var fingerprints = fingerprintService.CreateFingerprints(samples, fingerprintConfig);

            Assert.AreEqual(dividedLogSpectrum.Count, fingerprints.Count);
            for (int index = 0; index < fingerprints.Count; index++)
            {
                Assert.AreEqual(GenericFingerprint, fingerprints[index].Signature);
                Assert.AreEqual(dividedLogSpectrum[index].Timestamp, fingerprints[index].Timestamp, Epsilon);
            }
        }

        [TestMethod]
        public void AudioSamplesAreNormalized()
        {
            const int TenSeconds = 5512 * 10;
            var samples = TestUtilities.GenerateRandomAudioSamples(TenSeconds);
            var fingerprintConfig = new CustomFingerprintConfiguration { NormalizeSignal = true };
            var dividedLogSpectrum = GetDividedLogSpectrum();
            spectrumService.Setup(service => service.CreateLogSpectrogram(samples, It.IsAny<CustomSpectrogramConfig>())).Returns(dividedLogSpectrum);
            waveletDecomposition.Setup(service => service.DecomposeImageInPlace(It.IsAny<float[][]>()));
            fingerprintDescriptor.Setup(descriptor => descriptor.ExtractTopWavelets(It.IsAny<float[][]>(), fingerprintConfig.TopWavelets))
                .Returns(GenericFingerprint);
            audioSamplesNormalizer.Setup(normalizer => normalizer.NormalizeInPlace(samples.Samples));

            fingerprintService.CreateFingerprints(samples, fingerprintConfig);
        }

        [TestMethod]
        public void SilenceIsNotFingerprinted()
        {
            var samples = TestUtilities.GenerateRandomAudioSamples(5512 * 10);
            var configuration = FingerprintConfiguration.Default;
            var spectrogramConfig = SpectrogramConfig.Default;
            var dividedLogSpectrum = GetDividedLogSpectrum();

            spectrumService.Setup(service => service.CreateLogSpectrogram(samples, spectrogramConfig)).Returns(dividedLogSpectrum);

            waveletDecomposition.Setup(decomposition => decomposition.DecomposeImageInPlace(It.IsAny<float[][]>()));
            fingerprintDescriptor.Setup(descriptor => descriptor.ExtractTopWavelets(It.IsAny<float[][]>(), configuration.TopWavelets)).Returns(
                    new[] { false, false, false, false, false, false, false, false });

            var rawFingerprints = fingerprintService.CreateFingerprints(samples, configuration);

            Assert.IsTrue(rawFingerprints.Count == 0);
        }

        private List<SpectralImage> GetDividedLogSpectrum()
        {
            var dividedLogSpectrum = new List<SpectralImage>();
            for (int index = 0; index < 4; index++)
            {
                dividedLogSpectrum.Add(
                    new SpectralImage
                        {
                            Image = new[] { TestUtilities.GenerateRandomFloatArray(2048) }, Timestamp = 0.928 * index
                        });
            }

            return dividedLogSpectrum;
        }
    }
}