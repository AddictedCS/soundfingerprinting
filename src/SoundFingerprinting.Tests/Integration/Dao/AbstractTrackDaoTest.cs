namespace SoundFingerprinting.Tests.Integration.Dao
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Dao;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Strides;

    [TestClass]
    public abstract class AbstractTrackDaoTest : AbstractIntegrationTest
    {
        private IFingerprintCommandBuilder fingerprintCommandBuilder;

        private ITagService tagService;

        public abstract ITrackDao TrackDao { get; set; }

        public abstract ISubFingerprintDao SubFingerprintDao { get; set; }

        public abstract IHashBinDao HashBinDao { get; set; }

        [TestInitialize]
        public override void SetUp()
        {
            base.SetUp();

            fingerprintCommandBuilder = new FingerprintCommandBuilder();
            tagService = DependencyResolver.Current.Get<ITagService>();
        }

        [TestMethod]
        public void InsertTrackTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            var track = GetTrack(name);

            TrackDao.Insert(track);

            Assert.IsFalse(track.TrackReference.HashCode == 0);
            Assert.IsTrue(track.TrackReference is ModelReference<int>);
            Assert.IsFalse(((ModelReference<int>)track.TrackReference).Id == 0);
        }

        [TestMethod]
        public void MultipleInsertTest()
        {
            ConcurrentBag<int> ids = new ConcurrentBag<int>();
            for (int i = 0; i < 1000; i++)
            {
                int id = TrackDao.Insert(new TrackData("isrc", "artist", "title", "album", 2012, 200)
                    {
                        GroupId = "group-id"
                    });
                Assert.IsFalse(ids.Contains(id));
                ids.Add(id);
            }
        }

        [TestMethod]
        public void ReadAllTracksTest()
        {
            TrackData firstTrack = new TrackData("first isrc", "artist", "title", "album", 2012, 200);
            TrackData secondTrack = new TrackData("second isrc", "artist", "title", "album", 2012, 200);
            TrackDao.Insert(firstTrack);
            TrackDao.Insert(secondTrack);

            IList<TrackData> tracks = TrackDao.ReadAll();

            Assert.IsTrue(tracks.Count == 2);
            Assert.IsTrue(tracks.Any(track => track.ISRC == "first isrc"));
            Assert.IsTrue(tracks.Any(track => track.ISRC == "second isrc"));
        }

        [TestMethod]
        public void ReadByIdTest()
        {
            var track = new TrackData("isrc", "artist", "title", "album", 2012, 200)
                {
                    GroupId = "group-id"
                };
            int id = TrackDao.Insert(track);

            var actualTrack = TrackDao.ReadById(id);

            AssertTracksAreEqual(track, actualTrack);
        }

        [TestMethod]
        public void ReadByNonExistentIdTest()
        {
            var track = TrackDao.ReadById(-1);

            Assert.IsNull(track);
        }

        [TestMethod]
        public void InsertMultipleTrackAtOnceTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            List<TrackData> tracks = new List<TrackData>();
            const int TrackCount = 100;
            for (int i = 0; i < TrackCount; i++)
            {
                var track = GetTrack(name);
                tracks.Add(track);
                TrackDao.Insert(track);
            }

            foreach (var track in tracks)
            {
                Assert.IsNotNull(track.TrackReference);
                Assert.IsFalse(track.TrackReference.HashCode == 0);
            }

            var actualTracks = TrackDao.ReadAll();
            Assert.AreEqual(tracks.Count, actualTracks.Count);
            for (int i = 0; i < actualTracks.Count; i++)
            {
                AssertTracksAreEqual(
                    tracks[i],
                    actualTracks.First(track => track.TrackReference.HashCode == tracks[i].TrackReference.HashCode));
            }
        }

        [TestMethod]
        public void ReadTrackByArtistAndTitleTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            TrackData track = GetTrack(name);
            TrackDao.Insert(track);

            IList<TrackData> tracks = TrackDao.ReadTrackByArtistAndTitleName(track.Artist, track.Title);

            Assert.IsNotNull(tracks);
            Assert.IsTrue(tracks.Count == 1);
            AssertTracksAreEqual(track, tracks[0]);
        }

        [TestMethod]
        public void ReadByNonExistentArtistAndTitleTest()
        {
            var tracks = TrackDao.ReadTrackByArtistAndTitleName("artist", "title");

            Assert.IsTrue(tracks.Count == 0);
        }

        [TestMethod]
        public void ReadTrackByISRCTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            TrackData expectedTrack = GetTrack(name);
            TrackDao.Insert(expectedTrack);

            TrackData actualTrack = TrackDao.ReadTrackByISRC(expectedTrack.ISRC);

            AssertTracksAreEqual(expectedTrack, actualTrack);
        }

        [TestMethod]
        public void DeleteCollectionOfTracksTest()
        {
            const int NumberOfTracks = 10;
            IEnumerable<TrackData> tracks = GetRandomListOfTracks(NumberOfTracks);
            List<int> trackIds = new List<int>();
            foreach (var track in tracks)
            {
                trackIds.Add(TrackDao.Insert(track));
            }

            var allTracks = TrackDao.ReadAll();

            Assert.IsTrue(allTracks.Count > 0);
            foreach (var trackId in trackIds)
            {
                TrackDao.DeleteTrack(trackId);
            }

            Assert.IsTrue(TrackDao.ReadAll().Count == 0);
        }

        [TestMethod]
        public void DeleteOneTrackTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            TrackData track = GetTrack(name);
            int trackId = TrackDao.Insert(track);

            TrackDao.DeleteTrack(trackId);

            Assert.IsNull(TrackDao.ReadById(trackId));
        }

        [TestMethod]
        public void DeleteHashBinsAndSubfingerprintsOnTrackDelete()
        {
            const int StaticStride = 5115;
            const int SecondsToProcess = 20;
            const int StartAtSecond = 30;
            TagInfo tagInfo = tagService.GetTagInfo(PathToMp3);
            int releaseYear = tagInfo.Year;
            TrackData track = new TrackData(tagInfo.ISRC, tagInfo.Artist, tagInfo.Title, tagInfo.Album, releaseYear, (int)tagInfo.Duration);
            int trackId = TrackDao.Insert(track);
            var hashData = fingerprintCommandBuilder
                .BuildFingerprintCommand()
                .From(PathToMp3, SecondsToProcess, StartAtSecond)
                .WithFingerprintConfig(config =>
                    {
                        config.Stride = new IncrementalStaticStride(StaticStride, config.SamplesPerFingerprint);
                    })
                .Hash()
                .Result;

            List<long> subFingerprintIds = new List<long>();
            foreach (var hash in hashData)
            {
                long subFingerprintId = SubFingerprintDao.Insert(hash.SubFingerprint, trackId);
                HashBinDao.Insert(hash.HashBins, subFingerprintId);
                subFingerprintIds.Add(subFingerprintId);
            }

            var actualTrack = TrackDao.ReadTrackByISRC(tagInfo.ISRC);
            Assert.IsNotNull(actualTrack);
            AssertTracksAreEqual(track, actualTrack);

            // Act
            int modifiedRows = TrackDao.DeleteTrack(trackId);

            Assert.IsNull(TrackDao.ReadTrackByISRC(tagInfo.ISRC));
            foreach (var id in subFingerprintIds)
            {
                Assert.IsTrue(id != 0);
                Assert.IsNull(SubFingerprintDao.ReadById(id));
            }

            for (int i = 1; i <= 25; i++)
            {
                Assert.IsTrue(HashBinDao.ReadHashBinsByHashTable(i).Count == 0);
            }

            Assert.AreEqual(1 + hashData.Count + (25 * hashData.Count), modifiedRows);
        }

        [TestMethod]
        public void DeleteNonExistentTrackTest()
        {
            int actual = TrackDao.DeleteTrack(1234);
            Assert.AreEqual(0, actual);
        }

        [TestMethod]
        public void InserTrackShouldAcceptEmptyEntriesCodes()
        {
            TrackData track = new TrackData(string.Empty, string.Empty, string.Empty, string.Empty, 1986, 200);
           
            var trackReference = TrackDao.Insert(track);

            var actualTrack = TrackDao.ReadById(trackReference.GetHashCode());

            Assert.IsNotNull(trackReference);
            AssertTracksAreEqual(track, actualTrack);
        }

        private IEnumerable<TrackData> GetRandomListOfTracks(int count)
        {
            var tracks = new List<TrackData>();
            for (int i = 0; i < count; i++)
            {
                TrackData track = GetTrack(MethodBase.GetCurrentMethod().Name);
                track.ISRC += i;
                tracks.Add(track);
            }

            return tracks;
        }

        private TrackData GetTrack(string methodName)
        {
            string isrc = Guid.NewGuid().ToString();
            string albumName = methodName + "album name";
            string artist = methodName + "artist";
            string title = methodName + "title";
            return new TrackData(isrc, artist, title, albumName, 1986, 360) { GroupId = Guid.NewGuid().ToString().Substring(0, 20) };
        }
    }
}
