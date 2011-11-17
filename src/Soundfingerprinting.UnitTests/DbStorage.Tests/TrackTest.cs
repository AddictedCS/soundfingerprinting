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
    ///  This is a test class for TrackTest and is intended
    ///  to contain all TrackTest Unit Tests
    ///</summary>
    [TestClass]
    public class TrackTest : BaseTest
    {
        ///<summary>
        ///  Gets or sets the test context which provides
        ///  information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        ///<summary>
        ///  A test for Track Constructor
        ///</summary>
        [TestMethod]
        public void TrackConstructorTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            Track target = new Track();
            Assert.IsNotNull(target.Id);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for AlbumId
        ///</summary>
        [TestMethod]
        public void AlbumIdTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            Track target = new Track();
            Int32 expected = 0;
            target.AlbumId = expected;
            Int32 actual = target.AlbumId;
            Assert.AreEqual(expected, actual);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for Track Constructor
        ///</summary>
        [TestMethod]
        public void TrackConstructorTest1()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            Int32 id = 0;
            string artist = name;
            string title = name;
            Int32 albumId = 0;
            Track target = new Track(id, artist, title, albumId);
            Assert.AreEqual(id, target.Id);
            Assert.AreEqual(artist, target.Artist);
            Assert.AreEqual(title, target.Title);
            Assert.AreEqual(albumId, target.AlbumId);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for Track Constructor
        ///</summary>
        [TestMethod]
        public void TrackConstructorTest2()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            Int32 id = 0;
            string artist = name;
            string title = name;
            Int32 albumId = 0;
            const int trackLength = 0;
            Track target = new Track(id, artist, title, albumId, trackLength);
            Assert.AreEqual(id, target.Id);
            Assert.AreEqual(artist, target.Artist);
            Assert.AreEqual(title, target.Title);
            Assert.AreEqual(albumId, target.AlbumId);
            Assert.AreEqual(trackLength, target.TrackLength);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for Artist
        ///</summary>
        [TestMethod]
        public void ArtistTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            Track target = new Track();
            string expected = name;
            target.Artist = expected;
            string actual = target.Artist;
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

            Track target = new Track();
            Int32 expected = 0;
            target.Id = expected;
            Int32 actual = target.Id;
            Assert.AreEqual(expected, actual);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for Title
        ///</summary>
        [TestMethod]
        public void TitleTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            Track target = new Track();
            string expected = name;
            target.Title = expected;
            string actual = target.Title;
            Assert.AreEqual(expected, actual);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for TrackLength
        ///</summary>
        [TestMethod]
        public void TrackLengthTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            Track target = new Track();
            const int expected = 0;
            target.TrackLength = expected;
            int actual = target.TrackLength;
            Assert.AreEqual(expected, actual);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }
    }
}