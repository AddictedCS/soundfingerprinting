namespace SoundFingerprinting.Tests.Integration
{
    using System.Transactions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Dao.RAM;
    using SoundFingerprinting.Infrastructure;

    [DeploymentItem(@"floatsamples.bin")]
    [DeploymentItem(@"Kryptonite.mp3")]
    [TestClass]
    public abstract class AbstractIntegrationTest : AbstractTest
    {
        private TransactionScope transactionPerTestScope;

        [TestInitialize]
        public virtual void SetUp()
        {
            transactionPerTestScope = new TransactionScope();
        }

        [TestCleanup]
        public virtual void TearDown()
        {
            transactionPerTestScope.Dispose();
            DependencyResolver.Current.Get<IRAMStorage>().Reset(new DefaultFingerprintConfiguration().NumberOfLSHTables);
        }
    }
}
