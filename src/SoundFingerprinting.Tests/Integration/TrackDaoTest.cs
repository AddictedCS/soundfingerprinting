namespace SoundFingerprinting.Tests.Integration
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using NUnit.Framework;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.InMemory;
    using SoundFingerprinting.Math;
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
            subFingerprintDao = new SubFingerprintDao(ramStorage, new StandardGroupingCounter());
        }

        [Test]
        public void InsertTrackTest()
        {
            var track = GetTrack();

            var trackReference = trackDao.InsertTrack(track).TrackReference;

            AssertModelReferenceIsInitialized(trackReference);
        }

        [Test]
        public void MultipleInsertTest()
        {
            var modelReferences = new ConcurrentBag<IModelReference>();
            for (int i = 0; i < 1000; i++)
            {
                var modelReference = trackDao.InsertTrack(new TrackInfo("isrc", "artist", "title", 200)).TrackReference;

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
                Assert.IsTrue(tracks.Any(track => track.ISRC == expectedTrack.Id));
            }
        }

        [Test]
        public void ReadByIdTest()
        {
            var track = new TrackInfo("isrc", "artist", "title", 200);

            var trackReference = trackDao.InsertTrack(track).TrackReference;

            AssertTracksAreEqual(track, trackDao.ReadTrack(trackReference));
        }

        [Test]
        public void InsertMultipleTrackAtOnceTest()
        {
            const int TrackCount = 100;
            var tracks = InsertTracks(TrackCount);

            var actualTracks = trackDao.ReadAll();

            Assert.AreEqual(tracks.Count, actualTracks.Count);
        }

        [Test]
        public void ReadTrackByArtistAndTitleTest()
        {
            var track = GetTrack();
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
            var expectedTrack = GetTrack();
            trackDao.InsertTrack(expectedTrack);

            TrackData actualTrack = trackDao.ReadTrackById(expectedTrack.Id);

            AssertTracksAreEqual(expectedTrack, actualTrack);
        }

        [Test]
        public void DeleteCollectionOfTracksTest()
        {
            const int NumberOfTracks = 10;
            var tracks = InsertTracks(NumberOfTracks);
            
            var allTracks = trackDao.ReadAll();

            Assert.IsTrue(allTracks.Count == NumberOfTracks);
            foreach (var track in allTracks)
            {
                trackDao.DeleteTrack(track.TrackReference);
            }

            Assert.IsTrue(trackDao.ReadAll().Count == 0);
        }

        [Test]
        public void DeleteOneTrackTest()
        {
            var track = GetTrack();
            var trackReference = trackDao.InsertTrack(track).TrackReference;

            trackDao.DeleteTrack(trackReference);

            Assert.IsNull(trackDao.ReadTrack(trackReference));
        }

        [Test]
        public async Task DeleteHashBinsAndSubFingerprintsOnTrackDelete()
        {
            TagInfo tagInfo = GetTagInfo();
            var track = new TrackInfo(tagInfo.ISRC, tagInfo.Title, tagInfo.Artist, tagInfo.Duration);
            var trackReference = trackDao.InsertTrack(track).TrackReference;
            var hashData = await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(GetAudioSamples())
                .WithFingerprintConfig(config =>
                    {
                        config.Stride = new StaticStride(0);
                        return config;
                    })
                .UsingServices(audioService)
                .Hash();

            subFingerprintDao.InsertHashDataForTrack(hashData, trackReference);
            var actualTrack = trackDao.ReadTrackById(tagInfo.ISRC);
            Assert.IsNotNull(actualTrack);

            // Act
            int modifiedRows = trackDao.DeleteTrack(trackReference);

            Assert.IsNull(trackDao.ReadTrackById(tagInfo.ISRC));
            Assert.AreEqual(0, subFingerprintDao.ReadHashedFingerprintsByTrackReference(actualTrack.TrackReference).Count);
            Assert.AreEqual(1 + hashData.Count + (25 * hashData.Count), modifiedRows);
        }

        [Test]
        public void InsertTrackShouldAcceptEmptyEntriesCodes()
        {
            var track = new TrackInfo(string.Empty, string.Empty, string.Empty, 120d);
            var trackReference = trackDao.InsertTrack(track).TrackReference;

            var actualTrack = trackDao.ReadTrack(trackReference);

            AssertModelReferenceIsInitialized(trackReference);
            AssertTracksAreEqual(track, actualTrack);
        }

        private List<TrackInfo> InsertTracks(int trackCount)
        {
            var tracks = new List<TrackInfo>();
            for (int i = 0; i < trackCount; i++)
            {
                var track = GetTrack();
                tracks.Add(track);
                trackDao.InsertTrack(track);
            }

            return tracks;
        }

        private TrackInfo GetTrack()
        {
            return new TrackInfo(Guid.NewGuid().ToString(), "artist", "title", 360);
        }
    }
}
