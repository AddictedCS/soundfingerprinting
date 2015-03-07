namespace SoundFingerprinting.Tests.Unit.Builder
{
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.FFT;
    using SoundFingerprinting.LSH;

    [TestClass]
    public class FingerprintCommandBuilderTest : AbstractTest
    {
        private const int NumberOfHashTables = 25;
        
        private const int NumberOfHashKeysPerTable = 4;
        
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
            const int TenSeconds = 10;
            AudioSamples samples = new AudioSamples { Samples = TestUtilities.GenerateRandomFloatArray(SampleRate * TenSeconds) };
            const int ThreeFingerprints = 3;
            var rawFingerprints = GetGenericFingerprints(ThreeFingerprints);
            audioService.Setup(service => service.ReadMonoSamplesFromFile(PathToAudioFile, SampleRate, 0, 0)).Returns(samples);
            fingerprintService.Setup(service => service.CreateFingerprints(samples, It.IsAny<DefaultFingerprintConfiguration>())).Returns(rawFingerprints);

            List<bool[]> fingerprints = fingerprintCommandBuilder.BuildFingerprintCommand()
                                  .From(PathToAudioFile)
                                  .WithDefaultFingerprintConfig()
                                  .UsingServices(audioService.Object)
                                  .Fingerprint()
                                  .Result;

            Assert.AreEqual(ThreeFingerprints, fingerprints.Count);
            Assert.AreSame(rawFingerprints, fingerprints);
        }

        [TestMethod]
        public void SubFingerprintsAreBuiltCorrectlyFromFileForTrack()
        {
            const int TenSeconds = 10;
            AudioSamples samples = new AudioSamples
                { Samples = TestUtilities.GenerateRandomFloatArray(SampleRate * TenSeconds) };
            const int ThreeFingerprints = 3;
            List<bool[]> rawFingerprints = GetGenericFingerprints(ThreeFingerprints);
            const string PathToAudioFile = "path-to-audio-file";
            audioService.Setup(service => service.ReadMonoSamplesFromFile(PathToAudioFile, SampleRate, 0, 0)).Returns(samples);
            fingerprintService.Setup(service => service.CreateFingerprints(samples, It.IsAny<DefaultFingerprintConfiguration>())).Returns(rawFingerprints);
            lshAlgorithm.Setup(service => service.Hash(GenericFingerprint, NumberOfHashTables, NumberOfHashKeysPerTable)).Returns(
                new HashData(GenericSignature, GenericHashBuckets));

            var hashDatas = fingerprintCommandBuilder.BuildFingerprintCommand()
                                      .From(PathToAudioFile)
                                      .WithDefaultFingerprintConfig()
                                      .UsingServices(audioService.Object)
                                      .Hash()
                                      .Result;

            Assert.AreEqual(ThreeFingerprints, hashDatas.Count);
            foreach (var hashData in hashDatas)
            {
                Assert.AreSame(GenericSignature, hashData.SubFingerprint);
            }
        }

        [TestMethod]
        public void SubFingerprintsAreBuiltCorrectlyFromAudioSamplesForTrack()
        {
            const int TenSeconds = 10;
            AudioSamples samples = new AudioSamples
                { Samples = TestUtilities.GenerateRandomFloatArray(SampleRate * TenSeconds) };
            const int ThreeFingerprints = 3;
            List<bool[]> rawFingerprints = GetGenericFingerprints(ThreeFingerprints);
            fingerprintService.Setup(service => service.CreateFingerprints(samples, It.IsAny<DefaultFingerprintConfiguration>())).Returns(rawFingerprints);
            lshAlgorithm.Setup(service => service.Hash(GenericFingerprint, NumberOfHashTables, NumberOfHashKeysPerTable)).Returns(new HashData(GenericSignature, GenericHashBuckets));

            var hashDatas = fingerprintCommandBuilder.BuildFingerprintCommand()
                                      .From(samples)
                                      .WithDefaultFingerprintConfig()
                                      .UsingServices(audioService.Object)
                                      .Hash()
                                      .Result;

            Assert.AreEqual(ThreeFingerprints, hashDatas.Count);
            foreach (var hashData in hashDatas)
            {
                Assert.AreSame(GenericSignature, hashData.SubFingerprint);
                Assert.AreSame(GenericHashBuckets, hashData.HashBins);
            }
        }

        [TestMethod]
        public void SubFingerprintsAreBuiltCorrectlyFromFileForTrackStartingAtSpecificSecond()
        {
            const string PathToAudioFile = "path-to-audio-file";
            const int StartSecond = 10;
            const int SecondsToProcess = 20;
            AudioSamples samples = new AudioSamples
                { Samples = TestUtilities.GenerateRandomFloatArray(SampleRate * 10) };
            const int ThreeFingerprints = 3;
            List<bool[]> rawFingerprints = GetGenericFingerprints(ThreeFingerprints);

            audioService.Setup(service => service.ReadMonoSamplesFromFile(PathToAudioFile, SampleRate, SecondsToProcess, StartSecond)).Returns(samples);
            fingerprintService.Setup(service => service.CreateFingerprints(samples, It.IsAny<DefaultFingerprintConfiguration>())).Returns(rawFingerprints);
            lshAlgorithm.Setup(service => service.Hash(GenericFingerprint, NumberOfHashTables, NumberOfHashKeysPerTable)).Returns(
                new HashData(GenericSignature, GenericHashBuckets));

            var hashDatas = fingerprintCommandBuilder.BuildFingerprintCommand()
                                      .From(PathToAudioFile, SecondsToProcess, StartSecond)
                                      .WithDefaultFingerprintConfig()
                                      .UsingServices(audioService.Object)
                                      .Hash()
                                      .Result;

            Assert.AreEqual(ThreeFingerprints, hashDatas.Count);
            foreach (var hashData in hashDatas)
            {
                Assert.AreSame(GenericSignature, hashData.SubFingerprint);
            }

            audioService.Verify(service => service.ReadMonoSamplesFromFile(PathToAudioFile, SampleRate, SecondsToProcess, StartSecond));
        }

        [TestMethod]
        public void SubFingerprintsAreBuiltCorrectlyFromFingerprintsTest()
        {
            const int ThreeFingerprints = 3;
            List<bool[]> rawFingerprints = GetGenericFingerprints(ThreeFingerprints);

            lshAlgorithm.Setup(service => service.Hash(GenericFingerprint, NumberOfHashTables, NumberOfHashKeysPerTable)).Returns(
                new HashData(GenericSignature, GenericHashBuckets));

            var hashDatas = fingerprintCommandBuilder.BuildFingerprintCommand()
                                      .From(rawFingerprints)
                                      .WithDefaultFingerprintConfig()
                                      .UsingServices(audioService.Object)
                                      .Hash()
                                      .Result;

            Assert.AreEqual(ThreeFingerprints, hashDatas.Count);
            foreach (var hashData in hashDatas)
            {
                Assert.AreSame(GenericSignature, hashData.SubFingerprint);
            }
        }

        [TestMethod]
        public void CorrectFingerprintConfigurationIsUsedWithInstanceConfigTest()
        {
            const string PathToAudioFile = "path-to-audio-file";

            var configuration = new CustomFingerprintConfiguration
                { SpectrogramConfig = new CustomSpectrogramConfig { ImageLength = 1234 } };
             
            var fingerprintCommand = fingerprintCommandBuilder.BuildFingerprintCommand()
                                                              .From(PathToAudioFile)
                                                              .WithFingerprintConfig(configuration)
                                                              .UsingServices(audioService.Object);

            Assert.AreSame(configuration, fingerprintCommand.FingerprintConfiguration);
            Assert.AreEqual(configuration.SpectrogramConfig.ImageLength, fingerprintCommand.FingerprintConfiguration.SpectrogramConfig.ImageLength);
        }

        [TestMethod]
        public void CorrectFingerprintConfigurationIsUsedWithTemplateConfigTest()
        {
            var fingerprintCommand = fingerprintCommandBuilder.BuildFingerprintCommand()
                                                              .From("path-to-mp3-file")
                                                              .WithFingerprintConfig<DefaultFingerprintConfiguration>()
                                                              .UsingServices(audioService.Object);

            Assert.IsInstanceOfType(fingerprintCommand.FingerprintConfiguration, typeof(DefaultFingerprintConfiguration));
        }

        [TestMethod]
        public void TestSpectralImagesAreCreatedByFingerprintCommand()
        {
            var samples = GetAudioSamples(TestUtilities.GenerateRandomFloatArray(10 * 5512));
            audioService.Setup(service => service.ReadMonoSamplesFromFile("path-to-audio-file", SampleRate, 0, 0)).Returns(samples);
            var spectralImages = new List<SpectralImage>();
            fingerprintService.Setup(service => service.CreateSpectralImages(samples, It.IsAny<DefaultFingerprintConfiguration>())).Returns(spectralImages);

            var resultedSpectralImages = fingerprintCommandBuilder.BuildFingerprintCommand()
                                     .From("path-to-audio-file")
                                     .WithDefaultFingerprintConfig()
                                     .UsingServices(audioService.Object)
                                     .CreateSpectralImages()
                                     .Result;

            Assert.AreSame(spectralImages, resultedSpectralImages);
        }

        private AudioSamples GetAudioSamples(float[] samples)
        {
            return new AudioSamples { Samples = samples, Duration = 0, Origin = string.Empty, SampleRate = 0 }; // TODO Fix this
        }

        private List<bool[]> GetGenericFingerprints(int count)
        {
            var list = new List<bool[]>();
            for (int i = 0; i < count; i++)
            {
                list.Add(GenericFingerprint);
            }

            return list;
        }
    }
}
