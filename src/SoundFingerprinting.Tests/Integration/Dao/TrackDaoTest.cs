namespace SoundFingerprinting.Tests.Integration.Dao
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Dao;
    using SoundFingerprinting.Dao.Internal;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Strides;

    [TestClass]
    public class TrackDaoTest : AbstractIntegrationTest
    {
        private readonly TrackDao trackDao;

        private readonly IFingerprintCommandBuilder fingerprintCommandBuilder;

        private readonly ITagService tagService;

        private readonly SubFingerprintDao subFingerprintDao;

        private readonly HashBinDao hashBinDao;

        public TrackDaoTest()
        {
            fingerprintCommandBuilder = DependencyResolver.Current.Get<IFingerprintCommandBuilder>();
            tagService = DependencyResolver.Current.Get<ITagService>();
            trackDao = new TrackDao(
                DependencyResolver.Current.Get<IDatabaseProviderFactory>(),
                DependencyResolver.Current.Get<IModelBinderFactory>());
            subFingerprintDao = new SubFingerprintDao(
                DependencyResolver.Current.Get<IDatabaseProviderFactory>(),
                DependencyResolver.Current.Get<IModelBinderFactory>());
            hashBinDao = new HashBinDao(
                DependencyResolver.Current.Get<IDatabaseProviderFactory>(),
                DependencyResolver.Current.Get<IModelBinderFactory>());
        }

        [TestMethod]
        public void InsertTrackTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            var track = GetTrack(name);

            trackDao.Insert(track);

            Assert.IsFalse(track.TrackReference.HashCode == 0);
            Assert.IsTrue(track.TrackReference is RDBMSTrackReference);
            Assert.IsFalse(((RDBMSTrackReference)track.TrackReference).Id == 0);
        }

        [TestMethod]
        public void ReadTrackTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            var track = GetTrack(name);
            int trackId = trackDao.Insert(track);

            var listOfTracks = trackDao.ReadAll();

            Assert.AreEqual(1, listOfTracks.Count);
            TrackData actualTrack = trackDao.ReadById(trackId);
            AssertTracksAreEqual(track, actualTrack);
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
                trackDao.Insert(track);
            }

            foreach (var track in tracks)
            {
                Assert.IsNotNull(track.TrackReference);
                Assert.IsFalse(track.TrackReference.HashCode == 0);
            }

            var actualTracks = trackDao.ReadAll();
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
            trackDao.Insert(track);

            IList<TrackData> tracks = trackDao.ReadTrackByArtistAndTitleName(track.Artist, track.Title);

            Assert.IsNotNull(tracks);
            Assert.IsTrue(tracks.Count == 1);
            AssertTracksAreEqual(track, tracks[0]);
        }

        [TestMethod]
        public void ReadTrackByISRCTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            TrackData expectedTrack = GetTrack(name);
            trackDao.Insert(expectedTrack);

            TrackData actualTrack = trackDao.ReadTrackByISRC(expectedTrack.ISRC);

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
                trackIds.Add(trackDao.Insert(track));
            }

            var allTracks = trackDao.ReadAll();

            Assert.IsTrue(allTracks.Count > 0);
            foreach (var trackId in trackIds)
            {
                trackDao.DeleteTrack(trackId);
            }

            Assert.IsTrue(trackDao.ReadAll().Count == 0);
        }

        [TestMethod]
        public void DeleteOneTrackTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            TrackData track = GetTrack(name);
            int trackId = trackDao.Insert(track);

            trackDao.DeleteTrack(trackId);

            Assert.IsNull(trackDao.ReadById(trackId));
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
            int trackId = trackDao.Insert(track);
            var hashData = fingerprintCommandBuilder
                .BuildFingerprintCommand()
                .From(PathToMp3, SecondsToProcess, StartAtSecond)
                .WithCustomAlgorithmConfiguration(config =>
                    {
                        config.Stride = new IncrementalStaticStride(StaticStride, config.SamplesPerFingerprint);
                    })
                .Hash()
                .Result;

            List<long> subFingerprintIds = new List<long>();
            foreach (var hash in hashData)
            {
                long subFingerprintId = subFingerprintDao.Insert(hash.SubFingerprint, trackId);
                hashBinDao.Insert(hash.HashBins, subFingerprintId);
                subFingerprintIds.Add(subFingerprintId);
            }

            var actualTrack = trackDao.ReadTrackByISRC(tagInfo.ISRC);
            Assert.IsNotNull(actualTrack);
            AssertTracksAreEqual(track, actualTrack);

            // Act
            int modifiedRows = trackDao.DeleteTrack(trackId);
            Assert.IsNull(trackDao.ReadTrackByISRC(tagInfo.ISRC));
            foreach (var id in subFingerprintIds)
            {
                Assert.IsTrue(id != 0);
                Assert.IsNull(subFingerprintDao.ReadById(id));
            }

            for (int i = 1; i <= 25; i++)
            {
                Assert.IsTrue(hashBinDao.ReadHashBinsByHashTable(i).Count == 0);
            }

            Assert.AreEqual(1 + hashData.Count + (25 * hashData.Count), modifiedRows);
        }

        [TestMethod]
        public void DeleteNonExistentTrackTest()
        {
            int actual = trackDao.DeleteTrack(1234);
            Assert.AreEqual(0, actual);
        }

        [TestMethod]
        public void ReadTrackByRandomArtistAndTitleNameTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            string artist = name;
            string title = name;
            var tracks = trackDao.ReadTrackByArtistAndTitleName(artist, title);
            Assert.IsNotNull(tracks);
            Assert.IsTrue(tracks.Count == 0);
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
            return new TrackData(isrc, artist, title, albumName, 1986, 360);
        }

        private void AssertTracksAreEqual(TrackData expectedTrack, TrackData actualTrack)
        {
            Assert.AreEqual(expectedTrack.TrackReference.HashCode, actualTrack.TrackReference.HashCode);
            Assert.AreEqual(expectedTrack.Album, actualTrack.Album);
            Assert.AreEqual(expectedTrack.Artist, actualTrack.Artist);
            Assert.AreEqual(expectedTrack.Title, actualTrack.Title);
            Assert.AreEqual(expectedTrack.TrackLengthSec, actualTrack.TrackLengthSec);
            Assert.AreEqual(expectedTrack.ISRC, actualTrack.ISRC);
        }
    }
}
