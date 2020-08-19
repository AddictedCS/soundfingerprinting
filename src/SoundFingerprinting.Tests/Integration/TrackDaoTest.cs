namespace SoundFingerprinting.Tests.Integration
{
    using NUnit.Framework;
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.InMemory;
    using SoundFingerprinting.Math;
    using SoundFingerprinting.Strides;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    [TestFixture]
    public class TrackDaoTest : IntegrationWithSampleFilesTest
    {
        private readonly IAudioService audioService = new SoundFingerprintingAudioService();
        private ITrackDao trackDao;
        private ISubFingerprintDao subFingerprintDao;

        [SetUp]
        public void SetUp()
        {
            var ramStorage = new RAMStorage(25);
            trackDao = new TrackDao(ramStorage);
            subFingerprintDao = new SubFingerprintDao(ramStorage, new StandardGroupingCounter());
        }

        [Test]
        public void GetTrackIdsTest()
        {
            const int trackCount = 5;
            var expectedTracks = InsertTracks(trackCount);

            var tracks = trackDao.GetTrackIds().ToList();

            Assert.AreEqual(trackCount, tracks.Count);
            foreach (var expectedTrack in expectedTracks)
            {
                Assert.IsTrue(tracks.Any(trackId => trackId == expectedTrack.Id));
            }
        }

        [Test]
        public void InsertMultipleTrackAtOnceTest()
        {
            const int trackCount = 100;
            var tracks = InsertTracks(trackCount);

            var actualTracks = trackDao.GetTrackIds().ToList();

            Assert.AreEqual(tracks.Count, actualTracks.Count);
        }

        [Test]
        public void ReadTrackByIdTest()
        {
            var trackReference = new ModelReference<int>(101);
            var expectedTrack = GetTrack(trackReference, 10);
            trackDao.InsertTrack(expectedTrack);

            TrackData actualTrack = trackDao.ReadTrackById(expectedTrack.Id);

            AssertTracksAreEqual(expectedTrack, actualTrack);
        }

        [Test]
        public void DeleteCollectionOfTracksTest()
        {
            const int numberOfTracks = 10;
            InsertTracks(numberOfTracks);

            var allTracks = trackDao.GetTrackIds().ToList();

            Assert.IsTrue(allTracks.Count == numberOfTracks);
            foreach (var track in allTracks.Select(trackId => trackDao.ReadTrackById(trackId)))
            {
                Assert.IsNotNull(track);
                trackDao.DeleteTrack(track.TrackReference);
            }

            Assert.IsFalse(trackDao.GetTrackIds().Any());
        }

        [Test]
        public void DeleteOneTrackTest()
        {
            var trackReference = new ModelReference<int>(101);
            var track = GetTrack(trackReference);
            trackDao.InsertTrack(track);

            trackDao.DeleteTrack(trackReference);

            Assert.IsEmpty(trackDao.ReadTracksByReferences(new []{trackReference}));
        }

        [Test]
        public async Task DeleteHashBinsAndSubFingerprintsOnTrackDelete()
        {
            var tagInfo = GetTagInfo();
            var track = new TrackInfo(tagInfo.ISRC, tagInfo.Title, tagInfo.Artist);
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

            var modelReferenceTracker = new UIntModelReferenceTracker();
            var (trackData, subFingerprintData) = modelReferenceTracker.AssignModelReferences(track, hashData);
            trackDao.InsertTrack(trackData);
            subFingerprintDao.InsertSubFingerprints(subFingerprintData);
            
            var actualTrack = trackDao.ReadTrackById(tagInfo.ISRC);
            Assert.IsNotNull(actualTrack);

            // Act
            int modifiedRows = trackDao.DeleteTrack(trackData.TrackReference) +
                               subFingerprintDao.DeleteSubFingerprintsByTrackReference(trackData.TrackReference);

            Assert.IsNull(trackDao.ReadTrackById(tagInfo.ISRC));
            Assert.IsFalse(subFingerprintDao.ReadHashedFingerprintsByTrackReference(actualTrack.TrackReference).Any());
            Assert.AreEqual(1 + hashData.Count + 25 * hashData.Count, modifiedRows);
        }

        [Test]
        public void InsertTrackShouldAcceptEmptyEntriesCodes()
        {
            var track = new TrackData(string.Empty, string.Empty, string.Empty, 120d, new ModelReference<int>(101));
            trackDao.InsertTrack(track);

            var actualTrack = trackDao.ReadTracksByReferences(new [] { track.TrackReference }).First();
 
            AssertTracksAreEqual(track, actualTrack);
        }

        private List<TrackData> InsertTracks(int trackCount)
        {
            var tracks = new List<TrackData>();
            for (int i = 0; i < trackCount; i++)
            {
                var modelReference = new ModelReference<int>(i);
                var track = GetTrack(modelReference, 10);
                tracks.Add(track);
                trackDao.InsertTrack(track);
            }

            return tracks;
        }

        private static TrackData GetTrack(IModelReference modelReference, double length = 120)
        {
            return new TrackData(Guid.NewGuid().ToString(), "artist", "title", length, modelReference);
        }
    }
}