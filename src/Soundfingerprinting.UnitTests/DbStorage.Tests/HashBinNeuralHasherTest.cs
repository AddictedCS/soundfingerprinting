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
    ///  This is a test class for HashBinNeuralHasherTest and is intended
    ///  to contain all HashBinNeuralHasherTest Unit Tests
    ///</summary>
    [TestClass]
    public class HashBinNeuralHasherTest : BaseTest
    {
        ///<summary>
        ///  Gets or sets the test context which provides
        ///  information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        ///<summary>
        ///  A test for HashBinNeuralHasher Constructor
        ///</summary>
        [TestMethod]
        public void HashBinNeuralHasherConstructorTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            const int id = 0;
            const long hashBin = 0;
            const int hashTable = 0;
            const int trackId = -1;
            HashBinNeuralHasher target = new HashBinNeuralHasher(id, hashBin, hashTable, trackId);
            Assert.AreEqual(id, target.Id);
            Assert.AreEqual(hashBin, target.Hashbin);
            Assert.AreEqual(hashTable, target.HashTable);
            Assert.AreEqual(trackId, target.TrackId);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }
    }
}