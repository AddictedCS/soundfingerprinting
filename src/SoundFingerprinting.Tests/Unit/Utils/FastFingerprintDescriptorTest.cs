namespace SoundFingerprinting.Tests.Unit.Utils
{
    using System;
    using System.Threading.Tasks;

    using NUnit.Framework;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.FFT;
    using SoundFingerprinting.LSH;
    using SoundFingerprinting.Tests.Integration;
    using SoundFingerprinting.Utils;
    using SoundFingerprinting.Wavelets;

    [TestFixture]
    public class FastFingerprintDescriptorTest : IntegrationWithSampleFilesTest
    {
        [Test]
        public async Task ShouldCreateExactlyTheSameFingerprints()
        {
            var fcbWithOldFingerprintDescriptor = new FingerprintCommandBuilder(
                new FingerprintService(
                    new SpectrumService(new LomontFFT(), new LogUtility()),
                    LocalitySensitiveHashingAlgorithm.Instance,
                    new StandardHaarWaveletDecomposition(),
                    new FingerprintDescriptor()));

            var fcbWithFastFingerprintDescriptor = new FingerprintCommandBuilder(
                new FingerprintService(
                    new SpectrumService(new LomontFFT(), new LogUtility()),
                    LocalitySensitiveHashingAlgorithm.Instance,
                    new StandardHaarWaveletDecomposition(),
                    new FastFingerprintDescriptor()));

            var audioSamples = GetAudioSamples();

            int runs = 5;
            for (int i = 0; i < runs; ++i)
            {
                var (h1, _) = await fcbWithOldFingerprintDescriptor.BuildFingerprintCommand()
                    .From(audioSamples)
                    .Hash();

                var (h2, _) = await fcbWithFastFingerprintDescriptor.BuildFingerprintCommand()
                    .From(audioSamples)
                    .Hash();

                AssertHashDatasAreTheSame(h1, h2);
            }
        }

        [Test]
        public void ShouldRunCorrectlyForSpecificUseCase()
        {
            int sequenceNumber = 334;
            float[] samples = GetAudioSamples().Samples;
            int start = sequenceNumber * 1536;

            float[] troubledPart = new float[8192 + 2048];
            Array.Copy(samples, start, troubledPart, 0, 8192 + 2048);
            var audioSamples = new AudioSamples(troubledPart, "test", 5512);

            var fingerprintService = new FingerprintService(
                new SpectrumService(new LomontFFT(), new LogUtility()),
                LocalitySensitiveHashingAlgorithm.Instance,
                new StandardHaarWaveletDecomposition(),
                new FingerprintDescriptor());

            var fastFingerprintService = new FingerprintService(
                new SpectrumService(new LomontFFT(), new LogUtility()),
                LocalitySensitiveHashingAlgorithm.Instance,
                new StandardHaarWaveletDecomposition(),
                new FastFingerprintDescriptor());

            int runs = 10;
            var configuration = new DefaultFingerprintConfiguration();
            for (int i = 0; i < runs; ++i)
            {
                var (_, x) = fingerprintService.CreateFingerprintsFromAudioSamples(audioSamples, configuration);
                var (_, y) = fastFingerprintService.CreateFingerprintsFromAudioSamples(audioSamples, configuration);

                for (int j = 0; j < x.Count; ++j)
                {
                    CollectionAssert.AreEqual(x[j].HashBins, y[j].HashBins);
                }
            }
        }
    }
}