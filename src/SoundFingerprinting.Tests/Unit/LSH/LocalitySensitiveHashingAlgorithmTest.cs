namespace SoundFingerprinting.Tests.Unit.LSH
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SoundFingerprinting.Infrastructure;
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
            DependencyResolver.Current.Bind<IMinHashService, IMinHashService>(minHashService.Object);
            lshAlgorithm = new LocalitySensitiveHashingAlgorithm();
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

            var hash = lshAlgorithm.Hash(GenericFingerprint, 4, 4);

            Assert.IsNotNull(hash);
            Assert.AreEqual(hash.HashBins[0], BitConverter.ToInt32(new byte[] { 1, 0, 1, 0 }, 0));
            Assert.AreEqual(hash.HashBins[1], BitConverter.ToInt32(new byte[] { 0, 6, 0, 1 }, 0));
            Assert.AreEqual(hash.HashBins[2], BitConverter.ToInt32(new byte[] { 0, 9, 0, 2 }, 0));
            Assert.AreEqual(hash.HashBins[3], BitConverter.ToInt32(new byte[] { 8, 7, 6, 3 }, 0));
        }

        [TestMethod]
        public void LshAlgorithmTestForNonPowerOfTwoHashKeys()
        {
            minHashService.Setup(service => service.Hash(GenericFingerprint)).Returns(
                new byte[] { 1, 0, 1, 0, 0, 6, 0, 1, 0, 9, 0, 2, 8, 7, 6 });

            var hash = lshAlgorithm.Hash(GenericFingerprint, 5, 3);

            Assert.IsNotNull(hash);
            Assert.AreEqual(hash.HashBins[0], BitConverter.ToInt32(new byte[] { 1, 0, 1, 0 }, 0));
            Assert.AreEqual(hash.HashBins[1], BitConverter.ToInt32(new byte[] { 0, 0, 6, 0 }, 0));
            Assert.AreEqual(hash.HashBins[2], BitConverter.ToInt32(new byte[] { 0, 1, 0, 0 }, 0));
            Assert.AreEqual(hash.HashBins[3], BitConverter.ToInt32(new byte[] { 9, 0, 2, 0 }, 0));
            Assert.AreEqual(hash.HashBins[4], BitConverter.ToInt32(new byte[] { 8, 7, 6, 0 }, 0));
        }
    }
}
