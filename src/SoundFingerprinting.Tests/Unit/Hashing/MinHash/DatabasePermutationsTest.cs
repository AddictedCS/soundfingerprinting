namespace SoundFingerprinting.Tests.Unit.Hashing.MinHash
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SoundFingerprinting.Dao;
    using SoundFingerprinting.Hashing.MinHash;

    [TestClass]
    public class DatabasePermutationsTest : AbstractTest
    {
        private DatabasePermutations databasePermutations;

        private Mock<IModelService> modelService;

        [TestInitialize]
        public void SetUp()
        {
            modelService = new Mock<IModelService>(MockBehavior.Strict);

            databasePermutations = new DatabasePermutations(modelService.Object);
        }

        [TestCleanup]
        public void TearDown()
        {
            modelService.VerifyAll();
        }

        [TestMethod]
        public void DependencyResolverTest()
        {
            var instance = new DatabasePermutations();

            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void PermutationsAreReadFromTheDatabaseTest()
        {
            int[][] intPerms = new int[][] { };
            modelService.Setup(service => service.ReadPermutationsForLSHAlgorithm()).Returns(intPerms);

            var actualPermutations = databasePermutations.GetPermutations();

            Assert.AreEqual(intPerms, actualPermutations);
        }
    }
}
