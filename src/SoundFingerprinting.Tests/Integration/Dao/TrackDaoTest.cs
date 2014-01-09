namespace SoundFingerprinting.Tests.Integration.Dao
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Dao;
    using SoundFingerprinting.Dao.Entities;
    using SoundFingerprinting.Dao.Internal;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Strides;

    public class TrackDaoTest : AbstractIntegrationTest
    {
        private readonly TrackDao trackDao;
        
        private readonly IFingerprintCommandBuilder fingerprintCommandBuilder;
        private readonly ITagService tagService;

        private readonly SubFingerprintDao subFingerprintDao;
        private readonly HashBinMinHashDao hashBinMinHashDao;

        public TrackDaoTest()
        {
            fingerprintCommandBuilder = DependencyResolver.Current.Get<IFingerprintCommandBuilder>();
            tagService = DependencyResolver.Current.Get<ITagService>();
            trackDao = new TrackDao(DependencyResolver.Current.Get<IDatabaseProviderFactory>(), DependencyResolver.Current.Get<IModelBinderFactory>());
            subFingerprintDao = new SubFingerprintDao(DependencyResolver.Current.Get<IDatabaseProviderFactory>(), DependencyResolver.Current.Get<IModelBinderFactory>());
            hashBinMinHashDao = new HashBinMinHashDao(DependencyResolver.Current.Get<IDatabaseProviderFactory>(), DependencyResolver.Current.Get<IModelBinderFactory>());
        }

        [TestMethod]
        public void InsertReadTrackTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            var expectedTrack = GetTrack(name);
            trackDao.Insert(expectedTrack);

            Assert.AreNotEqual(int.MinValue, expectedTrack.Id);
            var listOfTracks = trackDao.Read();
            Assert.AreEqual(1, listOfTracks.Count);

            Track actualTrack = trackDao.ReadById(expectedTrack.Id);
            AssertTracksAreEqual(expectedTrack, actualTrack);
        }

        [TestMethod]
        public void InsertMultipleTrackAtOnceTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            List<Track> expectedTracks = new List<Track>();
            const int TrackCount = 10;
            for (int i = 0; i < TrackCount; i++)
            {
                Track expectedTrack = GetTrack(name);
                expectedTracks.Add(expectedTrack);
                trackDao.Insert(expectedTrack);
            }
            
            foreach (Track track in expectedTracks)
            {
                Assert.AreNotEqual(int.MinValue, track.Id);
            }

            var actualTracks = trackDao.Read();
            Assert.AreEqual(expectedTracks.Count, actualTracks.Count);
            for (int i = 0; i < actualTracks.Count; i++)
            {
                AssertTracksAreEqual(
                    expectedTracks[i], actualTracks.First(track => track.Id == expectedTracks[i].Id));
            }
        }

        [TestMethod]
        public void ReadTrackByArtistAndTitle()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            const int FakeId = int.MinValue;
            Track expectedTrack = GetTrack(name);
            trackDao.Insert(expectedTrack);
            Assert.AreNotEqual(FakeId, expectedTrack.Id);

            Track actualTrack = trackDao.ReadTrackByArtistAndTitleName(expectedTrack.Artist, expectedTrack.Title);

            Assert.IsNotNull(actualTrack);
            AssertTracksAreEqual(expectedTrack, actualTrack);
        }

        [TestMethod]
        public void ReadTrackByISRC()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            Track expectedTrack = GetTrack(name);
            trackDao.Insert(expectedTrack);

            Track actualTrack = trackDao.ReadTrackByISRC(expectedTrack.ISRC);

            AssertTracksAreEqual(expectedTrack, actualTrack);
        }

        [TestMethod]
        public void DeleteCollectionOfTracksTest()
        {
            List<Track> tracks = GetRandomListOfTracks(10);
            foreach (var track in tracks)
            {
                trackDao.Insert(track);
            }

            var allTracks = trackDao.Read();

            Assert.IsTrue(allTracks.Count > 0);
            foreach (var track in tracks)
            {
                trackDao.DeleteTrack(track.Id);
            }

            Assert.IsTrue(trackDao.Read().Count == 0);
        }

        [TestMethod]
        public void DeleteOneTrackTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            Track track = GetTrack(name);
            trackDao.Insert(track);
            trackDao.DeleteTrack(track.Id);
            Assert.IsNull(trackDao.ReadById(track.Id));
        }

        [TestMethod]
        public void DeleteHashBinsAndSubfingerprintsOnTrackDelete()
        {
            const int StaticStride = 5115;
            const int SecondsToProcess = 20;
            const int StartAtSecond = 30;
            TagInfo tagInfo = tagService.GetTagInfo(PathToMp3);
            int releaseYear = tagInfo.Year;
            Track track = new Track(tagInfo.ISRC, tagInfo.Artist, tagInfo.Title, tagInfo.Album, releaseYear, (int)tagInfo.Duration);
            trackDao.Insert(track);
            var hashData = fingerprintCommandBuilder
                .BuildFingerprintCommand()
                .From(PathToMp3, SecondsToProcess, StartAtSecond)
                .WithCustomAlgorithmConfiguration(config =>
                    {
                        config.Stride = new IncrementalStaticStride(StaticStride, config.SamplesPerFingerprint);
                    })
                .Hash()
                .Result;

            List<SubFingerprint> subFingerprints = new List<SubFingerprint>();
            foreach (var hash in hashData)
            {
                var subFingerprint = new SubFingerprint(hash.SubFingerprint, track.Id);
                subFingerprintDao.Insert(subFingerprint);
                int tableNumber = 1;
                foreach (var hashBin in hash.HashBins)
                {
                    var hashBinModel = new HashBinMinHash(hashBin, tableNumber++, subFingerprint.Id);
                    hashBinMinHashDao.Insert(hashBinModel);
                }

                subFingerprints.Add(subFingerprint);
            }

            var actualTrack = trackDao.ReadTrackByISRC(tagInfo.ISRC);
            Assert.IsNotNull(actualTrack);
            AssertTracksAreEqual(track, actualTrack);

            // Act
            int modifiedRows = trackDao.DeleteTrack(track.Id);
            Assert.IsNull(trackDao.ReadTrackByISRC(tagInfo.ISRC));
            foreach (var subFingerprint in subFingerprints)
            {
                Assert.IsTrue(subFingerprint.Id != 0);
                Assert.IsNull(subFingerprintDao.ReadById(subFingerprint.Id));
            }

            for (int i = 1; i <= 25; i++)
            {
                Assert.IsTrue(hashBinMinHashDao.ReadHashBinsByHashTable(i).Count == 0);
            }

            Assert.AreEqual(1 + subFingerprints.Count + (25 * subFingerprints.Count), modifiedRows);
        }

        [TestMethod]
        public void DeleteNonExistentTrackTest()
        {
            Track track = new Track();
            const int Expected = 0;
            int actual = trackDao.DeleteTrack(track.Id);
            Assert.AreEqual(Expected, actual);
        }

        [TestMethod]
        public void ReadTrackByRandomArtistAndTitleNameTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            string artist = name;
            string title = name;
            Track actual = trackDao.ReadTrackByArtistAndTitleName(artist, title);
            Assert.IsNull(actual);
        }

        private List<Track> GetRandomListOfTracks(int count)
        {
            var tracks = new List<Track>();
            for (int i = 0; i < count; i++)
            {
                Track track = GetTrack(MethodBase.GetCurrentMethod().Name);
                track.ISRC += i;
                tracks.Add(track);
            }

            return tracks;
        }

        private Track GetTrack(string methodName)
        {
            string isrc = Guid.NewGuid().ToString();
            string albumName = methodName + "album name";
            string artist = methodName + "artist";
            string title = methodName + "title";
            return new Track(isrc, artist, title, albumName, 1986, 360);
        }

        private void AssertTracksAreEqual(Track expectedTrack, Track actualTrack)
        {
            Assert.AreEqual(expectedTrack.Id, actualTrack.Id);
            Assert.AreEqual(expectedTrack.Album, actualTrack.Album);
            Assert.AreEqual(expectedTrack.Artist, actualTrack.Artist);
            Assert.AreEqual(expectedTrack.Title, actualTrack.Title);
            Assert.AreEqual(expectedTrack.TrackLengthSec, actualTrack.TrackLengthSec);
            Assert.AreEqual(expectedTrack.ISRC, actualTrack.ISRC);
        }
    }
}
