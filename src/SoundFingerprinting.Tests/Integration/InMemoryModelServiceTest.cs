namespace SoundFingerprinting.Tests.Integration
{
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.InMemory;

    [TestClass]
    public class InMemoryModelServiceTest : AbstractIntegrationTest
    {
        private IModelService modelService;

        [TestInitialize]
        public void SetUp()
        {
            var ramStorage = new RAMStorage(NumberOfHashTables);
            this.modelService = new InMemoryModelService(
                new TrackDao(ramStorage),
                new HashBinDao(ramStorage),
                new SubFingerprintDao(ramStorage),
                new FingerprintDao(ramStorage),
                new SpectralImageDao());
        }

        [TestMethod]
        public void InsertTrackTest()
        {
            TrackData track = new TrackData("isrc", "artist", "title", "album", 1986, 200);

            var trackReference = this.modelService.InsertTrack(track);

            AssertModelReferenceIsInitialized(trackReference);
        }

        [TestMethod]
        public void ReadTrackByTrackReferenceTest()
        {
            TrackData expectedTrack = new TrackData("isrc", "artist", "title", "album", 1986, 200);
            var trackReference = this.modelService.InsertTrack(expectedTrack);

            var actualTrack = this.modelService.ReadTrackByReference(trackReference);

            AssertTracksAreEqual(expectedTrack, actualTrack);
        }

        [TestMethod]
        public void ReadTrackByISRCTest()
        {
            const string ISRC = "isrc";
            TrackData expectedTrack = new TrackData(ISRC, "artist", "title", "album", 1986, 200);
            this.modelService.InsertTrack(expectedTrack);

            var actualTrack = this.modelService.ReadTrackByISRC(ISRC);

            AssertTracksAreEqual(expectedTrack, actualTrack);
        }

        [TestMethod]
        public void ReadTrackByArtistAndTitleTest()
        {
            const string Artist = "artist";
            const string Title = "title";
            TrackData expectedTrack = new TrackData("isrc", Artist, Title, "album", 1986, 200);
            this.modelService.InsertTrack(expectedTrack);

            var actualTracks = this.modelService.ReadTrackByArtistAndTitleName(Artist, Title);

            Assert.IsTrue(actualTracks.Count == 1);
            AssertTracksAreEqual(expectedTrack, actualTracks[0]);
        }

        [TestMethod]
        public void ReadMultipleTracksTest()
        {
            const int NumberOfTracks = 100;
            var allTracks = new HashSet<TrackData>();
            for (int i = 0; i < NumberOfTracks; i++)
            {
                TrackData track = new TrackData("isrc" + i, "artist", "title", "album", 1986, 200);
                this.modelService.InsertTrack(track);
                if (!allTracks.Add(track))
                {
                    Assert.Fail("Same primary key identifier was returned after inserting a track to the collection.");
                }
            }

            var actualTracks = this.modelService.ReadAllTracks();
            Assert.IsTrue(actualTracks.Count == NumberOfTracks);
        }

        [TestMethod]
        public void DeleteTrackTest()
        {
            const int Threshold = 5;
            TrackData track = new TrackData("isrc", "artist", "title", "album", 1986, 200);
            var trackReference = this.modelService.InsertTrack(track);
            var hashedFingerprints = new HashedFingerprint(GenericSignature, GenericHashBuckets, 1, 0.928);
            this.modelService.InsertHashDataForTrack(new[] { hashedFingerprints }, trackReference);

            this.modelService.DeleteTrack(trackReference);

            var subFingerprints = this.modelService.ReadSubFingerprintDataByHashBucketsWithThreshold(GenericHashBuckets, Threshold);
            Assert.IsTrue(subFingerprints.Any() == false);
            TrackData actualTrack = this.modelService.ReadTrackByReference(trackReference);
            Assert.IsNull(actualTrack);
        }

        [TestMethod]
        public void InsertHashDataTest()
        {
            const int Threshold = 5;
            TrackData expectedTrack = new TrackData("isrc", "artist", "title", "album", 1986, 200);
            var trackReference = this.modelService.InsertTrack(expectedTrack);
            var hashedFingerprints = new HashedFingerprint(GenericSignature, GenericHashBuckets, 1, 0.928);
            this.modelService.InsertHashDataForTrack(new[] { hashedFingerprints }, trackReference);

            var subFingerprints = this.modelService.ReadSubFingerprintDataByHashBucketsWithThreshold(GenericHashBuckets, Threshold);

            Assert.IsTrue(subFingerprints.Count == 1);
            Assert.AreEqual(trackReference, subFingerprints[0].TrackReference);
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
            var firstTrackReference = this.modelService.InsertTrack(firstTrack);
            TrackData secondTrack = new TrackData("isrc2", "artist", "title", "album", 1986, 200);
            var secondTrackReference = this.modelService.InsertTrack(secondTrack);
            Assert.IsFalse(firstTrackReference.Equals(secondTrackReference));
            long[] firstTrackBuckets = new long[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 };
            long[] secondTrackBuckets = new long[] { 2, 2, 4, 5, 6, 7, 7, 9, 10, 11, 12, 13, 14, 14, 16, 17, 18, 19, 20, 20, 22, 23, 24, 25, 26 };
            var firstHashData = new HashedFingerprint(GenericSignature, firstTrackBuckets, 1, 0.928);
            var secondHashData = new HashedFingerprint(GenericSignature, secondTrackBuckets, 1, 0.928);

            this.modelService.InsertHashDataForTrack(new[] { firstHashData }, firstTrackReference);
            this.modelService.InsertHashDataForTrack(new[] { secondHashData }, secondTrackReference);

            // query buckets are similar with 5 elements from first track and 4 elements from second track
            long[] queryBuckets = new long[] { 3, 2, 5, 6, 7, 8, 7, 10, 11, 12, 13, 14, 15, 14, 17, 18, 19, 20, 21, 20, 23, 24, 25, 26, 25 };

            var subFingerprints = this.modelService.ReadSubFingerprintDataByHashBucketsWithThreshold(queryBuckets, Threshold);

            Assert.IsTrue(subFingerprints.Count == 1);
            Assert.AreEqual(firstTrackReference, subFingerprints[0].TrackReference);
        }

        [TestMethod]
        public void ReadSubFingerprintsByHashBucketsHavingThresholdWithGroupIdTest()
        {
            const int Threshold = 5;
            TrackData firstTrack = new TrackData("isrc1", "artist", "title", "album", 1986, 200)
                { GroupId = "first-group-id" };
            var firstTrackReference = this.modelService.InsertTrack(firstTrack);
            TrackData secondTrack = new TrackData("isrc2", "artist", "title", "album", 1986, 200)
                { GroupId = "second-group-id" };
            var secondTrackReference = this.modelService.InsertTrack(secondTrack);
            Assert.IsFalse(firstTrackReference.Equals(secondTrackReference));
            long[] firstTrackBuckets = new long[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 };
            long[] secondTrackBuckets = new long[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 };
            var firstHashData = new HashedFingerprint(GenericSignature, firstTrackBuckets, 1, 0.928);
            var secondHashData = new HashedFingerprint(GenericSignature, secondTrackBuckets, 1, 0.928);

            this.modelService.InsertHashDataForTrack(new[] { firstHashData }, firstTrackReference);
            this.modelService.InsertHashDataForTrack(new[] { secondHashData }, secondTrackReference);

            // query buckets are similar with 5 elements from first track and 4 elements from second track
            long[] queryBuckets = new long[] { 3, 2, 5, 6, 7, 8, 7, 10, 11, 12, 13, 14, 15, 14, 17, 18, 19, 20, 21, 20, 23, 24, 25, 26, 25 };

            var subFingerprints = this.modelService.ReadSubFingerprintDataByHashBucketsThresholdWithGroupId(queryBuckets, Threshold, "first-group-id");

            Assert.IsTrue(subFingerprints.Count == 1);
            Assert.AreEqual(firstTrackReference, subFingerprints[0].TrackReference);
        }

        [TestMethod]
        public void InsertFingerprintTest()
        {
            var track = new TrackData("isrc", "artist", "title", "album", 1986, 200);
            var trackReference = this.modelService.InsertTrack(track);
            var fingerprint = new FingerprintData(GenericFingerprint, trackReference);

            this.modelService.InsertFingerprint(fingerprint);

            AssertModelReferenceIsInitialized(fingerprint.FingerprintReference);
        }

        [TestMethod]
        public void ReadFingerprintsByTrackReferenceTest()
        {
            TrackData track = new TrackData("isrc", "artist", "title", "album", 1986, 200);
            var trackReference = this.modelService.InsertTrack(track);
            FingerprintData fingerprint = new FingerprintData(GenericFingerprint, trackReference);
            this.modelService.InsertFingerprint(fingerprint);

            var fingerprints = this.modelService.ReadFingerprintsByTrackReference(trackReference);

            Assert.IsTrue(fingerprints.Count == 1);
            Assert.AreEqual(fingerprint.FingerprintReference, fingerprints[0].FingerprintReference);
            Assert.AreEqual(trackReference, fingerprints[0].TrackReference);
            for (int i = 0; i < GenericFingerprint.Length; i++)
            {
                Assert.AreEqual(GenericFingerprint[i], fingerprints[0].Signature[i]);
            }
        }
    }
}
