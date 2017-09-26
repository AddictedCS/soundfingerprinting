namespace SoundFingerprinting.Tests.Unit.Builder
{
    using System.Collections.Generic;
    using System.Linq;

    using Moq;

    using NUnit.Framework;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Command;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.LSH;
    using SoundFingerprinting.Utils;

    [TestFixture]
    public class FingerprintCommandBuilderTest : AbstractTest
    {
        private const int NumberOfHashKeysPerTable = 4;
        
        private FingerprintCommandBuilder fingerprintCommandBuilder;

        private Mock<IFingerprintService> fingerprintService;

        private Mock<IAudioService> audioService;

        private Mock<ILocalitySensitiveHashingAlgorithm> lshAlgorithm;

        [SetUp]
        public void SetUp()
        {
            fingerprintService = new Mock<IFingerprintService>(MockBehavior.Strict);
            audioService = new Mock<IAudioService>(MockBehavior.Strict);
            lshAlgorithm = new Mock<ILocalitySensitiveHashingAlgorithm>(MockBehavior.Strict);

            fingerprintCommandBuilder = new FingerprintCommandBuilder(fingerprintService.Object, lshAlgorithm.Object);
        }

        [TearDown]
        public void TearDown()
        {
            fingerprintService.VerifyAll();
            audioService.VerifyAll();
            lshAlgorithm.VerifyAll();
        }

        [Test]
        public void FingerprintsAreBuiltCorrectlyFromFile()
        {
            const string PathToAudioFile = "path-to-audio-file";
            const int TenSeconds = 10;
            var samples = new AudioSamples { Samples = TestUtilities.GenerateRandomFloatArray(SampleRate * TenSeconds) };
            const int ThreeFingerprints = 3;
            var rawFingerprints = GetGenericFingerprints(ThreeFingerprints);
            audioService.Setup(service => service.ReadMonoSamplesFromFile(PathToAudioFile, SampleRate)).Returns(samples);
            fingerprintService.Setup(service => service.CreateFingerprints(samples, It.IsAny<DefaultFingerprintConfiguration>())).Returns(rawFingerprints);

            var fingerprints = ((FingerprintCommand)fingerprintCommandBuilder.BuildFingerprintCommand()
                                  .From(PathToAudioFile)
                                  .UsingServices(audioService.Object))
                                  .Fingerprint()
                                  .Result;

            Assert.AreEqual(ThreeFingerprints, fingerprints.Count);
            Assert.AreSame(rawFingerprints, fingerprints);
        }

        [Test]
        public void SubFingerprintsAreBuiltCorrectlyFromFileForTrack()
        {
            const int TenSeconds = 10;
            var samples = new AudioSamples { Samples = TestUtilities.GenerateRandomFloatArray(SampleRate * TenSeconds) };
            const int ThreeFingerprints = 3;
            var rawFingerprints = GetGenericFingerprints(ThreeFingerprints);
            const string PathToAudioFile = "path-to-audio-file";
            audioService.Setup(service => service.ReadMonoSamplesFromFile(PathToAudioFile, SampleRate)).Returns(samples);
            fingerprintService.Setup(
                service => service.CreateFingerprints(samples, It.IsAny<DefaultFingerprintConfiguration>())).Returns(
                    rawFingerprints);
            byte[] genericSignature = GenericSignature();
            lshAlgorithm.Setup(service => service.Hash(It.IsAny<Fingerprint>(), NumberOfHashTables, NumberOfHashKeysPerTable, It.IsAny<IEnumerable<string>>())).Returns(new HashedFingerprint(genericSignature, GenericHashBuckets(), 0, 0.928, Enumerable.Empty<string>()));

            var hashDatas = fingerprintCommandBuilder.BuildFingerprintCommand()
                                                     .From(PathToAudioFile)
                                                     .UsingServices(audioService.Object)
                                                     .Hash()
                                                     .Result;

            Assert.AreEqual(ThreeFingerprints, hashDatas.Count);
            foreach (var hashData in hashDatas)
            {
                CollectionAssert.AreEqual(genericSignature, hashData.SubFingerprint);
            }
        }

        [Test]
        public void SubFingerprintsAreBuiltCorrectlyFromAudioSamplesForTrack()
        {
            const int TenSeconds = 10;
            var samples = new AudioSamples { Samples = TestUtilities.GenerateRandomFloatArray(SampleRate * TenSeconds) };
            const int ThreeFingerprints = 3;
            var rawFingerprints = GetGenericFingerprints(ThreeFingerprints);
            fingerprintService.Setup(service => service.CreateFingerprints(samples, It.IsAny<DefaultFingerprintConfiguration>())).Returns(rawFingerprints);
            lshAlgorithm.Setup(service => service.Hash(It.IsAny<Fingerprint>(), NumberOfHashTables, NumberOfHashKeysPerTable, It.IsAny<IEnumerable<string>>())).Returns(new HashedFingerprint(GenericSignature(), GenericHashBuckets(), 0, 0, Enumerable.Empty<string>()));

            var hashDatas = fingerprintCommandBuilder.BuildFingerprintCommand()
                                      .From(samples)
                                      .UsingServices(audioService.Object)
                                      .Hash()
                                      .Result;

            Assert.AreEqual(ThreeFingerprints, hashDatas.Count);
            byte[] genericSignature = GenericSignature();
            long[] genericHashBuckets = GenericHashBuckets();

            foreach (var hashData in hashDatas)
            {
                CollectionAssert.AreEqual(genericSignature, hashData.SubFingerprint);
                CollectionAssert.AreEqual(genericHashBuckets, hashData.HashBins);
            }
        }

        [Test]
        public void SubFingerprintsAreBuiltCorrectlyFromFileForTrackStartingAtSpecificSecond()
        {
            const string PathToAudioFile = "path-to-audio-file";
            const int StartSecond = 10;
            const int SecondsToProcess = 20;
            AudioSamples samples = new AudioSamples
                { Samples = TestUtilities.GenerateRandomFloatArray(SampleRate * 10) };
            const int ThreeFingerprints = 3;
            var rawFingerprints = GetGenericFingerprints(ThreeFingerprints);

            audioService.Setup(service => service.ReadMonoSamplesFromFile(PathToAudioFile, SampleRate, SecondsToProcess, StartSecond)).Returns(samples);
            fingerprintService.Setup(service => service.CreateFingerprints(samples, It.IsAny<DefaultFingerprintConfiguration>())).Returns(rawFingerprints);
            byte[] genericSignature = GenericSignature();
            lshAlgorithm.Setup(service => service.Hash(It.IsAny<Fingerprint>(), NumberOfHashTables, NumberOfHashKeysPerTable, It.IsAny<IEnumerable<string>>())).Returns(
                new HashedFingerprint(genericSignature, GenericHashBuckets(), 0, 0, Enumerable.Empty<string>()));

            var hashDatas = fingerprintCommandBuilder.BuildFingerprintCommand()
                                      .From(PathToAudioFile, SecondsToProcess, StartSecond)
                                      .UsingServices(audioService.Object)
                                      .Hash()
                                      .Result;

            Assert.AreEqual(ThreeFingerprints, hashDatas.Count);
            foreach (var hashData in hashDatas)
            {
                CollectionAssert.AreEqual(genericSignature, hashData.SubFingerprint);
            }

            audioService.Verify(service => service.ReadMonoSamplesFromFile(PathToAudioFile, SampleRate, SecondsToProcess, StartSecond));
        }

        [Test]
        public void CorrectFingerprintConfigurationIsUsedWithInstanceConfigTest()
        {
            const string PathToAudioFile = "path-to-audio-file";

            var configuration = new DefaultFingerprintConfiguration { SpectrogramConfig = new DefaultSpectrogramConfig { ImageLength = 1234 } };
             
            var fingerprintCommand = fingerprintCommandBuilder.BuildFingerprintCommand()
                                                              .From(PathToAudioFile)
                                                              .WithFingerprintConfig(configuration)
                                                              .UsingServices(audioService.Object);

            Assert.AreSame(configuration, fingerprintCommand.FingerprintConfiguration);
            Assert.AreEqual(configuration.SpectrogramConfig.ImageLength, fingerprintCommand.FingerprintConfiguration.SpectrogramConfig.ImageLength);
        }

        private List<Fingerprint> GetGenericFingerprints(int count)
        {
            var list = new List<Fingerprint>();
            for (int i = 0; i < count; i++)
            {
                list.Add(new Fingerprint(new TinyFingerprintSchema(8192), i * 0.928, i));
            }

            return list;
        }
    }
}
