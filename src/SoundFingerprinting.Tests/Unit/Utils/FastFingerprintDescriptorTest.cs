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
    using SoundFingerprinting.MinHash;
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
                    new LocalitySensitiveHashingAlgorithm(new MinHashService(new DefaultPermutations())),
                    new StandardHaarWaveletDecomposition(),
                    new FingerprintDescriptor()));

            var fcbWithFastFingerprintDescriptor = new FingerprintCommandBuilder(
                new FingerprintService(
                    new SpectrumService(new LomontFFT(), new LogUtility()),
                    new LocalitySensitiveHashingAlgorithm(new MinHashService(new DefaultPermutations())),
                    new StandardHaarWaveletDecomposition(),
                    new FastFingerprintDescriptor()));

            var audioService = new SoundFingerprintingAudioService();
            var audioSamples = GetAudioSamples();

            int runs = 5;
            for (int i = 0; i < runs; ++i)
            {
                var hashDatas0 = await fcbWithOldFingerprintDescriptor.BuildFingerprintCommand()
                    .From(audioSamples)
                    .UsingServices(audioService)
                    .Hash();

                var hashDatas1 = await fcbWithFastFingerprintDescriptor.BuildFingerprintCommand()
                    .From(audioSamples)
                    .UsingServices(audioService)
                    .Hash();

                AssertHashDatasAreTheSame(hashDatas0, hashDatas1);
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
                new LocalitySensitiveHashingAlgorithm(new MinHashService(new DefaultPermutations())),
                new StandardHaarWaveletDecomposition(),
                new FingerprintDescriptor());

            var fastFingerprintService = new FingerprintService(
                new SpectrumService(new LomontFFT(), new LogUtility()),
                new LocalitySensitiveHashingAlgorithm(new MinHashService(new DefaultPermutations())),
                new StandardHaarWaveletDecomposition(),
                new FastFingerprintDescriptor());

            int runs = 10;
            var configuration = new DefaultFingerprintConfiguration();
            for (int i = 0; i < runs; ++i)
            {
                var x = fingerprintService.CreateFingerprints(audioSamples, configuration);
                var y = fastFingerprintService.CreateFingerprints(audioSamples, configuration);

                for (int j = 0; j < x.Count; ++j)
                {
                    CollectionAssert.AreEqual(x[j].HashBins, y[j].HashBins);
                }
            }
        }
    }
}