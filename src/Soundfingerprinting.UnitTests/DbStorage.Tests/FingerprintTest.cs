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
    ///  This is a test class for FingerprintTest and is intended
    ///  to contain all FingerprintTest Unit Tests
    ///</summary>
    [TestClass]
    public class FingerprintTest : BaseTest
    {
        ///<summary>
        ///  Gets or sets the test context which provides
        ///  information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        ///<summary>
        ///  A test for Fingerprint Constructor
        ///</summary>
        [TestMethod]
        public void FingerprintConstructorTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            Fingerprint target = new Fingerprint();
            Assert.IsNotNull(target);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for Fingerprint Constructor
        ///</summary>
        [TestMethod]
        public void FingerprintConstructorTest1()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            const int id = 0;
            bool[] fingerprint = new[] {true, false, true, false, true, false, true, false};
            const int trackId = 0;
            const int songOrder = 1;
            const int totalFingerprints = 100;
            Fingerprint target = new Fingerprint(id, fingerprint, trackId, songOrder, totalFingerprints);
            Assert.AreEqual(id, target.Id);
            Assert.AreEqual(fingerprint, target.Signature);
            Assert.AreEqual(trackId, target.TrackId);
            Assert.AreEqual(songOrder, target.SongOrder);
            Assert.AreEqual(totalFingerprints, target.TotalFingerprintsPerTrack);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for Signature
        ///</summary>
        [TestMethod]
        public void FingerprintSByteTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            Fingerprint target = new Fingerprint();
            bool[] expected = new[] {true, false, true, false, true, false, true, false, true, false};
            target.Signature = expected;
            bool[] actual = target.Signature;
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

            Fingerprint target = new Fingerprint();
            const int expected = 0;
            target.Id = expected;
            Int32 actual = target.Id;
            Assert.AreEqual(expected, actual);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for SongOrder
        ///</summary>
        [TestMethod]
        public void SongOrderTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            Fingerprint target = new Fingerprint();
            const int expected = 10;
            target.SongOrder = expected;
            int actual = target.SongOrder;
            Assert.AreEqual(expected, actual);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for TotalFingerprintsPerTrack
        ///</summary>
        [TestMethod]
        public void TotalFingerprintsPerTrackTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            Fingerprint target = new Fingerprint();
            const int expected = 10;
            target.TotalFingerprintsPerTrack = expected;
            int actual = target.TotalFingerprintsPerTrack;
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

            Fingerprint target = new Fingerprint();
            const int expected = 0;
            target.TrackId = expected;
            Int32 actual = target.TrackId;
            Assert.AreEqual(expected, actual);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }
    }
}