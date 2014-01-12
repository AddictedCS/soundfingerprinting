namespace SoundFingerprinting.Tests.Unit.Hashing
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SoundFingerprinting.Hashing;
    using SoundFingerprinting.Hashing.LSH;
    using SoundFingerprinting.Hashing.MinHash;

    [TestClass]
    public class CombinedHashingAlgorithmTest : AbstractTest
    {
        private CombinedHashingAlgorithm combinedHashingAlgorithm;

        private Mock<IMinHashService> minHashService;

        private Mock<ILSHService> lshService;

        [TestInitialize]
        public void SetUp()
        {
            minHashService = new Mock<IMinHashService>(MockBehavior.Strict);
            lshService = new Mock<ILSHService>(MockBehavior.Strict);

            combinedHashingAlgorithm = new CombinedHashingAlgorithm(minHashService.Object, lshService.Object);
        }

        [TestCleanup]
        public void TearDown()
        {
            minHashService.VerifyAll();
            lshService.VerifyAll();
        }

        [TestMethod]
        public void DependencyResolverTest()
        {
            var instance = new CombinedHashingAlgorithm();
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void CombinedHashingInvokesBothAlgorithmsTest()
        {
            minHashService.Setup(service => service.Hash(GenericFingerprint)).Returns(GenericSignature);
            lshService.Setup(service => service.Hash(GenericSignature, 25, 4)).Returns(GenericHashBuckets);

            var tupple = combinedHashingAlgorithm.Hash(GenericFingerprint, 25, 4);

            Assert.IsNotNull(tupple);
            Assert.AreEqual(GenericSignature, tupple.Item1);
            Assert.AreEqual(GenericHashBuckets, tupple.Item2);
        }
    }
}
