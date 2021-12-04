namespace SoundFingerprinting.Tests.Integration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using NUnit.Framework;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO.Data;
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
            var track = new TrackInfo("id", "title", "artist", mediaType: MediaType.Audio | MediaType.Video);
            var audio = TestUtilities.GetRandomHashes(120, MediaType.Audio);
            var video = TestUtilities.GetRandomHashes(120, MediaType.Video);
            var avHashes = new AVHashes(audio, video);

            modelService.Insert(track, avHashes);

            var readTrack = modelService.ReadTrackById(track.Id);
            AssertTracksAreEqual(track, readTrack);
            
            var (audioHashes, videoHashes) = modelService.ReadHashesByTrackId("id");
            Assert.IsNotNull(audioHashes);
            Assert.IsNotNull(videoHashes);

            TestUtilities.AssertHashesAreTheSame(audio, audioHashes);
            TestUtilities.AssertHashesAreTheSame(video, videoHashes);

            var config = new DefaultQueryConfiguration();
            var audioResults =  modelService.Query(audio, config).ToList();
            AssertHashesAreTheSame(audio, audioResults);
            var videoResults = modelService.Query(video, config).ToList();
            AssertHashesAreTheSame(video, videoResults);
            AssertHashesAreNotTheSame(audioResults, videoResults);
        }

        [Test]
        public void ReadTrackByTrackIdTest()
        {
            var track = new TrackInfo("id", "title", "artist");
            modelService.Insert(track, AVHashes.Empty);
            
            var first = modelService.ReadTrackById("id");
            AssertTracksAreEqual(track, first);

            modelService.DeleteTrack("id");
            var result = modelService.ReadTrackById("id");

            Assert.IsNull(result);
        }

        [Test]
        public void ReadMultipleTracksTest()
        {
            const int numberOfTracks = 100;
            for (int i = 0; i < numberOfTracks; i++)
            {
                var track = new TrackInfo($"id-{i}", "title", "artist");
                modelService.Insert(track, AVHashes.Empty);
            }

            var actualTracks = modelService.GetTrackIds().ToList();

            Assert.AreEqual(numberOfTracks, actualTracks.Count);
        }

        [Test]
        public void DeleteTrackTest()
        {
            var track = new TrackInfo("id", "title", "artist");
            modelService.Insert(track, AVHashes.Empty);

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
            var hashes = new Hashes(new[] { new HashedFingerprint(GenericHashBuckets(), 0, 0f, Array.Empty<byte>()) }, 1.48, MediaType.Audio);
            modelService.Insert(expectedTrack, new AVHashes(hashes, null));

            var subFingerprints = modelService.Query(GetGenericHashes(MediaType.Audio), new DefaultQueryConfiguration()).ToList();
            AssertHashesAreTheSame(hashes, subFingerprints);
            var references = modelService.ReadTracksByReferences(subFingerprints.Select(s => s.TrackReference)).ToList();
            Assert.AreEqual(1, references.Count);
            var trackReference = references.First().TrackReference;
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

            modelService.Insert(t1, new AVHashes(new Hashes(new[] { firstHashData }, 1.48d, MediaType.Audio), null));
            modelService.Insert(t2, new AVHashes(new Hashes(new[] { secondHashData }, 1.48d, MediaType.Audio), null));

            // query buckets are similar with 5 elements from first track and 4 elements from second track
            int[] queryBuckets = { 3, 2, 5, 6, 7, 8, 7, 10, 11, 12, 13, 14, 15, 14, 17, 18, 19, 20, 21, 20, 23, 24, 25, 26, 25 };
            var queryHashes = new Hashes(new[] { new HashedFingerprint(queryBuckets, 0, 0f, Array.Empty<byte>()), }, 1.48d, MediaType.Audio);

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

            modelService.Insert(firstTrack, new AVHashes(new Hashes(new[] { firstHashData }, 1.48d, MediaType.Audio), null));
            modelService.Insert(secondTrack, new AVHashes(new Hashes(new[] { secondHashData }, 1.48d, MediaType.Audio), null));

            // query buckets are similar with 5 elements from first track and 4 elements from second track
            int[] queryBuckets = { 3, 2, 5, 6, 7, 8, 7, 10, 11, 12, 13, 14, 15, 14, 17, 18, 19, 20, 21, 20, 23, 24, 25, 26, 25 };
            var queryHashes = new Hashes(new[]{new HashedFingerprint(queryBuckets, 0, 0f, Array.Empty<byte>()), }, 1.48d, MediaType.Audio);
            var subFingerprints = modelService.Query(queryHashes, new DefaultQueryConfiguration
            {
                YesMetaFieldsFilters = new Dictionary<string, string> { { "group-id", "first-group-id" } }
            }).ToList();

            Assert.AreEqual(1, subFingerprints.Count);
        }

        [Test]
        public void ShouldUpdateTrackMetadata()
        {
            var track = new TrackInfo("id1", "title", "artist", new Dictionary<string, string>{{ "group-id", "first-group-id" }});
            var hashes = TestUtilities.GetRandomHashes(100, new Random(), true);

            modelService.Insert(track, new AVHashes(hashes, null));
            var oldTrack = modelService.ReadTrackById(track.Id);
            Assert.IsNotNull(oldTrack);

            var updateTrack = new TrackInfo(track.Id, "new_title", "new_artist", new Dictionary<string, string> {{"group-id", "second-group-id"}});
            modelService.UpdateTrack(updateTrack);

            var newTrack = modelService.ReadTrackById(track.Id);
            Assert.IsNotNull(newTrack);
            Assert.AreEqual("new_title", newTrack.Title);
            Assert.AreEqual("new_artist", newTrack.Artist);
            Assert.AreEqual("second-group-id", newTrack.MetaFields["group-id"]);

            var result = modelService.Query(hashes, new DefaultQueryConfiguration());
            Assert.AreEqual(100, result.Count());
        }
        
        private static void AssertHashesAreNotTheSame(IEnumerable<SubFingerprintData> left, IEnumerable<SubFingerprintData> right)
        {
            var fingerprints = left as SubFingerprintData[] ?? left.ToArray();
            var tuples = fingerprints.Join(right, _ => _.SequenceNumber, _ => _.SequenceNumber, (a, b) => (a, b)).ToList();
            Assert.AreEqual(tuples.Count, fingerprints.Length);
            foreach (var (first, second) in tuples)
            {
                CollectionAssert.AreNotEqual(first.Hashes, second.Hashes);
            } 
        }

        private static void AssertHashesAreTheSame(Hashes expected, IEnumerable<SubFingerprintData> actual)
        {
            var tuples = expected.Join(actual, _ => _.SequenceNumber, _ => _.SequenceNumber, (a, b) => (a, b)).ToList();
            Assert.AreEqual(tuples.Count, expected.Count);
            foreach (var (first, second) in tuples)
            {
                Assert.AreEqual(first.StartsAt, second.SequenceAt);
                Assert.AreEqual(first.SequenceNumber, second.SequenceNumber);
                CollectionAssert.AreEqual(first.HashBins, second.Hashes);
            } 
        }
    }
}