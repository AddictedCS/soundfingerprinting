namespace SoundFingerprinting.Tests.Unit.Infrastructure
{
    using NUnit.Framework;

    using SoundFingerprinting.Builder;

    [TestFixture]
    public class DependencyResolverTest
    {
        [Test]
        public void ResolveDefaultInterfacesForFingerprintCommandTest()
        {
            var fingerprintCommandBuilder = new FingerprintCommandBuilder();
            Assert.IsNotNull(fingerprintCommandBuilder);
        }

        [Test]
        public void ResolverDefaultInterfacesForQueryCommandTest()
        {
            var queryCommandBuilder = new QueryCommandBuilder();
            Assert.IsNotNull(queryCommandBuilder);
        }
    }
}
