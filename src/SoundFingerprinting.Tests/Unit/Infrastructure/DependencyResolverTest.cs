namespace SoundFingerprinting.Tests.Unit.Infrastructure
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Builder;

    [TestClass]
    public class DependencyResolverTest
    {
        [TestMethod]
        public void ResolveDefaultInterfacesForFingerprintCommandTest()
        {
            IFingerprintCommandBuilder fingerprintCommandBuilder = new FingerprintCommandBuilder();
            Assert.IsNotNull(fingerprintCommandBuilder);
        }

        [TestMethod]
        public void ResolverDefaultInterfacesForQueryCommandTest()
        {
            IQueryCommandBuilder queryCommandBuilder = new QueryCommandBuilder();
            Assert.IsNotNull(queryCommandBuilder);
        }
    }
}
