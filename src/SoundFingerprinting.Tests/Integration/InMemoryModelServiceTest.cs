namespace SoundFingerprinting.Tests.Integration
{
    using System;
    using System.Collections.Generic;
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
            var track = new TrackInfo("id", "title", "artist");

            modelService.Insert(track, new Hashes(new[] { new HashedFingerprint(GenericHashBuckets(), 0, 0f, Array.Empty<byte>()) }, 1.48, MediaType.Audio, DateTime.Now, Enumerable.Empty<string>()));

            Assert.IsNotNull(modelService.ReadTrackById("id"));
        }

        [Test]
        public void ReadTrackByTrackIdTest()
        {
            var track = new TrackInfo("id", "title", "artist");
            modelService.Insert(track, new Hashes(new[] { new HashedFingerprint(GenericHashBuckets(), 0, 0f, Array.Empty<byte>()) }, 1.48, MediaType.Audio, DateTime.Now, Enumerable.Empty<string>()));
            
            var first = modelService.ReadTrackById("id");
            AssertTracksAreEqual(track, first);

            modelService.DeleteTrack("id");
            var result = modelService.ReadTrackById("id");

            Assert.IsNull(result);
        }

        [Test]
        public void ReadTrackByArtistAndTitleTest()
        {
            var track = new TrackInfo("id", "title", "artist");

            modelService.Insert(track, new Hashes(new[] { new HashedFingerprint(GenericHashBuckets(), 0, 0f, Array.Empty<byte>()) }, 1.48d, MediaType.Audio, DateTime.Now, Enumerable.Empty<string>()));

            var actualTracks = modelService.ReadTrackByTitle("title").ToList();

            Assert.IsTrue(actualTracks.Any());
            AssertTracksAreEqual(track, actualTracks[0]);
        }

        [Test]
        public void ReadMultipleTracksTest()
        {
            const int numberOfTracks = 100;
            for (int i = 0; i < numberOfTracks; i++)
            {
                var track = new TrackInfo($"id{i}", "title", "artist");
                modelService.Insert(track, new Hashes(new[] { new HashedFingerprint(GenericHashBuckets(), 0, 0f, Array.Empty<byte>()) }, 1.48d, MediaType.Audio, DateTime.Now, Enumerable.Empty<string>()));
            }

            var actualTracks = modelService.GetTrackIds().ToList();

            Assert.AreEqual(numberOfTracks, actualTracks.Count);
        }

        [Test]
        public void DeleteTrackTest()
        {
            var track = new TrackInfo("id", "title", "artist");
            modelService.Insert(track, new Hashes(new[] { new HashedFingerprint(GenericHashBuckets(), 0, 0f, Array.Empty<byte>()) }, 1.48, MediaType.Audio, DateTime.Now, Enumerable.Empty<string>()));

            modelService.DeleteTrack("id");

            var subFingerprints = modelService.Query(GetGenericHashes(), new DefaultQueryConfiguration()).ToList();
            Assert.IsFalse(subFingerprints.Any());
            var actualTrack = modelService.ReadTrackById("id");
            Assert.IsNull(actualTrack);
        }

        [Test]
        public void InsertHashDataTest()
        {
            var expectedTrack = new TrackInfo("id", "title", "artist");
            modelService.Insert(expectedTrack, new Hashes(new[] { new HashedFingerprint(GenericHashBuckets(), 0, 0f, Array.Empty<byte>()) }, 1.48, MediaType.Audio, DateTime.Now, Enumerable.Empty<string>()));

            var subFingerprints = modelService.Query(GetGenericHashes(), new DefaultQueryConfiguration()).ToList();

            var trackReference = modelService.ReadTracksByReferences(subFingerprints.Select(s => s.TrackReference)).First().TrackReference;
            Assert.AreEqual(1, subFingerprints.Count);
            Assert.AreEqual(trackReference, subFingerprints[0].TrackReference);
            Assert.AreNotEqual(0, subFingerprints[0].SubFingerprintReference.GetHashCode());
            CollectionAssert.AreEqual(GenericHashBuckets(), subFingerprints[0].Hashes);
        }

        [Test]
        public void ReadSubFingerprintsByHashBucketsHavingThresholdTest()
        {
            var t1 = new TrackInfo("id1", "title", "artist");
            var t2 = new TrackInfo("id2", "title", "artist");
            int[] firstTrackBuckets = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 };
            int[] secondTrackBuckets = { 2, 2, 4, 5, 6, 7, 7, 9, 10, 11, 12, 13, 14, 14, 16, 17, 18, 19, 20, 20, 22, 23, 24, 25, 26 };

            var firstHashData = new HashedFingerprint(firstTrackBuckets, 1, 0.928f, Array.Empty<byte>());
            var secondHashData = new HashedFingerprint(secondTrackBuckets, 1, 0.928f, Array.Empty<byte>());

            modelService.Insert(t1, new Hashes(new[] { firstHashData }, 1.48d, MediaType.Audio, DateTime.Now, Enumerable.Empty<string>()));
            modelService.Insert(t2, new Hashes(new[] { secondHashData }, 1.48d, MediaType.Audio, DateTime.Now, Enumerable.Empty<string>()));

            // query buckets are similar with 5 elements from first track and 4 elements from second track
            int[] queryBuckets = { 3, 2, 5, 6, 7, 8, 7, 10, 11, 12, 13, 14, 15, 14, 17, 18, 19, 20, 21, 20, 23, 24, 25, 26, 25 };
            var queryHashes = new Hashes(new[]{new HashedFingerprint(queryBuckets, 0, 0f, Array.Empty<byte>()), }, 1.48d, MediaType.Audio, DateTime.Now, Enumerable.Empty<string>());

            var subFingerprints = modelService.Query(queryHashes, new LowLatencyQueryConfiguration()).ToList();

            Assert.AreEqual(1, subFingerprints.Count);
        }

        [Test]
        public void ReadSubFingerprintsByHashBucketsHavingThresholdWithClustersTest()
        {
            var firstTrack = new TrackInfo("id1", "title", "artist", new Dictionary<string, string>{{ "group-id", "first-group-id" }});
            var secondTrack = new TrackInfo("id2", "title", "artist", new Dictionary<string, string>{{ "group-id", "second-group-id" }});
            int[] firstTrackBuckets = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 };
            int[] secondTrackBuckets = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 };
            var firstHashData = new HashedFingerprint(firstTrackBuckets, 1, 0.928f, Array.Empty<byte>());
            var secondHashData = new HashedFingerprint(secondTrackBuckets, 1, 0.928f, Array.Empty<byte>());

            modelService.Insert(firstTrack, new Hashes(new[] { firstHashData }, 1.48d, MediaType.Audio, DateTime.Now, Enumerable.Empty<string>()));
            modelService.Insert(secondTrack, new Hashes(new[] { secondHashData }, 1.48d, MediaType.Audio, DateTime.Now, Enumerable.Empty<string>()));

            // query buckets are similar with 5 elements from first track and 4 elements from second track
            int[] queryBuckets = { 3, 2, 5, 6, 7, 8, 7, 10, 11, 12, 13, 14, 15, 14, 17, 18, 19, 20, 21, 20, 23, 24, 25, 26, 25 };
            var queryHashes = new Hashes(new[]{new HashedFingerprint(queryBuckets, 0, 0f, Array.Empty<byte>()), }, 1.48d, MediaType.Audio, DateTime.Now, Enumerable.Empty<string>());
            var subFingerprints = modelService.Query(queryHashes, 
                new DefaultQueryConfiguration 
                { 
                    YesMetaFieldsFilters = new Dictionary<string, string> {{"group-id", "first-group-id"}}
                }).ToList();

            Assert.AreEqual(1, subFingerprints.Count);
        }
    }
}