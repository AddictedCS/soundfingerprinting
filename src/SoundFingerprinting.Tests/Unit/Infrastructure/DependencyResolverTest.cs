namespace SoundFingerprinting.Tests.Unit.Infrastructure
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Infrastructure;

    [TestClass]
    public class DependencyResolverTest
    {
        [TestMethod]
        public void TestResolveFingerprintUnitBuilder()
        {
            IFingerprintUnitBuilder builder = DependencyResolver.Current.Get<IFingerprintUnitBuilder>();
            Assert.IsNotNull(builder);
            Assert.AreEqual(typeof(FingerprintUnitBuilder), builder.GetType());
        }
    }
}
