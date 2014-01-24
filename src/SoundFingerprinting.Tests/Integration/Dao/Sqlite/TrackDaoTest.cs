using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using SoundFingerprinting.Dao;
using SoundFingerprinting.Dao.Sqlite;

namespace SoundFingerprinting.Tests.Integration.Dao.Sqlite
{
    [TestClass]
    public class TrackDaoTest : AbstractTrackDaoTest
    {
        public TrackDaoTest()
        {
            TrackDao = new TrackDao("SQLite");
            //SubFingerprintDao = new SubFingerprintDao();
            //HashBinDao = new HashBinDao();
        }

        [SetUp]
        public void Setup()
        {
            if (File.Exists("TestData.sqlite"))
                File.Delete("TestData.sqlite");

            SQLiteConnection.CreateFile("TestData.sqlite");
            using (var db = new DataConnection("SQLite"))
            {
                db.CreateTable<Track>();
            }
            
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists("TestData.sqlite"))
                File.Delete("TestData.sqlite");
        }

        public override sealed ITrackDao TrackDao { get; set; }

        public override sealed ISubFingerprintDao SubFingerprintDao { get; set; }

        public override sealed IHashBinDao HashBinDao { get; set; }
    }
}
