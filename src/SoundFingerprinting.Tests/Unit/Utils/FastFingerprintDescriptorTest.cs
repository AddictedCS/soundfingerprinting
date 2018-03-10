namespace SoundFingerprinting.Tests.Unit.Utils
{
    using System;

    using NUnit.Framework;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.FFT;
    using SoundFingerprinting.LSH;
    using SoundFingerprinting.Math;
    using SoundFingerprinting.MinHash;
    using SoundFingerprinting.Tests.Integration;
    using SoundFingerprinting.Utils;
    using SoundFingerprinting.Wavelets;

    [TestFixture]
    public class FastFingerprintDescriptorTest : IntegrationWithSampleFilesTest
    {
        [Test]
        public void ShouldCreateExactlyTheSameFingerprints()
        {
            var fcb0 = new FingerprintCommandBuilder(
                new FingerprintService(
                    new SpectrumService(new LomontFFT(), new LogUtility()),
                    new LocalitySensitiveHashingAlgorithm(
                        new MinHashService(new DefaultPermutations()),
                        new HashConverter()),
                    new StandardHaarWaveletDecomposition(),
                    new FingerprintDescriptor()));

            var fcb1 = new FingerprintCommandBuilder(
                new FingerprintService(
                    new SpectrumService(new LomontFFT(), new LogUtility()),
                    new LocalitySensitiveHashingAlgorithm(
                        new MinHashService(new DefaultPermutations()),
                        new HashConverter()),
                    new StandardHaarWaveletDecomposition(),
                    new FastFingerprintDescriptor()));

            var audioService = new SoundFingerprintingAudioService();
            var audioSamples = GetAudioSamples();

            int testRuns = 5;
            for (int i = 0; i < testRuns; ++i)
            {
                var hashDatas0 = fcb0.BuildFingerprintCommand()
                    .From(audioSamples)
                    .UsingServices(audioService)
                    .Hash()
                    .Result;

                var hashDatas1 = fcb1.BuildFingerprintCommand()
                    .From(audioSamples)
                    .UsingServices(audioService)
                    .Hash()
                    .Result;

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
                new LocalitySensitiveHashingAlgorithm(
                    new MinHashService(new DefaultPermutations()),
                    new HashConverter()),
                new StandardHaarWaveletDecomposition(),
                new FingerprintDescriptor());

            var fastFingerprintService = new FingerprintService(
                new SpectrumService(new LomontFFT(), new LogUtility()),
                new LocalitySensitiveHashingAlgorithm(
                    new MinHashService(new DefaultPermutations()),
                    new HashConverter()),
                new StandardHaarWaveletDecomposition(),
                new FastFingerprintDescriptor());

            int runs = 10;
            for (int i = 0; i < runs; ++i)
            {
                var x = fingerprintService.CreateFingerprints(audioSamples, new DefaultFingerprintConfiguration());
                var y = fastFingerprintService.CreateFingerprints(audioSamples, new DefaultFingerprintConfiguration());

                for (int j = 0; j < x.Count; ++j)
                {
                    CollectionAssert.AreEqual(x[j].HashBins, y[j].HashBins);
                }
            }
        }
    }
}