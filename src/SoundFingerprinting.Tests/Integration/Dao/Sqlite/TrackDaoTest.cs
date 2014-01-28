namespace SoundFingerprinting.Tests.Integration.Dao.Sqlite
{
    using System;
    using System.Data.SQLite;
    using System.IO;
    using LinqToDB;
    using LinqToDB.Data;
    using LinqToDB.DataProvider.SQLite;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NUnit.Framework;
    using SoundFingerprinting.Dao;
    using SoundFingerprinting.Dao.Sqlite;

    [TestClass]
    public class TrackDaoTest : AbstractTrackDaoTest
    {
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

        public TrackDaoTest()
        {
            TrackDao = new TrackDao("SQLite");
            //SubFingerprintDao = new SubFingerprintDao();
            //HashBinDao = new HashBinDao();
        }

        private string _sqliteFile;

        public override sealed ITrackDao TrackDao { get; set; }

        public override sealed ISubFingerprintDao SubFingerprintDao { get; set; }

        public override sealed IHashBinDao HashBinDao { get; set; }
    }
}