// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Soundfingerprinting.DbStorage.Entities;

namespace Soundfingerprinting.UnitTests.DbStorage.Tests
{
    ///<summary>
    ///  This is a test class for HashBinMinHashTest and is intended
    ///  to contain all HashBinMinHashTest Unit Tests
    ///</summary>
    [TestClass]
    public class HashBinMinHashTest : BaseTest
    {
        ///<summary>
        ///  Gets or sets the test context which provides
        ///  information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        ///<summary>
        ///  A test for HashBinMinHash Constructor
        ///</summary>
        [TestMethod]
        public void HashBinMinHashConstructorTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            const int id = -1;
            const long hashBin = 10;
            const int hashTable = 120;
            const int trackId = -1;
            const int hashedFingerprint = -1;
            HashBinMinHash target = new HashBinMinHash(id, hashBin, hashTable, trackId, hashedFingerprint);
            Assert.AreEqual(id, target.Id);
            Assert.AreEqual(hashBin, target.Hashbin);
            Assert.AreEqual(hashTable, target.HashTable);
            Assert.AreEqual(trackId, target.TrackId);
            Assert.AreEqual(hashedFingerprint, target.HashedFingerprint);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for HashedFingerprint
        ///</summary>
        [TestMethod]
        public void HashedFingerprintTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            const int id = 0;
            const long hashBin = 20;
            const int hashTable = 30;
            const int trackId = 0;
            const int hashedFingerprint = 0;
            HashBinMinHash target = new HashBinMinHash(id, hashBin, hashTable, trackId, hashedFingerprint);
            const int expected = 0;
            target.HashedFingerprint = expected;
            int actual = target.HashedFingerprint;
            Assert.AreEqual(expected, actual);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }
    }
}