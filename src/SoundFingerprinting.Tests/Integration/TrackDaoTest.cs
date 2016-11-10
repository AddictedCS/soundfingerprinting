namespace SoundFingerprinting.Tests.Integration
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.InMemory;
    using SoundFingerprinting.Strides;

    [TestClass]
    public class TrackDaoTest : IntegrationWithSampleFilesTest
    {
        private ITrackDao trackDao;
        private ISubFingerprintDao subFingerprintDao;
        private IHashBinDao hashBinDao;

        [TestInitialize]
        public void SetUp()
        {
            var ramStorage = new RAMStorage(NumberOfHashTables);
            hashBinDao = new HashBinDao(ramStorage);
            trackDao = new TrackDao(ramStorage);
            subFingerprintDao = new SubFingerprintDao(ramStorage);
        }

        [TestMethod]
        public void InsertTrackTest()
        {
            var track = GetTrack();

            var trackReference = trackDao.InsertTrack(track);

            AssertModelReferenceIsInitialized(trackReference);
            AssertModelReferenceIsInitialized(track.TrackReference);
        }

        [TestMethod]
        public void MultipleInsertTest()
        {
            var modelReferences = new ConcurrentBag<IModelReference>();
            for (int i = 0; i < 1000; i++)
            {
                var modelReference = trackDao.InsertTrack(new TrackData("isrc", "artist", "title", "album", 2012, 200)
                    {
                        GroupId = "group-id"
                    });

                Assert.IsFalse(modelReferences.Contains(modelReference));
                modelReferences.Add(modelReference);
            }
        }

        [TestMethod]
        public void ReadAllTracksTest()
        {
            const int TrackCount = 5;
            var expectedTracks = InsertTracks(TrackCount);
            
            var tracks = trackDao.ReadAll();

            Assert.IsTrue(tracks.Count == TrackCount);
            foreach (var expectedTrack in expectedTracks)
            {
                Assert.IsTrue(tracks.Any(track => track.ISRC == expectedTrack.ISRC));
            }
        }

        [TestMethod]
        public void ReadByIdTest()
        {
            var track = new TrackData("isrc", "artist", "title", "album", 2012, 200)
                {
                    GroupId = "group-id"
                };

            var trackReference = trackDao.InsertTrack(track);

            AssertTracksAreEqual(track, trackDao.ReadTrack(trackReference));
        }

        [TestMethod]
        public void InsertMultipleTrackAtOnceTest()
        {
            const int TrackCount = 100;
            var tracks = InsertTracks(TrackCount);

            var actualTracks = trackDao.ReadAll();

            Assert.AreEqual(tracks.Count, actualTracks.Count);
            for (int i = 0; i < actualTracks.Count; i++)
            {
                AssertModelReferenceIsInitialized(actualTracks[i].TrackReference);
                AssertTracksAreEqual(tracks[i], actualTracks.First(track => track.TrackReference.Equals(tracks[i].TrackReference)));
            }
        }

        [TestMethod]
        public void ReadTrackByArtistAndTitleTest()
        {
            TrackData track = GetTrack();
            trackDao.InsertTrack(track);

            var tracks = trackDao.ReadTrackByArtistAndTitleName(track.Artist, track.Title);

            Assert.IsNotNull(tracks);
            Assert.IsTrue(tracks.Count == 1);
            AssertTracksAreEqual(track, tracks[0]);
        }

        [TestMethod]
        public void ReadByNonExistentArtistAndTitleTest()
        {
            var tracks = trackDao.ReadTrackByArtistAndTitleName("artist", "title");

            Assert.IsTrue(tracks.Count == 0);
        }

        [TestMethod]
        public void ReadTrackByISRCTest()
        {
            TrackData expectedTrack = GetTrack();
            trackDao.InsertTrack(expectedTrack);

            TrackData actualTrack = trackDao.ReadTrackByISRC(expectedTrack.ISRC);

            AssertTracksAreEqual(expectedTrack, actualTrack);
        }

        [TestMethod]
        public void DeleteCollectionOfTracksTest()
        {
            const int NumberOfTracks = 10;
            var tracks = InsertTracks(NumberOfTracks);
            
            var allTracks = trackDao.ReadAll();

            Assert.IsTrue(allTracks.Count == NumberOfTracks);
            foreach (var track in tracks)
            {
                trackDao.DeleteTrack(track.TrackReference);
            }

            Assert.IsTrue(trackDao.ReadAll().Count == 0);
        }

        [TestMethod]
        public void DeleteOneTrackTest()
        {
            TrackData track = GetTrack();
            var trackReference = trackDao.InsertTrack(track);

            trackDao.DeleteTrack(trackReference);

            Assert.IsNull(trackDao.ReadTrack(trackReference));
        }

        [TestMethod]
        public void DeleteHashBinsAndSubfingerprintsOnTrackDelete()
        {
            const int StaticStride = 5115;
            const int SecondsToProcess = 20;
            const int StartAtSecond = 30;
            TagInfo tagInfo = GetTagInfo();
            int releaseYear = tagInfo.Year;
            TrackData track = new TrackData(tagInfo.ISRC, tagInfo.Artist, tagInfo.Title, tagInfo.Album, releaseYear, (int)tagInfo.Duration);
            var trackReference = trackDao.InsertTrack(track);
            var hashData = FingerprintCommandBuilder
                .BuildFingerprintCommand()
                .From(PathToMp3, SecondsToProcess, StartAtSecond)
                .WithFingerprintConfig(config =>
                    {
                        config.SpectrogramConfig.Stride = new IncrementalStaticStride(StaticStride, config.SamplesPerFingerprint);
                    })
                .UsingServices(AudioService)
                .Hash()
                .Result;

            var subFingerprintReferences = new List<IModelReference>();
            foreach (var hash in hashData)
            {
                var subFingerprintReference = subFingerprintDao.InsertSubFingerprint(hash.SubFingerprint, hash.SequenceNumber, hash.Timestamp, trackReference);
                hashBinDao.InsertHashBins(hash.HashBins, subFingerprintReference, trackReference);
                subFingerprintReferences.Add(subFingerprintReference);
            }

            var actualTrack = trackDao.ReadTrackByISRC(tagInfo.ISRC);
            Assert.IsNotNull(actualTrack);
            AssertTracksAreEqual(track, actualTrack);

            // Act
            int modifiedRows = trackDao.DeleteTrack(trackReference);

            Assert.IsNull(trackDao.ReadTrackByISRC(tagInfo.ISRC));
            foreach (var id in subFingerprintReferences)
            {
                Assert.IsTrue(id.GetHashCode() != 0);
                Assert.IsNull(subFingerprintDao.ReadSubFingerprint(id));
            }
 
            Assert.IsTrue(hashBinDao.ReadHashedFingerprintsByTrackReference(actualTrack.TrackReference).Count == 0);
            Assert.AreEqual(1 + hashData.Count + (25 * hashData.Count), modifiedRows);
        }

        [TestMethod]
        public void InserTrackShouldAcceptEmptyEntriesCodes()
        {
            TrackData track = new TrackData(string.Empty, string.Empty, string.Empty, string.Empty, 1986, 200);
            var trackReference = trackDao.InsertTrack(track);

            var actualTrack = trackDao.ReadTrack(trackReference);

            AssertModelReferenceIsInitialized(trackReference);
            AssertTracksAreEqual(track, actualTrack);
        }

        private List<TrackData> InsertTracks(int trackCount)
        {
            var tracks = new List<TrackData>();
            for (int i = 0; i < trackCount; i++)
            {
                var track = GetTrack();
                tracks.Add(track);
                trackDao.InsertTrack(track);
            }

            return tracks;
        }

        private TrackData GetTrack()
        {
            return new TrackData(Guid.NewGuid().ToString(), "artist", "title", "album", 1986, 360)
                { 
                    GroupId = Guid.NewGuid().ToString().Substring(0, 20)
                };
        }
    }
}
