namespace SoundFingerprinting.Tests.Integration.Dao
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Audio.Bass;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Strides;

    [TestClass]
    public abstract class AbstractTrackDaoTest : AbstractIntegrationTest
    {
        private readonly IFingerprintCommandBuilder fingerprintCommandBuilder;
        private readonly ITagService tagService;
        private readonly IAudioService audioService;

        protected AbstractTrackDaoTest()
        {
            fingerprintCommandBuilder = new FingerprintCommandBuilder();
            tagService = new BassTagService();
            audioService = new BassAudioService();
        }

        public abstract ITrackDao TrackDao { get; set; }

        public abstract ISubFingerprintDao SubFingerprintDao { get; set; }

        public abstract IHashBinDao HashBinDao { get; set; }

        [TestMethod]
        public void InsertTrackTest()
        {
            var track = GetTrack();

            var trackReference = TrackDao.InsertTrack(track);

            AssertModelReferenceIsInitialized(trackReference);
            AssertModelReferenceIsInitialized(track.TrackReference);
        }

        [TestMethod]
        public void MultipleInsertTest()
        {
            var modelReferences = new ConcurrentBag<IModelReference>();
            for (int i = 0; i < 1000; i++)
            {
                var modelReference = TrackDao.InsertTrack(new TrackData("isrc", "artist", "title", "album", 2012, 200)
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
            
            var tracks = TrackDao.ReadAll();

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

            var trackReference = TrackDao.InsertTrack(track);

            AssertTracksAreEqual(track, TrackDao.ReadTrack(trackReference));
        }

        [TestMethod]
        public void InsertMultipleTrackAtOnceTest()
        {
            const int TrackCount = 100;
            var tracks = InsertTracks(TrackCount);

            var actualTracks = TrackDao.ReadAll();

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
            TrackDao.InsertTrack(track);

            var tracks = TrackDao.ReadTrackByArtistAndTitleName(track.Artist, track.Title);

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
            TrackData expectedTrack = GetTrack();
            TrackDao.InsertTrack(expectedTrack);

            TrackData actualTrack = TrackDao.ReadTrackByISRC(expectedTrack.ISRC);

            AssertTracksAreEqual(expectedTrack, actualTrack);
        }

        [TestMethod]
        public void DeleteCollectionOfTracksTest()
        {
            const int NumberOfTracks = 10;
            var tracks = InsertTracks(NumberOfTracks);
            
            var allTracks = TrackDao.ReadAll();

            Assert.IsTrue(allTracks.Count == NumberOfTracks);
            foreach (var track in tracks)
            {
                TrackDao.DeleteTrack(track.TrackReference);
            }

            Assert.IsTrue(TrackDao.ReadAll().Count == 0);
        }

        [TestMethod]
        public void DeleteOneTrackTest()
        {
            TrackData track = GetTrack();
            var trackReference = TrackDao.InsertTrack(track);

            TrackDao.DeleteTrack(trackReference);

            Assert.IsNull(TrackDao.ReadTrack(trackReference));
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
            var trackReference = TrackDao.InsertTrack(track);
            var hashData = fingerprintCommandBuilder
                .BuildFingerprintCommand()
                .From(PathToMp3, SecondsToProcess, StartAtSecond)
                .WithFingerprintConfig(config =>
                    {
                        config.SpectrogramConfig.Stride = new IncrementalStaticStride(StaticStride, config.SamplesPerFingerprint);
                    })
                .UsingServices(audioService)
                .Hash()
                .Result;

            var subFingerprintReferences = new List<IModelReference>();
            int counter = 1;
            foreach (var hash in hashData)
            {
                var subFingerprintReference = SubFingerprintDao.InsertSubFingerprint(hash.SubFingerprint, counter++, hash.SequenceAt, trackReference);
                HashBinDao.InsertHashBins(hash.HashBins, subFingerprintReference, trackReference);
                subFingerprintReferences.Add(subFingerprintReference);
            }

            var actualTrack = TrackDao.ReadTrackByISRC(tagInfo.ISRC);
            Assert.IsNotNull(actualTrack);
            AssertTracksAreEqual(track, actualTrack);

            // Act
            int modifiedRows = TrackDao.DeleteTrack(trackReference);

            Assert.IsNull(TrackDao.ReadTrackByISRC(tagInfo.ISRC));
            foreach (var id in subFingerprintReferences)
            {
                Assert.IsTrue(id.GetHashCode() != 0);
                Assert.IsNull(SubFingerprintDao.ReadSubFingerprint(id));
            }
 
            Assert.IsTrue(HashBinDao.ReadHashDataByTrackReference(actualTrack.TrackReference).Count == 0);
            Assert.AreEqual(1 + hashData.Count + (25 * hashData.Count), modifiedRows);
        }

        [TestMethod]
        public void InserTrackShouldAcceptEmptyEntriesCodes()
        {
            TrackData track = new TrackData(string.Empty, string.Empty, string.Empty, string.Empty, 1986, 200);
            var trackReference = TrackDao.InsertTrack(track);

            var actualTrack = TrackDao.ReadTrack(trackReference);

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
                TrackDao.InsertTrack(track);
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
