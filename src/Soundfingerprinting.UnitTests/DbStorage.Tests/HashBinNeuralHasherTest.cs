namespace Soundfingerprinting.UnitTests.DbStorage.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Soundfingerprinting.DbStorage.Entities;

    [TestClass]
    public class HashBinNeuralHasherTest : BaseTest
    {
        [TestMethod]
        public void HashBinNeuralHasherConstructorTest()
        {
            const int Id = 0;
            const long HashBin = 0;
            const int HashTable = 0;
            const int TrackId = -1;
            HashBinNeuralHasher target = new HashBinNeuralHasher(Id, HashBin, HashTable, TrackId);
            Assert.AreEqual(Id, target.Id);
            Assert.AreEqual(HashBin, target.Hashbin);
            Assert.AreEqual(HashTable, target.HashTable);
            Assert.AreEqual(TrackId, target.TrackId);
        }
    }
}