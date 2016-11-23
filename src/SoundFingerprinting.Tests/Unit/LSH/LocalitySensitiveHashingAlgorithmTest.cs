namespace SoundFingerprinting.Tests.Unit.LSH
{
    using Moq;

    using NUnit.Framework;

    using SoundFingerprinting.Data;
    using SoundFingerprinting.LSH;
    using SoundFingerprinting.Math;
    using SoundFingerprinting.MinHash;

    [TestFixture]
    public class LocalitySensitiveHashingAlgorithmTest : AbstractTest
    {
        private LocalitySensitiveHashingAlgorithm lshAlgorithm;
        private Mock<IMinHashService> minHashService;
        private Mock<IHashConverter> hashConverter;

        [SetUp]
        public void SetUp()
        {
            minHashService = new Mock<IMinHashService>(MockBehavior.Strict);
            hashConverter = new Mock<IHashConverter>(MockBehavior.Strict);
            lshAlgorithm = new LocalitySensitiveHashingAlgorithm(minHashService.Object, hashConverter.Object);
        }

        [TearDown]
        public void TearDown()
        {
            minHashService.VerifyAll();
            hashConverter.VerifyAll();
        }

        [Test]
        public void FingerprintParametersAreCopiedToHashedFingerprintObject()
        {
            var bytes = new byte[] { 1, 0, 1, 0, 0, 6, 0, 1, 0, 9, 0, 2, 8, 7, 6, 3 };
            hashConverter.Setup(converter => converter.ToLongs(bytes, 4)).Returns(new long[4]);
            minHashService.Setup(service => service.Hash(GenericFingerprint)).Returns(bytes);

            var hash = lshAlgorithm.Hash(new Fingerprint(GenericFingerprint, 5 * 0.928, 5), 4, 4);

            Assert.AreEqual(5, hash.SequenceNumber);
            Assert.AreEqual(5 * 0.928, hash.StartsAt, Epsilon);
        }
    }
}
