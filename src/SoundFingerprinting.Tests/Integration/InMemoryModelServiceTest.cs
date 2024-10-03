namespace SoundFingerprinting.Tests.Integration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using NUnit.Framework;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.InMemory;
    using SoundFingerprinting.Query;

    [TestFixture]
    public class InMemoryModelServiceTest : AbstractTest
    {
        private IModelService modelService;

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
            var candidates =  modelService.QueryEfficiently(audio, config);
            Assert.That(candidates.Count, Is.EqualTo(1));
            var audioResults = candidates.GetMatches().SelectMany(_ => _.Value).ToList();
            AssertHashesAreTheSame(audio, audioResults);
            
            candidates =  modelService.QueryEfficiently(video, config);
            Assert.That(candidates.Count, Is.EqualTo(1));
            var videoResults = candidates.GetMatches().SelectMany(_ => _.Value).ToList();
            AssertHashesAreTheSame(video, videoResults);
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

            var candidates = modelService.QueryEfficiently(GetGenericHashes(), new DefaultQueryConfiguration());
            Assert.IsTrue(candidates.IsEmpty);
            var actualTrack = modelService.ReadTrackById("id");
            Assert.IsNull(actualTrack);
        }

        [Test]
        public void InsertHashDataTest()
        {
            var expectedTrack = new TrackInfo("id", "title", "artist");
            var hashes = new Hashes([new HashedFingerprint(GenericHashBuckets(), 0, 0f, Array.Empty<byte>())], 1.48, MediaType.Audio);
            
            modelService.Insert(expectedTrack, new AVHashes(hashes, null));
            var candidates = modelService.QueryEfficiently(GetGenericHashes(MediaType.Audio), new DefaultQueryConfiguration());
            
            Assert.AreEqual(1, candidates.Count);
            AssertHashesAreTheSame(hashes, candidates.GetMatches().SelectMany(_ => _.Value));
            var references = modelService.ReadTracksByReferences(candidates.GetMatchedTracks()).ToList();
            Assert.AreEqual(1, references.Count);
            var trackReference = references.First().TrackReference;
            Assert.AreEqual(1, candidates.Count);
            Assert.AreEqual(trackReference, candidates.GetMatchedTracks().FirstOrDefault());
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

            var candidates = modelService.QueryEfficiently(queryHashes, new DefaultQueryConfiguration
            {
                ThresholdVotes = 5
            });

            Assert.AreEqual(1, candidates.Count);
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
            var queryHashes = new Hashes([new HashedFingerprint(queryBuckets, 0, 0f, Array.Empty<byte>())], 1.48d, MediaType.Audio);
            var candidates = modelService.QueryEfficiently(queryHashes, new DefaultQueryConfiguration
            {
                YesMetaFieldsFilters = new Dictionary<string, string> { { "group-id", "first-group-id" } }
            });

            Assert.AreEqual(1, candidates.Count);
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

            var candidates = modelService.QueryEfficiently(hashes, new DefaultQueryConfiguration());
            Assert.AreEqual(100, candidates.GetMatches().SelectMany(_ => _.Value).Count());
        }

        private static void AssertHashesAreTheSame(Hashes expected, IEnumerable<MatchedWith> actual)
        {
            var tuples = expected.Join(actual, _ => _.SequenceNumber, _ => _.QuerySequenceNumber, (a, b) => (a, b)).ToList();
            Assert.AreEqual(tuples.Count, expected.Count);
            foreach (var (first, second) in tuples)
            {
                Assert.AreEqual(first.StartsAt, second.QueryMatchAt);
                Assert.AreEqual(first.SequenceNumber, second.QuerySequenceNumber);
            } 
        }
    }
}