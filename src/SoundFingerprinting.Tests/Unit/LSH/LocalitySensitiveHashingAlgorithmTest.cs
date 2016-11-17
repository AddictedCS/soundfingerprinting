namespace SoundFingerprinting.Tests.Unit.LSH
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SoundFingerprinting.Data;
    using SoundFingerprinting.LSH;
    using SoundFingerprinting.Math;
    using SoundFingerprinting.MinHash;

    [TestClass]
    public class LocalitySensitiveHashingAlgorithmTest : AbstractTest
    {
        private LocalitySensitiveHashingAlgorithm lshAlgorithm;
        private Mock<IMinHashService> minHashService;
        private Mock<IHashConverter> hashConverter;

        [TestInitialize]
        public void SetUp()
        {
            minHashService = new Mock<IMinHashService>(MockBehavior.Strict);
            hashConverter = new Mock<IHashConverter>(MockBehavior.Strict);
            lshAlgorithm = new LocalitySensitiveHashingAlgorithm(minHashService.Object, hashConverter.Object);
        }

        [TestCleanup]
        public void TearDown()
        {
            minHashService.VerifyAll();
            hashConverter.VerifyAll();
        }

        [TestMethod]
        public void FingerprintParametersAreCopiedToHashedFingerprintObject()
        {
            var bytes = new byte[] { 1, 0, 1, 0, 0, 6, 0, 1, 0, 9, 0, 2, 8, 7, 6, 3 };
            hashConverter.Setup(converter => converter.ToLongs(bytes, 4)).Returns(new long[4]);
            minHashService.Setup(service => service.Hash(GenericFingerprint)).Returns(bytes);

            var hash = lshAlgorithm.Hash(new Fingerprint { Signature = GenericFingerprint, SequenceNumber = 5, Timestamp = 5 * 0.928 }, 4, 4);

            Assert.AreEqual(5, hash.SequenceNumber);
            Assert.AreEqual(5 * 0.928, hash.StartsAt, Epsilon);
        }
    }
}
