namespace SoundFingerprinting.Tests.Integration
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    using NUnit.Framework;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.InMemory;
    using SoundFingerprinting.Strides;

    [TestFixture]
    public class TrackDaoTest : IntegrationWithSampleFilesTest
    {
        private readonly IAudioService audioService = new SoundFingerprintingAudioService();
        private ITrackDao trackDao;
        private ISubFingerprintDao subFingerprintDao;

        [SetUp]
        public void SetUp()
        {
            var ramStorage = new RAMStorage(NumberOfHashTables);
            trackDao = new TrackDao(ramStorage);
            subFingerprintDao = new SubFingerprintDao(ramStorage);
        }

        [Test]
        public void InsertTrackTest()
        {
            var track = GetTrack();

            var trackReference = trackDao.InsertTrack(track);

            AssertModelReferenceIsInitialized(trackReference);
            AssertModelReferenceIsInitialized(track.TrackReference);
        }

        [Test]
        public void MultipleInsertTest()
        {
            var modelReferences = new ConcurrentBag<IModelReference>();
            for (int i = 0; i < 1000; i++)
            {
                var modelReference = trackDao.InsertTrack(new TrackData("isrc", "artist", "title", "album", 2012, 200));

                Assert.IsFalse(modelReferences.Contains(modelReference));
                modelReferences.Add(modelReference);
            }
        }

        [Test]
        public void ReadAllTracksTest()
        {
            const int TrackCount = 5;
            var expectedTracks = InsertTracks(TrackCount);
            
            var tracks = trackDao.ReadAll();

            Assert.AreEqual(TrackCount, tracks.Count);
            foreach (var expectedTrack in expectedTracks)
            {
                Assert.IsTrue(tracks.Any(track => track.ISRC == expectedTrack.ISRC));
            }
        }

        [Test]
        public void ReadByIdTest()
        {
            var track = new TrackData("isrc", "artist", "title", "album", 2012, 200);

            var trackReference = trackDao.InsertTrack(track);

            AssertTracksAreEqual(track, trackDao.ReadTrack(trackReference));
        }

        [Test]
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

        [Test]
        public void ReadTrackByArtistAndTitleTest()
        {
            TrackData track = GetTrack();
            trackDao.InsertTrack(track);

            var tracks = trackDao.ReadTrackByArtistAndTitleName(track.Artist, track.Title);

            Assert.IsNotNull(tracks);
            Assert.IsTrue(tracks.Count == 1);
            AssertTracksAreEqual(track, tracks[0]);
        }

        [Test]
        public void ReadByNonExistentArtistAndTitleTest()
        {
            var tracks = trackDao.ReadTrackByArtistAndTitleName("artist", "title");

            Assert.IsTrue(tracks.Count == 0);
        }

        [Test]
        public void ReadTrackByISRCTest()
        {
            TrackData expectedTrack = GetTrack();
            trackDao.InsertTrack(expectedTrack);

            TrackData actualTrack = trackDao.ReadTrackByISRC(expectedTrack.ISRC);

            AssertTracksAreEqual(expectedTrack, actualTrack);
        }

        [Test]
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

        [Test]
        public void DeleteOneTrackTest()
        {
            TrackData track = GetTrack();
            var trackReference = trackDao.InsertTrack(track);

            trackDao.DeleteTrack(trackReference);

            Assert.IsNull(trackDao.ReadTrack(trackReference));
        }

        [Test]
        public void DeleteHashBinsAndSubfingerprintsOnTrackDelete()
        {
            TagInfo tagInfo = GetTagInfo();
            int releaseYear = tagInfo.Year;
            var track = new TrackData(tagInfo.ISRC, tagInfo.Artist, tagInfo.Title, tagInfo.Album, releaseYear, (int)tagInfo.Duration);
            var trackReference = trackDao.InsertTrack(track);
            var hashData = FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(GetAudioSamples())
                .WithFingerprintConfig(config =>
                    {
                        config.Stride = new StaticStride(0);
                        return config;
                    })
                .UsingServices(audioService)
                .Hash()
                .Result;

            subFingerprintDao.InsertHashDataForTrack(hashData, trackReference);
            var actualTrack = trackDao.ReadTrackByISRC(tagInfo.ISRC);
            Assert.IsNotNull(actualTrack);
            AssertTracksAreEqual(track, actualTrack);

            // Act
            int modifiedRows = trackDao.DeleteTrack(trackReference);

            Assert.IsNull(trackDao.ReadTrackByISRC(tagInfo.ISRC));
            Assert.AreEqual(0, subFingerprintDao.ReadHashedFingerprintsByTrackReference(actualTrack.TrackReference).Count);
            Assert.AreEqual(1 + hashData.Count + (25 * hashData.Count), modifiedRows);
        }

        [Test]
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
            return new TrackData(Guid.NewGuid().ToString(), "artist", "title", "album", 1986, 360);
        }
    }
}
