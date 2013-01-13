namespace Soundfingerprinting.UnitTests.DbStorage.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Soundfingerprinting.Dao.Entities;
    using Soundfingerprinting.DbStorage.Entities;

    [TestClass]
    public class HashBinTest : BaseTest
    {
        [TestMethod]
        public void HashBinConstructorTest()
        {
            HashBin target = new HashBin();
            Assert.IsNotNull(target.Id);
        }

        [TestMethod]
        public void HashBinConstructorTest1()
        {
            const int Id = -1;
            const long HashBin = 20;
            const int HashTable = 30;
            const int TrackId = 0;
            HashBin target = new HashBin(Id, HashBin, HashTable, TrackId);
            Assert.AreEqual(Id, target.Id);
            Assert.AreEqual(HashBin, target.Bin);
            Assert.AreEqual(HashTable, target.HashTable);
            Assert.AreEqual(TrackId, target.TrackId);
        }

        [TestMethod]
        public void HashTableTest()
        {
            HashBin target = new HashBin();
            const int Expected = 0;
            target.HashTable = Expected;
            int actual = target.HashTable;
            Assert.AreEqual(Expected, actual);
        }

        [TestMethod]
        public void HashbinTest()
        {
            HashBin target = new HashBin();
            const long Expected = 60;
            target.Bin = Expected;
            long actual = target.Bin;
            Assert.AreEqual(Expected, actual);
        }
    }
}