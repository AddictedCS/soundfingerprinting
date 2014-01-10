namespace SoundFingerprinting.Tests.Unit.Infrastructure
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Infrastructure;

    [TestClass]
    public class DependencyResolverTest
    {
        [TestMethod]
        public void TestResolveFingerprintUnitBuilder()
        {
            IFingerprintCommandBuilder builder = DependencyResolver.Current.Get<IFingerprintCommandBuilder>();
            Assert.IsNotNull(builder);
            Assert.AreEqual(typeof(FingerprintCommandBuilder), builder.GetType());
        }
    }
}
