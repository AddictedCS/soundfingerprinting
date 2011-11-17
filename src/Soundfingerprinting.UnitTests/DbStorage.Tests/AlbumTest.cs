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
    /// <summary>
    ///   Class which make a complete test of Album class
    /// </summary>
    [TestClass]
    public class AlbumTest : BaseTest
    {
        /// <summary>
        ///   Constructor test
        /// </summary>
        [TestMethod]
        public void AlbumConstructorTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            Album album = new Album();
            Assert.IsNotNull(album.Id);
            Assert.AreEqual(MIN_YEAR, album.ReleaseYear);
            Assert.AreEqual("Unknown", album.Name);

            const int id = 0;
            string albumname = name;
            album = new Album(id, albumname);

            Assert.AreEqual(id, album.Id);
            Assert.AreEqual(albumname, album.Name);
            Assert.AreEqual(MIN_YEAR, album.ReleaseYear);

            const int release = 2010;
            album = new Album(id, albumname, release);

            Assert.AreEqual(id, album.Id);
            Assert.AreEqual(albumname, album.Name);
            Assert.AreEqual(release, album.ReleaseYear);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for Album Constructor
        ///</summary>
        [TestMethod]
        public void AlbumConstructorTest0()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            const Int32 id = 0;
            string albumname = name;
            const int releaseYear = 2000;
            Album target = new Album(id, albumname, releaseYear);

            Assert.IsNotNull(target);

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

            Album target = new Album();
            const int expected = 0;
            target.Id = expected;
            Int32 actual = target.Id;
            Assert.AreEqual(expected, actual);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for Name
        ///</summary>
        [TestMethod]
        public void NameTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            Album target = new Album();
            string expected = name;
            target.Name = expected;
            string actual = target.Name;
            Assert.AreEqual(expected, actual);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for ReleaseYear
        ///</summary>
        [TestMethod]
        public void ReleaseYearTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            Album target = new Album();
            const int expected = 2000;
            target.ReleaseYear = expected;
            int actual = target.ReleaseYear;
            Assert.AreEqual(expected, actual);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }
    }
}