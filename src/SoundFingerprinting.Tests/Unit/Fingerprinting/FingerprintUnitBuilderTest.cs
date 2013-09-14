namespace SoundFingerprinting.Tests.Unit.Fingerprinting
{
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Dao.Entities;
    using SoundFingerprinting.Hashing.MinHash;

    [TestClass]
    public class FingerprintUnitBuilderTest : AbstractTest
    {
        private FingerprintUnitBuilder fingerprintUnitBuilder;

        private Mock<IFingerprintService> fingerprintService;

        private Mock<IAudioService> audioService;

        private Mock<IMinHashService> minHashService;

        [TestInitialize]
        public void SetUp()
        {
            fingerprintService = new Mock<IFingerprintService>(MockBehavior.Strict);
            audioService = new Mock<IAudioService>(MockBehavior.Strict);
            minHashService = new Mock<IMinHashService>(MockBehavior.Strict);

            fingerprintUnitBuilder = new FingerprintUnitBuilder(fingerprintService.Object, audioService.Object, minHashService.Object);
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
            fingerprintService.Setup(service => service.CreateFingerprints(samples, It.IsAny<DefaultFingerprintingConfiguration>())).Returns(rawFingerprints);

            List<bool[]> fingerprints = fingerprintUnitBuilder.BuildAudioFingerprintingUnit()
                                  .From(PathToAudioFile)
                                  .WithDefaultAlgorithmConfiguration()
                                  .FingerprintIt()
                                  .AsIs()
                                  .Result;

            Assert.AreEqual(3, fingerprints.Count);
            Assert.AreEqual(rawFingerprints, fingerprints);
        }

        [TestMethod]
        public void FingerprintsAreBuiltCorrectlyFromFileForSpecificTrack()
        {
            const string PathToAudioFile = "path-to-audio-file";
            const int TrackId = 101;
            float[] samples = TestUtilities.GenerateRandomFloatArray(SampleRate * 10);
            List<bool[]> rawFingerprints = new List<bool[]>(new[] { GenericFingerprint, GenericFingerprint, GenericFingerprint });
            
            audioService.Setup(service => service.ReadMonoFromFile(PathToAudioFile, SampleRate, 0, 0)).Returns(samples);
            fingerprintService.Setup(service => service.CreateFingerprints(samples, It.IsAny<DefaultFingerprintingConfiguration>())).Returns(rawFingerprints);
            
            List<Fingerprint> fingerprints = fingerprintUnitBuilder.BuildAudioFingerprintingUnit()
                                  .From(PathToAudioFile)
                                  .WithDefaultAlgorithmConfiguration()
                                  .FingerprintIt()
                                  .ForTrack(TrackId)
                                  .Result;

            Assert.AreEqual(3, fingerprints.Count);
            int count = 0;
            foreach (var fingerprint in fingerprints)
            {
                Assert.AreEqual(count, fingerprint.SongOrder);
                Assert.AreEqual(rawFingerprints[count], fingerprint.Signature);
                Assert.AreEqual(TrackId, fingerprint.TrackId);
                Assert.AreEqual(3, fingerprint.TotalFingerprintsPerTrack);
                count++;
            }
        }

        [TestMethod]
        public void SubFingerprintsAreBuiltCorrectlyFromFileForTrack()
        {
            const string PathToAudioFile = "path-to-audio-file";
            const int TrackId = 101;
            float[] samples = TestUtilities.GenerateRandomFloatArray(SampleRate * 10);
            List<bool[]> rawFingerprints = new List<bool[]>(new[] { GenericFingerprint, GenericFingerprint, GenericFingerprint });
            byte[] rawSubFingerprint = TestUtilities.GenerateRandomByteArray(100);

            audioService.Setup(service => service.ReadMonoFromFile(PathToAudioFile, SampleRate, 0, 0)).Returns(samples);
            fingerprintService.Setup(service => service.CreateFingerprints(samples, It.IsAny<DefaultFingerprintingConfiguration>())).Returns(rawFingerprints);
            minHashService.Setup(service => service.Hash(GenericFingerprint)).Returns(rawSubFingerprint);

            List<SubFingerprint> subFingerprints =
                fingerprintUnitBuilder.BuildAudioFingerprintingUnit()
                                      .From(PathToAudioFile)
                                      .WithDefaultAlgorithmConfiguration()
                                      .FingerprintIt()
                                      .HashIt()
                                      .ForTrack(TrackId)
                                      .Result;

            Assert.AreEqual(3, subFingerprints.Count);
            foreach (var fingerprint in subFingerprints)
            {
                Assert.AreEqual(rawSubFingerprint, fingerprint.Signature);
                Assert.AreEqual(TrackId, fingerprint.TrackId);
            }
        }

        [TestMethod]
        public void SubFingerprintsAreBuiltCorrectlyFromAudioSamplesForTrack()
        {
            const int TrackId = 101;
            float[] samples = TestUtilities.GenerateRandomFloatArray(SampleRate * 10);
            List<bool[]> rawFingerprints = new List<bool[]>(new[] { GenericFingerprint, GenericFingerprint, GenericFingerprint });
            byte[] rawSubFingerprint = TestUtilities.GenerateRandomByteArray(100);

            fingerprintService.Setup(service => service.CreateFingerprints(samples, It.IsAny<DefaultFingerprintingConfiguration>())).Returns(rawFingerprints);
            minHashService.Setup(service => service.Hash(GenericFingerprint)).Returns(rawSubFingerprint);

            List<SubFingerprint> subFingerprints =
                fingerprintUnitBuilder.BuildAudioFingerprintingUnit()
                                      .From(samples)
                                      .WithDefaultAlgorithmConfiguration()
                                      .FingerprintIt()
                                      .HashIt()
                                      .ForTrack(TrackId)
                                      .Result;

            Assert.AreEqual(3, subFingerprints.Count);
            foreach (var fingerprint in subFingerprints)
            {
                Assert.AreEqual(rawSubFingerprint, fingerprint.Signature);
                Assert.AreEqual(TrackId, fingerprint.TrackId);
            }
        }

        [TestMethod]
        public void SubFingerprintsAreBuiltCorrectlyFromFileForTrackStartingAtSpecificSecond()
        {
            const string PathToAudioFile = "path-to-audio-file";
            const int TrackId = 101;
            const int StartSecond = 10;
            const int SecondsToProcess = 20;
            float[] samples = TestUtilities.GenerateRandomFloatArray(SampleRate * 10);
            List<bool[]> rawFingerprints = new List<bool[]>(new[] { GenericFingerprint, GenericFingerprint, GenericFingerprint });
            byte[] rawSubFingerprint = TestUtilities.GenerateRandomByteArray(100);

            audioService.Setup(service => service.ReadMonoFromFile(PathToAudioFile, SampleRate, SecondsToProcess, StartSecond)).Returns(samples);
            fingerprintService.Setup(service => service.CreateFingerprints(samples, It.IsAny<DefaultFingerprintingConfiguration>())).Returns(rawFingerprints);
            minHashService.Setup(service => service.Hash(GenericFingerprint)).Returns(rawSubFingerprint);

            List<SubFingerprint> subFingerprints =
                fingerprintUnitBuilder.BuildAudioFingerprintingUnit()
                                      .From(PathToAudioFile, SecondsToProcess, StartSecond)
                                      .WithDefaultAlgorithmConfiguration()
                                      .FingerprintIt()
                                      .HashIt()
                                      .ForTrack(TrackId)
                                      .Result;

            Assert.AreEqual(3, subFingerprints.Count);
            foreach (var fingerprint in subFingerprints)
            {
                Assert.AreEqual(rawSubFingerprint, fingerprint.Signature);
                Assert.AreEqual(TrackId, fingerprint.TrackId);
            }

            audioService.Verify(service => service.ReadMonoFromFile(PathToAudioFile, SampleRate, SecondsToProcess, StartSecond));
        }
    }
}
