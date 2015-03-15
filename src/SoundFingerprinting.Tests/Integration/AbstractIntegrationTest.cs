namespace SoundFingerprinting.Tests.Integration
{
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.Data;

    [DeploymentItem(@"TestEnvironment\floatsamples.bin")]
    [DeploymentItem(@"TestEnvironment\Kryptonite.mp3")]
    [TestClass]
    public abstract class AbstractIntegrationTest : AbstractTest
    {
        protected void AssertHashDatasAreTheSame(IList<HashedFingerprint> firstHashDatas, IList<HashedFingerprint> secondHashDatas)
        {
            Assert.AreEqual(firstHashDatas.Count, secondHashDatas.Count);
         
            // hashes are not ordered as parallel computation is involved
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

                Assert.AreEqual(firstHashDatas[i].SequenceNumber, secondHashDatas[i].SequenceNumber);
                Assert.AreEqual(firstHashDatas[i].Timestamp, secondHashDatas[i].Timestamp, Epsilon);
            }
        }

        protected void AssertModelReferenceIsInitialized(IModelReference modelReference)
        {
            Assert.IsNotNull(modelReference);
            Assert.IsTrue(modelReference.GetHashCode() != 0);
        }

        private List<HashedFingerprint> SortHashesByFirstValueOfHashBin(IEnumerable<HashedFingerprint> hashDatasFromFile)
        {
            return hashDatasFromFile.OrderBy(hashData => hashData.HashBins[0]).ToList();
        }
    }
}
