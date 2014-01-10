namespace SoundFingerprinting.Tests.Integration.Dao
{
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Dao;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Tests.Integration;

    [TestClass]
    public class ModelServiceTest : AbstractIntegrationTest
    {
        private readonly IModelService modelService = new ModelService();

        [TestMethod]
        public void InsertTrackTest()
        {
            TrackData track = new TrackData("isrc", "artist", "title", "album", 1986, 200);

            var trackReference = modelService.InsertTrack(track);

            Assert.IsNotNull(trackReference);
            Assert.IsFalse(trackReference.HashCode == 0);
        }

        [TestMethod]
        public void ReadTrackByTrackReferenceTest()
        {
            TrackData expectedTrack = new TrackData("isrc", "artist", "title", "album", 1986, 200);
            var trackReference = modelService.InsertTrack(expectedTrack);

            var actualTrack = modelService.ReadTrackByReference(trackReference);

            AssertTracksAreEqual(expectedTrack, actualTrack);
        }

        [TestMethod]
        public void ReadTrackByISRCTest()
        {
            const string ISRC = "isrc";
            TrackData expectedTrack = new TrackData(ISRC, "artist", "title", "album", 1986, 200);
            modelService.InsertTrack(expectedTrack);

            var actualTrack = modelService.ReadTrackByISRC(ISRC);

            AssertTracksAreEqual(expectedTrack, actualTrack);
        }

        [TestMethod]
        public void ReadTrackByArtistAndTitleTest()
        {
            const string Artist = "artist";
            const string Title = "title";
            TrackData expectedTrack = new TrackData("isrc", Artist, Title, "album", 1986, 200);
            modelService.InsertTrack(expectedTrack);

            var actualTracks = modelService.ReadTrackByArtistAndTitleName(Artist, Title);

            Assert.IsTrue(actualTracks.Count == 1);
            AssertTracksAreEqual(expectedTrack, actualTracks[0]);
        }

        [TestMethod]
        public void ReadMultipleTracksTest()
        {
            const int NumberOfTracks = 100;
            HashSet<TrackData> allTracks = new HashSet<TrackData>();
            for (int i = 0; i < NumberOfTracks; i++)
            {
                TrackData track = new TrackData("isrc" + i, "artist", "title", "album", 1986, 200);
                modelService.InsertTrack(track);
                if (!allTracks.Add(track))
                {
                    Assert.Fail("Same primary key identifier was returned after inserting a track to the collection.");
                }
            }

            var actualTracks = modelService.ReadAllTracks();
            Assert.IsTrue(actualTracks.Count == NumberOfTracks);
        }

        [TestMethod]
        public void DeleteTrackTest()
        {
            const int Threshold = 5;
            TrackData track = new TrackData("isrc", "artist", "title", "album", 1986, 200);
            var trackReference = modelService.InsertTrack(track);
            var hashData = new HashData(GenericSignature, GenericHashBuckets);
            modelService.InsertHashDataForTrack(new[] { hashData }, trackReference);

            modelService.DeleteTrack(trackReference);

            var subFingerprints = modelService.ReadSubFingerprintDataByHashBucketsWithThreshold(GenericHashBuckets, Threshold);
            Assert.IsTrue(subFingerprints.Any() == false);
            TrackData actualTrack = modelService.ReadTrackByReference(trackReference);
            Assert.IsNull(actualTrack);
        }

        [TestMethod]
        public void InsertHashDataTest()
        {
            const int Threshold = 5;
            TrackData expectedTrack = new TrackData("isrc", "artist", "title", "album", 1986, 200);
            var trackReference = modelService.InsertTrack(expectedTrack);
            var hashData = new HashData(GenericSignature, GenericHashBuckets);
            modelService.InsertHashDataForTrack(new[] { hashData }, trackReference);

            var subFingerprints = modelService.ReadSubFingerprintDataByHashBucketsWithThreshold(GenericHashBuckets, Threshold);

            Assert.IsTrue(subFingerprints.Count == 1);
            Assert.AreEqual(trackReference.HashCode, subFingerprints[0].TrackReference.HashCode);
            Assert.IsFalse(subFingerprints[0].SubFingerprintReference.GetHashCode() == 0);
            for (int i = 0; i < GenericSignature.Length; i++)
            {
                Assert.AreEqual(GenericSignature[i], subFingerprints[0].Signature[i]);
            }
        }

        [TestMethod]
        public void ReadSubFingerprintsByHashBucketsHavingThresholdTest()
        {
            const int Threshold = 5;
            TrackData firstTrack = new TrackData("isrc1", "artist", "title", "album", 1986, 200);
            var firstTrackReference = modelService.InsertTrack(firstTrack);
            TrackData secondTrack = new TrackData("isrc2", "artist", "title", "album", 1986, 200);
            var secondTrackReference = modelService.InsertTrack(secondTrack);
            Assert.IsFalse(firstTrackReference.HashCode == secondTrackReference.HashCode);
            long[] firstTrackBuckets = new long[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 };
            long[] secondTrackBuckets = new long[] { 2, 2, 4, 5, 6, 7, 7, 9, 10, 11, 12, 13, 14, 14, 16, 17, 18, 19, 20, 20, 22, 23, 24, 25, 26 };
            var firstHashData = new HashData(GenericSignature, firstTrackBuckets);
            var secondHashData = new HashData(GenericSignature, secondTrackBuckets);

            modelService.InsertHashDataForTrack(new[] { firstHashData }, firstTrackReference);
            modelService.InsertHashDataForTrack(new[] { secondHashData }, secondTrackReference);

            // query buckets are similar with 5 elements from first track and 4 elements from second track
            long[] queryBuckets = new long[] { 3, 2, 5, 6, 7, 8, 7, 10, 11, 12, 13, 14, 15, 14, 17, 18, 19, 20, 21, 20, 23, 24, 25, 26, 25 };

            var subFingerprints = modelService.ReadSubFingerprintDataByHashBucketsWithThreshold(queryBuckets, Threshold);

            Assert.IsTrue(subFingerprints.Count == 1);
            Assert.AreEqual(firstTrackReference.HashCode, subFingerprints[0].TrackReference.HashCode);
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
