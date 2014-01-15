namespace SoundFingerprinting.Tests.Unit.Hashing
{
    using System;
    using System.Collections;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SoundFingerprinting.Hashing;
    using SoundFingerprinting.Hashing.MinHash;

    [TestClass]
    public class LocalitySensitiveHashingAlgorithmTest : AbstractTest
    {
        private LocalitySensitiveHashingAlgorithm localitySensitiveHashingAlgorithm;

        private Mock<IMinHashService> minHashService;

        [TestInitialize]
        public void SetUp()
        {
            minHashService = new Mock<IMinHashService>(MockBehavior.Strict);
            
            localitySensitiveHashingAlgorithm = new LocalitySensitiveHashingAlgorithm(minHashService.Object);
        }

        [TestCleanup]
        public void TearDown()
        {
            minHashService.VerifyAll();
        }

        [TestMethod]
        public void CombinedHashingInvokesBothAlgorithmsTest()
        {
            minHashService.Setup(service => service.Hash(GenericFingerprint)).Returns(
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 3 });

            var hash = localitySensitiveHashingAlgorithm.Hash(GenericFingerprint, 4, 4);

            Assert.IsNotNull(hash);
            Assert.AreEqual(hash.HashBins[0], BitConverter.ToInt32(new byte[] { 0, 0, 0, 0 }, 0));
            Assert.AreEqual(hash.HashBins[1], BitConverter.ToInt32(new byte[] { 0, 0, 0, 1 }, 0));
            Assert.AreEqual(hash.HashBins[2], BitConverter.ToInt32(new byte[] { 0, 0, 0, 2 }, 0));
            Assert.AreEqual(hash.HashBins[3], BitConverter.ToInt32(new byte[] { 0, 0, 0, 3 }, 0));
        }
    }
}
