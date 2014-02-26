namespace SoundFingerprinting.Tests.Integration
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Transactions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.InMemory;

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

        protected void AssertHashDatasAreTheSame(IList<HashData> firstHashDatas, IList<HashData> secondHashDatas)
        {
            Assert.AreEqual(firstHashDatas.Count, secondHashDatas.Count);
         
            // hashes are not ordered the same way as parallel computation is involved
            firstHashDatas = SortHashesByFirstValueOfHashBin(firstHashDatas);
            secondHashDatas = SortHashesByFirstValueOfHashBin(secondHashDatas);

            for (int i = 0; i < firstHashDatas.Count; i++)
            {
                for (int j = 0; j < firstHashDatas[i].SubFingerprint.Length; j++)
                {
                    Assert.AreEqual(firstHashDatas[i].SubFingerprint[j], secondHashDatas[i].SubFingerprint[j]);
                }

                for (int j = 0; j < firstHashDatas[i].HashBins.Length; j++)
                {
                    Assert.AreEqual(firstHashDatas[i].HashBins[j], secondHashDatas[i].HashBins[j]);
                }
            }
        }

        protected void AssertModelReferenceIsInitialized(IModelReference modelReference)
        {
            Assert.IsNotNull(modelReference);
            Assert.IsFalse(modelReference.GetHashCode() == 0);
        }

        private List<HashData> SortHashesByFirstValueOfHashBin(IEnumerable<HashData> hashDatasFromFile)
        {
            return hashDatasFromFile.OrderBy(hashData => hashData.HashBins[0]).ToList();
        }
    }
}
