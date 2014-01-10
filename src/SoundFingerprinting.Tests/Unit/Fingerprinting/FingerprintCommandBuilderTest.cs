namespace SoundFingerprinting.Tests.Unit.Fingerprinting
{
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Hashing.LSH;
    using SoundFingerprinting.Hashing.MinHash;

    [TestClass]
    public class FingerprintCommandBuilderTest : AbstractTest
    {
        private FingerprintCommandBuilder fingerprintCommandBuilder;

        private Mock<IFingerprintService> fingerprintService;

        private Mock<IAudioService> audioService;

        private Mock<IMinHashService> minHashService;

        private Mock<ILSHService> lshService;

        [TestInitialize]
        public void SetUp()
        {
            fingerprintService = new Mock<IFingerprintService>(MockBehavior.Strict);
            audioService = new Mock<IAudioService>(MockBehavior.Strict);
            minHashService = new Mock<IMinHashService>(MockBehavior.Strict);
            lshService = new Mock<ILSHService>(MockBehavior.Strict);

            fingerprintCommandBuilder = new FingerprintCommandBuilder(fingerprintService.Object, audioService.Object, minHashService.Object, lshService.Object);
        }

        [TestCleanup]
        public void TearDown()
        {
            fingerprintService.VerifyAll();
            audioService.VerifyAll();
            minHashService.VerifyAll();
        }

        [TestMethod]
        public void FingerprintsAreBuiltCorrectlyFromFile()
        {
            const string PathToAudioFile = "path-to-audio-file";
            float[] samples = TestUtilities.GenerateRandomFloatArray(SampleRate * 10);
            List<bool[]> rawFingerprints = new List<bool[]>(new[] { GenericFingerprint, GenericFingerprint, GenericFingerprint });

            audioService.Setup(service => service.ReadMonoFromFile(PathToAudioFile, SampleRate, 0, 0)).Returns(samples);
            fingerprintService.Setup(service => service.CreateFingerprints(samples, It.IsAny<DefaultFingerprintConfiguration>())).Returns(rawFingerprints);

            List<bool[]> fingerprints = fingerprintCommandBuilder.BuildFingerprintCommand()
                                  .From(PathToAudioFile)
                                  .WithDefaultFingerprintConfig()
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
            byte[] rawSubFingerprint = TestUtilities.GenerateRandomByteArray(100);

            audioService.Setup(service => service.ReadMonoFromFile(PathToAudioFile, SampleRate, 0, 0)).Returns(samples);
            fingerprintService.Setup(service => service.CreateFingerprints(samples, It.IsAny<DefaultFingerprintConfiguration>())).Returns(rawFingerprints);
            minHashService.Setup(service => service.Hash(GenericFingerprint)).Returns(rawSubFingerprint);
            lshService.Setup(service => service.Hash(rawSubFingerprint, 25, 4)).Returns(GenericHashBuckets);

            var hashDatas =
                fingerprintCommandBuilder.BuildFingerprintCommand()
                                      .From(PathToAudioFile)
                                      .WithDefaultFingerprintConfig()
                                      .Hash()
                                      .Result;

            Assert.AreEqual(3, hashDatas.Count);
            foreach (var hashData in hashDatas)
            {
                Assert.AreEqual(rawSubFingerprint, hashData.SubFingerprint);
            }
        }

        [TestMethod]
        public void SubFingerprintsAreBuiltCorrectlyFromAudioSamplesForTrack()
        {
            float[] samples = TestUtilities.GenerateRandomFloatArray(SampleRate * 10);
            List<bool[]> rawFingerprints = new List<bool[]>(new[] { GenericFingerprint, GenericFingerprint, GenericFingerprint });
            byte[] rawSubFingerprint = TestUtilities.GenerateRandomByteArray(100);

            fingerprintService.Setup(service => service.CreateFingerprints(samples, It.IsAny<DefaultFingerprintConfiguration>())).Returns(rawFingerprints);
            minHashService.Setup(service => service.Hash(GenericFingerprint)).Returns(rawSubFingerprint);
            lshService.Setup(service => service.Hash(rawSubFingerprint, 25, 4)).Returns(GenericHashBuckets);

            var hashDatas =
                fingerprintCommandBuilder.BuildFingerprintCommand()
                                      .From(samples)
                                      .WithDefaultFingerprintConfig()
                                      .Hash()
                                      .Result;

            Assert.AreEqual(3, hashDatas.Count);
            foreach (var hashData in hashDatas)
            {
                Assert.AreEqual(rawSubFingerprint, hashData.SubFingerprint);
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
            byte[] rawSubFingerprint = TestUtilities.GenerateRandomByteArray(100);

            audioService.Setup(service => service.ReadMonoFromFile(PathToAudioFile, SampleRate, SecondsToProcess, StartSecond)).Returns(samples);
            fingerprintService.Setup(service => service.CreateFingerprints(samples, It.IsAny<DefaultFingerprintConfiguration>())).Returns(rawFingerprints);
            minHashService.Setup(service => service.Hash(GenericFingerprint)).Returns(rawSubFingerprint);
            lshService.Setup(service => service.Hash(rawSubFingerprint, 25, 4)).Returns(GenericHashBuckets);

            var hashDatas =
                fingerprintCommandBuilder.BuildFingerprintCommand()
                                      .From(PathToAudioFile, SecondsToProcess, StartSecond)
                                      .WithDefaultFingerprintConfig()
                                      .Hash()
                                      .Result;

            Assert.AreEqual(3, hashDatas.Count);
            foreach (var hashData in hashDatas)
            {
                Assert.AreEqual(rawSubFingerprint, hashData.SubFingerprint);
            }

            audioService.Verify(service => service.ReadMonoFromFile(PathToAudioFile, SampleRate, SecondsToProcess, StartSecond));
        }
    }
}
