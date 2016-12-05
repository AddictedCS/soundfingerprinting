namespace SoundFingerprinting.Tests.Unit.Utils
{
    using System;
    using System.Linq;

    using NUnit.Framework;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Audio.NAudio;
    using SoundFingerprinting.Builder;
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
        private readonly Random random = new Random((int)DateTime.Now.Ticks << 4);

        [Test]
        public void ShouldFindTop200Element()
        {
            var descriptor = new FastFingerprintDescriptor();

            const int Count = 4096;
            float[] floats = Enumerable.Range(0, Count).Select(elem => elem % 2 == 0 ? (float)elem : (float)-elem).ToArray();
            const int TopWavelets = 200;
            int[] indexes = Enumerable.Range(0, Count).ToArray();
            int kth = descriptor.Find(
                TopWavelets - 1,
                floats,
                indexes,
                0,
                4095);

            Assert.AreEqual(TopWavelets - 1, kth);
            for (int i = 1; i < TopWavelets; ++i)
            {
                Assert.IsTrue(Math.Abs(floats[TopWavelets - i]).CompareTo(floats[TopWavelets + i - 1]) > 0);
                Assert.IsTrue(indexes[i - 1] > Count - TopWavelets);
            }
        }

        [Test]
        public void ShouldFindTop200BoundaryTest()
        {
            var descriptor = new FastFingerprintDescriptor();
            const int TopWavelets = 200;

            float[] topElements =
                Enumerable.Repeat(1, TopWavelets).Select(elem => (float)elem).ToList().Concat(
                    Enumerable.Repeat(0, 3896).Select(elem => (float)elem)).ToArray();

            float[] randomized = topElements.OrderBy(x => random.Next(0, topElements.Length)).ToArray();

            int kth = descriptor.Find(
                TopWavelets - 1,
                randomized,
                Enumerable.Range(0, randomized.Length).ToArray(),
                0,
                randomized.Length - 1);

            Assert.AreEqual(TopWavelets - 1, kth);

            for (int i = 0; i < TopWavelets; i++)
            {
                Assert.AreEqual(1, randomized[i]);
            }
        }

        [Test]
        public void ShouldCreateExactlyTheSameFingerprints()
        {
            var fcb0 =
                new FingerprintCommandBuilder(
                    new FingerprintService(
                        new SpectrumService(new LomontFFT()),
                        new StandardHaarWaveletDecomposition(),
                        new FingerprintDescriptor(),
                        new AudioSamplesNormalizer()),
                    new LocalitySensitiveHashingAlgorithm(
                        new MinHashService(new DefaultPermutations()), new HashConverter()));

            var fcb1 = new FingerprintCommandBuilder(
                    new FingerprintService(
                        new SpectrumService(new LomontFFT()),
                        new StandardHaarWaveletDecomposition(),
                        new FastFingerprintDescriptor(), 
                        new AudioSamplesNormalizer()),
                    new LocalitySensitiveHashingAlgorithm(
                        new MinHashService(new DefaultPermutations()), new HashConverter()));

            var fingerprints0 = fcb0.BuildFingerprintCommand()
                .From(GetAudioSamples())
                .UsingServices(new NAudioService())
                .Hash()
                .Result;

            fingerprints0.Sort(
                (fingerprint, hashedFingerprint) =>
                fingerprint.SequenceNumber.CompareTo(hashedFingerprint.SequenceNumber));
                
            var fingerprints1 = fcb1.BuildFingerprintCommand()
                .From(GetAudioSamples())
                .UsingServices(new NAudioService())
                .Hash()
                .Result;

            fingerprints1.Sort(
                (fingerprint, hashedFingerprint) =>
                fingerprint.SequenceNumber.CompareTo(hashedFingerprint.SequenceNumber));

            CollectionAssert.AreEqual(
                fingerprints0.Select(f => f.SubFingerprint).ToList(),
                fingerprints1.Select(f => f.SubFingerprint).ToList());
        }
    }
}
