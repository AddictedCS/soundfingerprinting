namespace SoundFingerprinting.Tests.Unit.LSH
{
    using System.Linq;

    using Moq;

    using NUnit.Framework;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.LSH;
    using SoundFingerprinting.Math;
    using SoundFingerprinting.MinHash;
    using SoundFingerprinting.Utils;

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
            hashConverter.Setup(converter => converter.ToInts(bytes, 4)).Returns(value: new[]{1,2,3,4});
            minHashService.Setup(service => service.Hash(It.IsAny<IEncodedFingerprintSchema>(), 16)).Returns(bytes);

            var hash = lshAlgorithm.Hash(
                new Fingerprint(new TinyFingerprintSchema(8192), 5 * 0.928f, 5),
                new DefaultHashingConfig { NumberOfLSHTables = 4, NumberOfMinHashesPerTable = 4, HashBuckets = 0 },
                Enumerable.Empty<string>());

            Assert.AreEqual(5, hash.SequenceNumber);
            Assert.AreEqual(5 * 0.928, hash.StartsAt, Epsilon);
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, hash.HashBins);
        }
    }
}
