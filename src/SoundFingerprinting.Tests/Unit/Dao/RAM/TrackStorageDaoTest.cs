namespace SoundFingerprinting.Tests.Unit.Dao.RAM
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Dao.RAM;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;

    [TestClass]
    public class TrackStorageDaoTest : AbstractTest
    {
        private TrackDao trackDao;

        [TestInitialize]
        public void SetUp()
        {
            trackDao = new TrackDao(DependencyResolver.Current.Get<IRAMStorage>());
        }

        [TestCleanup]
        public void TearDown()
        {
            DependencyResolver.Current.Get<IRAMStorage>().Reset(new DefaultFingerprintConfiguration().NumberOfLSHTables);
        }

        [TestMethod]
        public void DependencyResolverTest()
        {
            var instance = new TrackDao();

            Assert.IsNotNull(instance);
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
                        int id = trackDao.Insert(new TrackData("isrc", "artist", "title", "album", 2012, 200));
                        Assert.IsFalse(ids.Contains(id));
                        ids.Add(id);
                    });
        }

        [TestMethod]
        public void ReadByISRCTest()
        {
            const string ISRC = "isrc";
            TrackData expected = new TrackData(ISRC, "artist", "title", "album", 2012, 200);
            trackDao.Insert(expected);

            var actual = trackDao.ReadTrackByISRC(ISRC);

            Assert.IsNotNull(actual);
            AssertTracksAreEqual(expected, actual);
        }

        [TestMethod]
        public void ReadByNonExistentISRCTest()
        {
            var track = trackDao.ReadTrackByISRC("isrc");
            Assert.IsNull(track);
        }

        [TestMethod]
        public void ReadAllTest()
        {
            TrackData firstTrack = new TrackData("first isrc", "artist", "title", "album", 2012, 200);
            TrackData secondTrack = new TrackData("second isrc", "artist", "title", "album", 2012, 200);
            trackDao.Insert(firstTrack);
            trackDao.Insert(secondTrack);

            IList<TrackData> tracks = trackDao.ReadAll();

            Assert.IsTrue(tracks.Count == 2);
            Assert.IsTrue(tracks.Any(track => track.ISRC == "first isrc"));
            Assert.IsTrue(tracks.Any(track => track.ISRC == "second isrc"));
        }

        [TestMethod]
        public void ReadByNonExistentArtistAndTitleTest()
        {
            var tracks = trackDao.ReadTrackByArtistAndTitleName("artist", "title");

            Assert.IsTrue(tracks.Count == 0);
        }

        [TestMethod]
        public void ReadByArtistAndTitleTest()
        {
            var track = new TrackData("isrc", "artist", "title", "album", 2012, 200);
            trackDao.Insert(track);

            var tracks = trackDao.ReadTrackByArtistAndTitleName("artist", "title");

            Assert.IsTrue(tracks.Count == 1);
            AssertTracksAreEqual(track, tracks[0]);
        }

        [TestMethod]
        public void ReadByRandomIdTest()
        {
            var track = trackDao.ReadById(-1);

            Assert.IsNull(track);
        }

        [TestMethod]
        public void ReadByIdTest()
        {
            var track = new TrackData("isrc", "artist", "title", "album", 2012, 200);
            int id = trackDao.Insert(track);

            var actualTrack = trackDao.ReadById(id);

            AssertTracksAreEqual(track, actualTrack);
        }
    }
}
