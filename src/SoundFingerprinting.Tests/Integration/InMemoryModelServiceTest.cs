namespace SoundFingerprinting.Tests.Integration
{
    using System.Linq;

    using NUnit.Framework;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.InMemory;

    [TestFixture]
    public class InMemoryModelServiceTest : AbstractTest
    {
        private IAdvancedModelService modelService;

        [SetUp]
        public void SetUp()
        {
            modelService = new InMemoryModelService();
        }

        [Test]
        public void InsertTrackTest()
        {
            var track = new TrackInfo("id", "title", "artist", 200);

            var trackReference = modelService.Insert(track, new[] { new HashedFingerprint(GenericHashBuckets(), 0, 0f, Enumerable.Empty<string>()) });

            AssertModelReferenceIsInitialized(trackReference);
        }

        [Test]
        public void ReadTrackByTrackReferenceTest()
        {
            var track = new TrackInfo("id", "title", "artist", 200);

            var trackReference = modelService.Insert(track, new[] { new HashedFingerprint(GenericHashBuckets(), 0, 0f, Enumerable.Empty<string>()) });

            var first = modelService.ReadTrackByReference(trackReference);

            AssertTracksAreEqual(track, first);

            modelService.DeleteTrack(trackReference);

            var result = modelService.ReadTrackByReference(trackReference);

            Assert.IsTrue(result == null);
        }

        [Test]
        public void ReadTrackByArtistAndTitleTest()
        {
            var track = new TrackInfo("isrc", "title", "artist", 200);

            modelService.Insert(track, new[] { new HashedFingerprint(GenericHashBuckets(), 0, 0f, Enumerable.Empty<string>()) });

            var actualTracks = modelService.ReadTrackByTitle("title").ToList();

            Assert.IsTrue(actualTracks.Any());
            AssertTracksAreEqual(track, actualTracks[0]);
        }

        [Test]
        public void ReadMultipleTracksTest()
        {
            const int NumberOfTracks = 100;
            for (int i = 0; i < NumberOfTracks; i++)
            {
                var track = new TrackInfo($"isrc{i}", "title", "artist", 200);
                modelService.Insert(track, new[] { new HashedFingerprint(GenericHashBuckets(), 0, 0f, Enumerable.Empty<string>()) });
            }

            var actualTracks = modelService.ReadAllTracks().ToList();

            Assert.AreEqual(NumberOfTracks, actualTracks.Count);
        }

        [Test]
        public void DeleteTrackTest()
        {
            var track = new TrackInfo("isrc", "title", "artist", 200);
            var trackReference = modelService.Insert(track, new[] { new HashedFingerprint(GenericHashBuckets(), 0, 0f, Enumerable.Empty<string>()) });

            modelService.DeleteTrack(trackReference);

            var subFingerprints = modelService.ReadSubFingerprints(new[] { GenericHashBuckets() }, new DefaultQueryConfiguration())
                                              .ToList();

            Assert.IsFalse(subFingerprints.Any());
            var actualTrack = modelService.ReadTrackById("isrc");
            Assert.IsNull(actualTrack);
        }

        [Test]
        public void InsertHashDataTest()
        {
            var expectedTrack = new TrackInfo("isrc", "title", "artist", 200);
            var trackReference = modelService.Insert(expectedTrack, new[] { new HashedFingerprint(GenericHashBuckets(), 0, 0f, Enumerable.Empty<string>()) });

            var subFingerprints = modelService.ReadSubFingerprints(new[] { GenericHashBuckets() }, new DefaultQueryConfiguration())
                                              .ToList();

            Assert.AreEqual(1, subFingerprints.Count);
            Assert.AreEqual(trackReference, subFingerprints[0].TrackReference);
            Assert.AreNotEqual(0, subFingerprints[0].SubFingerprintReference.GetHashCode());
            CollectionAssert.AreEqual(GenericHashBuckets(), subFingerprints[0].Hashes);
        }

        [Test]
        public void ReadSubFingerprintsByHashBucketsHavingThresholdTest()
        {
            var t1 = new TrackInfo("isrc1", "title", "artist", 200);
            var t2 = new TrackInfo("isrc2", "title", "artist", 200);
            int[] firstTrackBuckets = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 };
            int[] secondTrackBuckets = { 2, 2, 4, 5, 6, 7, 7, 9, 10, 11, 12, 13, 14, 14, 16, 17, 18, 19, 20, 20, 22, 23, 24, 25, 26 };

            var firstHashData = new HashedFingerprint(firstTrackBuckets, 1, 0.928f, Enumerable.Empty<string>());
            var secondHashData = new HashedFingerprint(secondTrackBuckets, 1, 0.928f, Enumerable.Empty<string>());

            var firstTrackReference = modelService.Insert(t1, new[] { firstHashData });
            modelService.Insert(t2, new[] { secondHashData });

            // query buckets are similar with 5 elements from first track and 4 elements from second track
            int[] queryBuckets = { 3, 2, 5, 6, 7, 8, 7, 10, 11, 12, 13, 14, 15, 14, 17, 18, 19, 20, 21, 20, 23, 24, 25, 26, 25 };

            var subFingerprints = modelService.ReadSubFingerprints(new[] { queryBuckets }, new LowLatencyQueryConfiguration())
                                              .ToList();

            Assert.AreEqual(1, subFingerprints.Count);
            Assert.AreEqual(firstTrackReference, subFingerprints[0].TrackReference);
        }

        [Test]
        public void ReadSubFingerprintsByHashBucketsHavingThresholdWithClustersTest()
        {
            var firstTrack = new TrackInfo("isrc1", "title", "artist", 200);
            var secondTrack = new TrackInfo("isrc2", "title", "artist", 200);
            int[] firstTrackBuckets = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 };
            int[] secondTrackBuckets = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 };
            var firstHashData = new HashedFingerprint(firstTrackBuckets, 1, 0.928f, new[] { "first-group-id" });
            var secondHashData = new HashedFingerprint(secondTrackBuckets, 1, 0.928f, new[] { "second-group-id" });

            var firstTrackReference = modelService.Insert(firstTrack, new[] { firstHashData });
            modelService.Insert(secondTrack, new[] { secondHashData });

            // query buckets are similar with 5 elements from first track and 4 elements from second track
            int[] queryBuckets = { 3, 2, 5, 6, 7, 8, 7, 10, 11, 12, 13, 14, 15, 14, 17, 18, 19, 20, 21, 20, 23, 24, 25, 26, 25 };

            var subFingerprints = modelService.ReadSubFingerprints(new[] { queryBuckets }, new DefaultQueryConfiguration { Clusters = new[] { "first-group-id" } })
                                              .ToList();

            Assert.AreEqual(1, subFingerprints.Count);
            Assert.AreEqual(firstTrackReference, subFingerprints[0].TrackReference);
        }
    }
}