namespace SoundFingerprinting.Tests.Unit.MinHash
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SoundFingerprinting.MinHash;

    [TestClass]
    public class CachedPermutationsTest : AbstractTest
    {
        private CachedPermutations cachedPermutations;

        private Mock<IPermutations> permutations;

        [TestInitialize]
        public void SetUp()
        {
            permutations = new Mock<IPermutations>(MockBehavior.Strict);
            cachedPermutations = new CachedPermutations(permutations.Object);
        }

        [TestCleanup]
        public void TearDown()
        {
            permutations.VerifyAll();
        }

        [TestMethod]
        public void PermutationsAreCachedTest()
        {
            int[][] intPerms = new int[][] { };
            permutations.Setup(perms => perms.GetPermutations()).Returns(intPerms);

            for (var i = 0; i < 100; i++)
            {
                var actualPerms = cachedPermutations.GetPermutations();
                Assert.AreEqual(intPerms, actualPerms);
            }

            permutations.Verify(perms => perms.GetPermutations(), Times.Once());
        }
    }
}
