namespace Soundfingerprinting.UnitTests.DbStorage.Tests
{
    using System;
    using System.Diagnostics;
    using System.Reflection;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Soundfingerprinting.Dao.Entities;
    using Soundfingerprinting.DbStorage.Entities;

    [TestClass]
    public class HashBinMinHashTest : BaseTest
    {
        [TestMethod]
        public void HashBinMinHashConstructorTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            
            const int Id = -1;
            const long HashBin = 10;
            const int HashTable = 120;
            const int TrackId = -1;
            const int HashedFingerprint = -1;
            HashBinMinHash target = new HashBinMinHash(Id, HashBin, HashTable, TrackId, HashedFingerprint);
            Assert.AreEqual(Id, target.Id);
            Assert.AreEqual(HashBin, target.Bin);
            Assert.AreEqual(HashTable, target.HashTable);
            Assert.AreEqual(TrackId, target.TrackId);
            Assert.AreEqual(HashedFingerprint, target.FingerprintId);
        }

        [TestMethod]
        public void HashedFingerprintTest()
        {
            const int Id = 0;
            const long HashBin = 20;
            const int HashTable = 30;
            const int TrackId = 0;
            const int HashedFingerprint = 0;
            HashBinMinHash target = new HashBinMinHash(Id, HashBin, HashTable, TrackId, HashedFingerprint);
            const int Expected = 0;
            target.FingerprintId = Expected;
            int actual = target.FingerprintId;
            Assert.AreEqual(Expected, actual);
        }
    }
}