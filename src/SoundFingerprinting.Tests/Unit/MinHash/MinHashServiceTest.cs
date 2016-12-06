namespace SoundFingerprinting.Tests.Unit.MinHash
{
    using Moq;

    using NUnit.Framework;

    using SoundFingerprinting.MinHash;

    [TestFixture]
    public class MinHashServiceTest : AbstractTest
    {
        private MinHashService minHashService;

        private Mock<IPermutations> permutations;

        [SetUp]
        public void SetUp()
        {
            permutations = new Mock<IPermutations>(MockBehavior.Strict);
            minHashService = new MinHashService(permutations.Object);
        }

        [TearDown]
        public void TearDown()
        {
            permutations.VerifyAll();
        }

        [Test]
        public void DependencyResolverTest()
        {
            var instance = new MinHashService();

            Assert.IsNotNull(instance);
        }

        [Test]
        public void PermutationsCountTest()
        {
            int[][] perms = new[] { new int[] { }, new int[] { }, new int[] { } };
            permutations.Setup(perm => perm.GetPermutations()).Returns(perms);

            int count = minHashService.PermutationsCount;

            Assert.AreEqual(3, count);
        }

        [Test]
        public void ComputeHashTest()
        {
            int[][] perms = new[] { new[] { 1, 4, 8 }, new[] { 2, 3, 8 }, new[] { 7, 9, 0 } };
            permutations.Setup(perm => perm.GetPermutations()).Returns(perms);

            bool[] fingerprint = new[] { false, false, true, false, true, false, true, false, false, false };

            byte[] hashed = minHashService.Hash(fingerprint);

            Assert.AreEqual(1, hashed[0]);
            Assert.AreEqual(0, hashed[1]);
            Assert.AreEqual(255, hashed[2]);
        }
    }
}
