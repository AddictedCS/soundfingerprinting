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
    public class TrackDaoTest : AbstractIntegrationTest
    {
        private ITrackDao trackDao;
        private ISubFingerprintDao subFingerprintDao;
        private IHashBinDao hashBinDao;

        [TestInitialize]
        public void SetUp()
        {
            var ramStorage = new RAMStorage(NumberOfHashTables);
            this.hashBinDao = new HashBinDao(ramStorage);
            this.trackDao = new TrackDao(ramStorage);
            this.subFingerprintDao = new SubFingerprintDao(ramStorage);
        }

        [TestMethod]
        public void InsertTrackTest()
        {
            var track = this.GetTrack();

            var trackReference = this.trackDao.InsertTrack(track);

            this.AssertModelReferenceIsInitialized(trackReference);
            this.AssertModelReferenceIsInitialized(track.TrackReference);
        }

        [TestMethod]
        public void MultipleInsertTest()
        {
            var modelReferences = new ConcurrentBag<IModelReference>();
            for (int i = 0; i < 1000; i++)
            {
                var modelReference = this.trackDao.InsertTrack(new TrackData("isrc", "artist", "title", "album", 2012, 200)
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
            var expectedTracks = this.InsertTracks(TrackCount);
            
            var tracks = this.trackDao.ReadAll();

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

            var trackReference = this.trackDao.InsertTrack(track);

            this.AssertTracksAreEqual(track, this.trackDao.ReadTrack(trackReference));
        }

        [TestMethod]
        public void InsertMultipleTrackAtOnceTest()
        {
            const int TrackCount = 100;
            var tracks = this.InsertTracks(TrackCount);

            var actualTracks = this.trackDao.ReadAll();

            Assert.AreEqual(tracks.Count, actualTracks.Count);
            for (int i = 0; i < actualTracks.Count; i++)
            {
                this.AssertModelReferenceIsInitialized(actualTracks[i].TrackReference);
                this.AssertTracksAreEqual(tracks[i], actualTracks.First(track => track.TrackReference.Equals(tracks[i].TrackReference)));
            }
        }

        [TestMethod]
        public void ReadTrackByArtistAndTitleTest()
        {
            TrackData track = this.GetTrack();
            this.trackDao.InsertTrack(track);

            var tracks = this.trackDao.ReadTrackByArtistAndTitleName(track.Artist, track.Title);

            Assert.IsNotNull(tracks);
            Assert.IsTrue(tracks.Count == 1);
            this.AssertTracksAreEqual(track, tracks[0]);
        }

        [TestMethod]
        public void ReadByNonExistentArtistAndTitleTest()
        {
            var tracks = this.trackDao.ReadTrackByArtistAndTitleName("artist", "title");

            Assert.IsTrue(tracks.Count == 0);
        }

        [TestMethod]
        public void ReadTrackByISRCTest()
        {
            TrackData expectedTrack = this.GetTrack();
            this.trackDao.InsertTrack(expectedTrack);

            TrackData actualTrack = this.trackDao.ReadTrackByISRC(expectedTrack.ISRC);

            this.AssertTracksAreEqual(expectedTrack, actualTrack);
        }

        [TestMethod]
        public void DeleteCollectionOfTracksTest()
        {
            const int NumberOfTracks = 10;
            var tracks = this.InsertTracks(NumberOfTracks);
            
            var allTracks = this.trackDao.ReadAll();

            Assert.IsTrue(allTracks.Count == NumberOfTracks);
            foreach (var track in tracks)
            {
                this.trackDao.DeleteTrack(track.TrackReference);
            }

            Assert.IsTrue(this.trackDao.ReadAll().Count == 0);
        }

        [TestMethod]
        public void DeleteOneTrackTest()
        {
            TrackData track = this.GetTrack();
            var trackReference = this.trackDao.InsertTrack(track);

            this.trackDao.DeleteTrack(trackReference);

            Assert.IsNull(this.trackDao.ReadTrack(trackReference));
        }

        [TestMethod]
        public void DeleteHashBinsAndSubfingerprintsOnTrackDelete()
        {
            const int StaticStride = 5115;
            const int SecondsToProcess = 20;
            const int StartAtSecond = 30;
            TagInfo tagInfo = this.GetTagInfo();
            int releaseYear = tagInfo.Year;
            TrackData track = new TrackData(tagInfo.ISRC, tagInfo.Artist, tagInfo.Title, tagInfo.Album, releaseYear, (int)tagInfo.Duration);
            var trackReference = this.trackDao.InsertTrack(track);
            var hashData = this.FingerprintCommandBuilder
                .BuildFingerprintCommand()
                .From(PathToMp3, SecondsToProcess, StartAtSecond)
                .WithFingerprintConfig(config =>
                    {
                        config.SpectrogramConfig.Stride = new IncrementalStaticStride(StaticStride, config.SamplesPerFingerprint);
                    })
                .UsingServices(this.AudioService)
                .Hash()
                .Result;

            var subFingerprintReferences = new List<IModelReference>();
            foreach (var hash in hashData)
            {
                var subFingerprintReference = this.subFingerprintDao.InsertSubFingerprint(hash.SubFingerprint, hash.SequenceNumber, hash.Timestamp, trackReference);
                this.hashBinDao.InsertHashBins(hash.HashBins, subFingerprintReference, trackReference);
                subFingerprintReferences.Add(subFingerprintReference);
            }

            var actualTrack = this.trackDao.ReadTrackByISRC(tagInfo.ISRC);
            Assert.IsNotNull(actualTrack);
            this.AssertTracksAreEqual(track, actualTrack);

            // Act
            int modifiedRows = this.trackDao.DeleteTrack(trackReference);

            Assert.IsNull(this.trackDao.ReadTrackByISRC(tagInfo.ISRC));
            foreach (var id in subFingerprintReferences)
            {
                Assert.IsTrue(id.GetHashCode() != 0);
                Assert.IsNull(this.subFingerprintDao.ReadSubFingerprint(id));
            }
 
            Assert.IsTrue(this.hashBinDao.ReadHashedFingerprintsByTrackReference(actualTrack.TrackReference).Count == 0);
            Assert.AreEqual(1 + hashData.Count + (25 * hashData.Count), modifiedRows);
        }

        [TestMethod]
        public void InserTrackShouldAcceptEmptyEntriesCodes()
        {
            TrackData track = new TrackData(string.Empty, string.Empty, string.Empty, string.Empty, 1986, 200);
            var trackReference = this.trackDao.InsertTrack(track);

            var actualTrack = this.trackDao.ReadTrack(trackReference);

            this.AssertModelReferenceIsInitialized(trackReference);
            this.AssertTracksAreEqual(track, actualTrack);
        }

        private List<TrackData> InsertTracks(int trackCount)
        {
            var tracks = new List<TrackData>();
            for (int i = 0; i < trackCount; i++)
            {
                var track = this.GetTrack();
                tracks.Add(track);
                this.trackDao.InsertTrack(track);
            }

            return tracks;
        }

        private TrackData GetTrack()
        {
            return new TrackData(Guid.NewGuid().ToString(), "artist", "title", "album", 1986, 360)
                { 
                    GroupId = Guid.NewGuid().ToString().Substring(0, 20) // db max length
                };
        }
    }
}
