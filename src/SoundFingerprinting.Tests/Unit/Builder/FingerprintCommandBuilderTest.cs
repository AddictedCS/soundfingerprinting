namespace SoundFingerprinting.Tests.Unit.Builder
{
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.LSH;

    [TestClass]
    public class FingerprintCommandBuilderTest : AbstractTest
    {
        private FingerprintCommandBuilder fingerprintCommandBuilder;

        private Mock<IFingerprintService> fingerprintService;

        private Mock<IAudioService> audioService;

        private Mock<ILocalitySensitiveHashingAlgorithm> lshAlgorithm;

        [TestInitialize]
        public void SetUp()
        {
            fingerprintService = new Mock<IFingerprintService>(MockBehavior.Strict);
            audioService = new Mock<IAudioService>(MockBehavior.Strict);
            lshAlgorithm = new Mock<ILocalitySensitiveHashingAlgorithm>(MockBehavior.Strict);

            fingerprintCommandBuilder = new FingerprintCommandBuilder(fingerprintService.Object, lshAlgorithm.Object);
        }

        [TestCleanup]
        public void TearDown()
        {
            fingerprintService.VerifyAll();
            audioService.VerifyAll();
            lshAlgorithm.VerifyAll();
        }

        [TestMethod]
        public void FingerprintsAreBuiltCorrectlyFromFile()
        {
            const string PathToAudioFile = "path-to-audio-file";
            float[] samples = TestUtilities.GenerateRandomFloatArray(SampleRate * 10);
            List<bool[]> rawFingerprints = new List<bool[]>(new[] { GenericFingerprint, GenericFingerprint, GenericFingerprint });

            audioService.Setup(service => service.ReadMonoSamplesFromFile(PathToAudioFile, SampleRate, 0, 0)).Returns(samples);
            fingerprintService.Setup(service => service.CreateFingerprints(samples, It.IsAny<DefaultFingerprintConfiguration>())).Returns(rawFingerprints);

            List<bool[]> fingerprints = fingerprintCommandBuilder.BuildFingerprintCommand()
                                  .From(PathToAudioFile)
                                  .WithDefaultFingerprintConfig()
                                  .UsingServices(services => services.AudioService = audioService.Object)
                                  .Fingerprint()
                                  .Result;

            Assert.AreEqual(3, fingerprints.Count);
            Assert.AreEqual(rawFingerprints, fingerprints);
        }

        [TestMethod]
        public void SubFingerprintsAreBuiltCorrectlyFromFileForTrack()
        {
            const string PathToAudioFile = "path-to-audio-file";
            float[] samples = TestUtilities.GenerateRandomFloatArray(SampleRate * 10);
            List<bool[]> rawFingerprints = new List<bool[]>(new[] { GenericFingerprint, GenericFingerprint, GenericFingerprint });
            
            audioService.Setup(service => service.ReadMonoSamplesFromFile(PathToAudioFile, SampleRate, 0, 0)).Returns(samples);
            fingerprintService.Setup(service => service.CreateFingerprints(samples, It.IsAny<DefaultFingerprintConfiguration>())).Returns(rawFingerprints);
            lshAlgorithm.Setup(service => service.Hash(GenericFingerprint, 25, 4)).Returns(
                new HashData(GenericSignature, GenericHashBuckets));

            var hashDatas =
                fingerprintCommandBuilder.BuildFingerprintCommand()
                                      .From(PathToAudioFile)
                                      .WithDefaultFingerprintConfig()
                                      .UsingServices(services => services.AudioService = audioService.Object)
                                      .Hash()
                                      .Result;

            Assert.AreEqual(3, hashDatas.Count);
            foreach (var hashData in hashDatas)
            {
                Assert.AreEqual(GenericSignature, hashData.SubFingerprint);
            }
        }

        [TestMethod]
        public void SubFingerprintsAreBuiltCorrectlyFromAudioSamplesForTrack()
        {
            float[] samples = TestUtilities.GenerateRandomFloatArray(SampleRate * 10);
            List<bool[]> rawFingerprints = new List<bool[]>(new[] { GenericFingerprint, GenericFingerprint, GenericFingerprint });

            fingerprintService.Setup(service => service.CreateFingerprints(samples, It.IsAny<DefaultFingerprintConfiguration>())).Returns(rawFingerprints);
            lshAlgorithm.Setup(service => service.Hash(GenericFingerprint, 25, 4)).Returns(
                new HashData(GenericSignature, GenericHashBuckets));

            var hashDatas =
                fingerprintCommandBuilder.BuildFingerprintCommand()
                                      .From(samples)
                                      .WithDefaultFingerprintConfig()
                                      .UsingServices(services => services.AudioService = audioService.Object)
                                      .Hash()
                                      .Result;

            Assert.AreEqual(3, hashDatas.Count);
            foreach (var hashData in hashDatas)
            {
                Assert.AreEqual(GenericSignature, hashData.SubFingerprint);
                Assert.AreEqual(GenericHashBuckets, hashData.HashBins);
            }
        }

        [TestMethod]
        public void SubFingerprintsAreBuiltCorrectlyFromFileForTrackStartingAtSpecificSecond()
        {
            const string PathToAudioFile = "path-to-audio-file";
            const int StartSecond = 10;
            const int SecondsToProcess = 20;
            float[] samples = TestUtilities.GenerateRandomFloatArray(SampleRate * 10);
            List<bool[]> rawFingerprints = new List<bool[]>(new[] { GenericFingerprint, GenericFingerprint, GenericFingerprint });

            audioService.Setup(service => service.ReadMonoSamplesFromFile(PathToAudioFile, SampleRate, SecondsToProcess, StartSecond)).Returns(samples);
            fingerprintService.Setup(service => service.CreateFingerprints(samples, It.IsAny<DefaultFingerprintConfiguration>())).Returns(rawFingerprints);
            lshAlgorithm.Setup(service => service.Hash(GenericFingerprint, 25, 4)).Returns(
                new HashData(GenericSignature, GenericHashBuckets));

            var hashDatas =
                fingerprintCommandBuilder.BuildFingerprintCommand()
                                      .From(PathToAudioFile, SecondsToProcess, StartSecond)
                                      .WithDefaultFingerprintConfig()
                                      .UsingServices(services => services.AudioService = audioService.Object)
                                      .Hash()
                                      .Result;

            Assert.AreEqual(3, hashDatas.Count);
            foreach (var hashData in hashDatas)
            {
                Assert.AreEqual(GenericSignature, hashData.SubFingerprint);
            }

            audioService.Verify(service => service.ReadMonoSamplesFromFile(PathToAudioFile, SampleRate, SecondsToProcess, StartSecond));
        }

        [TestMethod]
        public void SubFingerprintsAreBuiltCorrectlyFromFingerprintsTest()
        {
            List<bool[]> rawFingerprints = new List<bool[]>(new[] { GenericFingerprint, GenericFingerprint, GenericFingerprint });

            lshAlgorithm.Setup(service => service.Hash(GenericFingerprint, 25, 4)).Returns(
                new HashData(GenericSignature, GenericHashBuckets));

            var hashDatas =
                fingerprintCommandBuilder.BuildFingerprintCommand()
                                      .From(rawFingerprints)
                                      .WithDefaultFingerprintConfig()
                                      .UsingServices(services => services.AudioService = audioService.Object)
                                      .Hash()
                                      .Result;

            Assert.AreEqual(3, hashDatas.Count);
            foreach (var hashData in hashDatas)
            {
                Assert.AreEqual(GenericSignature, hashData.SubFingerprint);
            }
        }

        [TestMethod]
        public void CorrectFingerprintConfigurationIsUsedWithInstanceConfigTest()
        {
            const string PathToAudioFile = "path-to-audio-file";

            var configuration = new CustomFingerprintConfiguration { FingerprintLength = 1234 };

            var fingerprintCommand = fingerprintCommandBuilder.BuildFingerprintCommand()
                                                              .From(PathToAudioFile)
                                                              .WithFingerprintConfig(configuration)
                                                              .UsingServices(services => services.AudioService = audioService.Object);

            Assert.AreEqual(configuration, fingerprintCommand.FingerprintConfiguration);
            Assert.AreEqual(
                configuration.FingerprintLength, fingerprintCommand.FingerprintConfiguration.FingerprintLength);
        }

        [TestMethod]
        public void CorrectFingerprintConfigurationIsUsedWithTemplateConfigTest()
        {
            var fingerprintCommand = fingerprintCommandBuilder.BuildFingerprintCommand()
                                                              .From("path-to-mp3-file")
                                                              .WithFingerprintConfig<DefaultFingerprintConfiguration>()
                                                              .UsingServices(services => services.AudioService = audioService.Object);

            Assert.IsTrue(fingerprintCommand.FingerprintConfiguration is DefaultFingerprintConfiguration);
        }
    }
}
