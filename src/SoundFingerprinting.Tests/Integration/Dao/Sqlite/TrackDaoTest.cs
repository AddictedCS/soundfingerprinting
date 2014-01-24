using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider.SQLite;
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

        private string _sqliteFile;

        [SetUp]
        [TestInitialize]
        public override void SetUp()
        {
            _sqliteFile = Guid.NewGuid() + ".sqlite";

            SQLiteConnection.CreateFile(_sqliteFile);
            using (var db = new DataConnection(new SQLiteDataProvider(), string.Format("Data Source={0}", _sqliteFile)))
            {
                db.CreateTable<Track>();
            }
            
            base.SetUp();
        }

        [TearDown]
        [TestCleanup]
        public override void TearDown()
        {
            if (File.Exists(_sqliteFile))
                File.Delete(_sqliteFile);

            base.TearDown();
        }

        public override sealed ITrackDao TrackDao { get; set; }

        public override sealed ISubFingerprintDao SubFingerprintDao { get; set; }

        public override sealed IHashBinDao HashBinDao { get; set; }
    }
}
