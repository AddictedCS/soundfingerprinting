namespace Soundfingerprinting.UnitTests.Infrastructure.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Soundfingerprinting.Fingerprinting.FingerprintUnitBuilder;
    using Soundfingerprinting.Infrastructure;

    [TestClass]
    public class DependencyResolverTest
    {
        [TestMethod]
        public void TestResolveFingerprintUnitBuilder()
        {
            IFingerprintingUnitsBuilder builder = DependencyResolver.Current.Get<IFingerprintingUnitsBuilder>();

            Assert.IsNotNull(builder);
            Assert.AreEqual(typeof(FingerprintingUnitsBuilder), builder.GetType());
        }
    }
}
