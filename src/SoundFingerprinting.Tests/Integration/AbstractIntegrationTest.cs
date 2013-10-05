namespace SoundFingerprinting.Tests.Integration
{
    using System.Transactions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

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
        }
    }
}
