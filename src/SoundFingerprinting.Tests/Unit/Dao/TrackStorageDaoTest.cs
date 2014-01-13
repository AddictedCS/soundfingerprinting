namespace SoundFingerprinting.Tests.Unit.Dao
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Dao.InMemory;
    using SoundFingerprinting.Data;

    [TestClass]
    public class TrackStorageDaoTest : AbstractTest
    {
        private TrackStorageDao trackStorageDao;

        [TestInitialize]
        public void SetUp()
        {
            trackStorageDao = new TrackStorageDao();
        }

        [TestMethod]
        public void InsertTest()
        {
            ConcurrentBag<int> ids = new ConcurrentBag<int>();
            Parallel.For(
                0,
                1000,
                i =>
                    {
                        int id = trackStorageDao.Insert(new TrackData("isrc", "artist", "title", "album", 2012, 200));
                        Assert.IsFalse(ids.Contains(id));
                        ids.Add(id);
                    });
        }

        [TestMethod]
        public void ReadByISRCTest()
        {
            const string ISRC = "isrc";
            TrackData expected = new TrackData(ISRC, "artist", "title", "album", 2012, 200);
            trackStorageDao.Insert(expected);

            var actual = trackStorageDao.ReadByISRC(ISRC);

            Assert.IsNotNull(actual);
            AssertTracksAreEqual(expected, actual);
        }

        [TestMethod]
        public void ReadByNonExistentISRCTest()
        {
            var track = trackStorageDao.ReadByISRC("isrc");
            Assert.IsNull(track);
        }

        [TestMethod]
        public void ReadAllTest()
        {
            TrackData firstTrack = new TrackData("first isrc", "artist", "title", "album", 2012, 200);
            TrackData secondTrack = new TrackData("second isrc", "artist", "title", "album", 2012, 200);
            trackStorageDao.Insert(firstTrack);
            trackStorageDao.Insert(secondTrack);

            IList<TrackData> tracks = trackStorageDao.ReadAll();

            Assert.IsTrue(tracks.Count == 2);
            Assert.IsTrue(tracks.Any(track => track.ISRC == "first isrc"));
            Assert.IsTrue(tracks.Any(track => track.ISRC == "second isrc"));
        }

        [TestMethod]
        public void ReadByNonExistentArtistAndTitleTest()
        {
            var tracks = trackStorageDao.ReadTrackByArtistAndTitleName("artist", "title");

            Assert.IsTrue(tracks.Count == 0);
        }

        [TestMethod]
        public void ReadByArtistAndTitleTest()
        {
            var track = new TrackData("isrc", "artist", "title", "album", 2012, 200);
            trackStorageDao.Insert(track);

            var tracks = trackStorageDao.ReadTrackByArtistAndTitleName("artist", "title");

            Assert.IsTrue(tracks.Count == 1);
            AssertTracksAreEqual(track, tracks[0]);
        }

        [TestMethod]
        public void ReadByRandomIdTest()
        {
            var track = trackStorageDao.ReadById(-1);

            Assert.IsNull(track);
        }

        [TestMethod]
        public void ReadByIdTest()
        {
            var track = new TrackData("isrc", "artist", "title", "album", 2012, 200);
            int id = trackStorageDao.Insert(track);

            var actualTrack = trackStorageDao.ReadById(id);

            AssertTracksAreEqual(track, actualTrack);
        }
    }
}
