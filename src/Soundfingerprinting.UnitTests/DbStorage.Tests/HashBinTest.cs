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
    ///  This is a test class for HashBinTest and is intended
    ///  to contain all HashBinTest Unit Tests
    ///</summary>
    [TestClass]
    public class HashBinTest : BaseTest
    {
        ///<summary>
        ///  Gets or sets the test context which provides
        ///  information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        ///<summary>
        ///  A test for HashBin Constructor
        ///</summary>
        [TestMethod]
        public void HashBinConstructorTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            HashBin target = new HashBin();
            Assert.IsNotNull(target.Id);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for HashBin Constructor
        ///</summary>
        [TestMethod]
        public void HashBinConstructorTest1()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            const int id = -1;
            const long hashBin = 20;
            const int hashTable = 30;
            const int trackId = 0;
            HashBin target = new HashBin(id, hashBin, hashTable, trackId);
            Assert.AreEqual(id, target.Id);
            Assert.AreEqual(hashBin, target.Hashbin);
            Assert.AreEqual(hashTable, target.HashTable);
            Assert.AreEqual(trackId, target.TrackId);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for HashTable
        ///</summary>
        [TestMethod]
        public void HashTableTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            HashBin target = new HashBin();
            const int expected = 0;
            target.HashTable = expected;
            int actual = target.HashTable;
            Assert.AreEqual(expected, actual);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for Hash bin
        ///</summary>
        [TestMethod]
        public void HashbinTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            HashBin target = new HashBin();
            const long expected = 60;
            target.Hashbin = expected;
            long actual = target.Hashbin;
            Assert.AreEqual(expected, actual);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for Id
        ///</summary>
        [TestMethod]
        public void IdTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            HashBin target = new HashBin();
            const int expected = 0;
            target.Id = expected;
            Int32 actual = target.Id;
            Assert.AreEqual(expected, actual);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for TrackId
        ///</summary>
        [TestMethod]
        public void TrackIdTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            HashBin target = new HashBin();
            const int expected = -1;
            target.TrackId = expected;
            Int32 actual = target.TrackId;
            Assert.AreEqual(expected, actual);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }
    }
}