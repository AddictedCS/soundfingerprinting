namespace SoundFingerprinting.Tests.Unit.MinHash
{
    using Moq;

    using NUnit.Framework;

    using SoundFingerprinting.MinHash;

    [TestFixture]
    public class CachedPermutationsTest : AbstractTest
    {
        private CachedPermutations cachedPermutations;

        private Mock<IPermutations> permutations;

        [SetUp]
        public void SetUp()
        {
            permutations = new Mock<IPermutations>(MockBehavior.Strict);
            cachedPermutations = new CachedPermutations(permutations.Object);
        }

        [TearDown]
        public void TearDown()
        {
            permutations.VerifyAll();
        }

        [Test]
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
