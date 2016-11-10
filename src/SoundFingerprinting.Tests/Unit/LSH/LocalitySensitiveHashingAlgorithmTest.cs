namespace SoundFingerprinting.Tests.Unit.LSH
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SoundFingerprinting.Data;
    using SoundFingerprinting.LSH;
    using SoundFingerprinting.MinHash;

    [TestClass]
    public class LocalitySensitiveHashingAlgorithmTest : AbstractTest
    {
        private LocalitySensitiveHashingAlgorithm lshAlgorithm;
        private Mock<IMinHashService> minHashService;

        [TestInitialize]
        public void SetUp()
        {
            minHashService = new Mock<IMinHashService>(MockBehavior.Strict);
            lshAlgorithm = new LocalitySensitiveHashingAlgorithm(minHashService.Object);
        }

        [TestCleanup]
        public void TearDown()
        {
            minHashService.VerifyAll();
        }

        [TestMethod]
        public void LshAlgorithmTest()
        {
            minHashService.Setup(service => service.Hash(GenericFingerprint)).Returns(
                new byte[] { 1, 0, 1, 0, 0, 6, 0, 1, 0, 9, 0, 2, 8, 7, 6, 3 });

            var hash = lshAlgorithm.Hash(new Fingerprint { Signature = GenericFingerprint, SequenceNumber = 5, Timestamp = 5 * 0.928 }, 4, 4);

            Assert.IsNotNull(hash);
            Assert.AreEqual(BitConverter.ToInt32(new byte[] { 1, 0, 1, 0 }, 0), hash.HashBins[0]);
            Assert.AreEqual(BitConverter.ToInt32(new byte[] { 0, 6, 0, 1 }, 0), hash.HashBins[1]);
            Assert.AreEqual(BitConverter.ToInt32(new byte[] { 0, 9, 0, 2 }, 0), hash.HashBins[2]);
            Assert.AreEqual(BitConverter.ToInt32(new byte[] { 8, 7, 6, 3 }, 0), hash.HashBins[3]);
        }

        [TestMethod]
        public void LshAlgorithmTestForNonPowerOfTwoHashKeys()
        {
            minHashService.Setup(service => service.Hash(GenericFingerprint)).Returns(
                new byte[] { 1, 0, 1, 0, 0, 6, 0, 1, 0, 9, 0, 2, 8, 7, 6 });

            var hash = lshAlgorithm.Hash(new Fingerprint { Signature = GenericFingerprint, SequenceNumber = 0, Timestamp = 0.928 }, 5, 3);

            Assert.IsNotNull(hash);
            Assert.AreEqual(BitConverter.ToInt32(new byte[] { 1, 0, 1, 0 }, 0), hash.HashBins[0]);
            Assert.AreEqual(BitConverter.ToInt32(new byte[] { 0, 0, 6, 0 }, 0), hash.HashBins[1]);
            Assert.AreEqual(BitConverter.ToInt32(new byte[] { 0, 1, 0, 0 }, 0), hash.HashBins[2]);
            Assert.AreEqual(BitConverter.ToInt32(new byte[] { 9, 0, 2, 0 }, 0), hash.HashBins[3]);
            Assert.AreEqual(BitConverter.ToInt32(new byte[] { 8, 7, 6, 0 }, 0), hash.HashBins[4]);
        }
 
        [TestMethod]
        public void FingerprintParametersAreCopiedToHashedFingerprintObject()
        {
            minHashService.Setup(service => service.Hash(GenericFingerprint)).Returns(
                new byte[] { 1, 0, 1, 0, 0, 6, 0, 1, 0, 9, 0, 2, 8, 7, 6, 3 });

            var hash = lshAlgorithm.Hash(new Fingerprint { Signature = GenericFingerprint, SequenceNumber = 5, Timestamp = 5 * 0.928 }, 4, 4);

            Assert.AreEqual(5, hash.SequenceNumber);
            Assert.AreEqual(5 * 0.928, hash.Timestamp, Epsilon);
        }
    }
}
