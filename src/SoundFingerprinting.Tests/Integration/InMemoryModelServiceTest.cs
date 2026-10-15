namespace SoundFingerprinting.Tests.Integration
{
    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Linq;

    using NUnit.Framework;
    using SoundFingerprinting.Audio;
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
        public async Task InsertTrackTest()
        {
            var track = new TrackInfo("id", "title", "artist", mediaType: MediaType.Audio | MediaType.Video);
            var audio = TestUtilities.GetRandomHashes(120, MediaType.Audio);
            var video = TestUtilities.GetRandomHashes(120, MediaType.Video);
            var avHashes = new AVHashes(audio, video);

            await modelService.InsertAsync(track, avHashes);

            var readTrack = await modelService.ReadTrackByIdAsync(track.Id);
            AssertTracksAreEqual(track, readTrack);
            
            var (audioHashes, videoHashes) = await modelService.ReadHashesByTrackIdAsync("id");
			Assert.Multiple(() =>
			{
				Assert.That(audioHashes, Is.Not.Null);
				Assert.That(videoHashes, Is.Not.Null);
			});

			TestUtilities.AssertHashesAreTheSame(audio, audioHashes);
            TestUtilities.AssertHashesAreTheSame(video, videoHashes);

            var config = new DefaultQueryConfiguration();
            var candidates =  await modelService.QueryEfficientlyAsync(audio, config);
            Assert.That(candidates.Count, Is.EqualTo(1));
            var audioResults = candidates.GetMatches().SelectMany(_ => _.Value).ToList();
            AssertHashesAreTheSame(audio, audioResults);
            
            candidates =  await modelService.QueryEfficientlyAsync(video, config);
            Assert.That(candidates.Count, Is.EqualTo(1));
            var videoResults = candidates.GetMatches().SelectMany(_ => _.Value).ToList();
            AssertHashesAreTheSame(video, videoResults);
        }

        [Test]
        public async Task InsertAndReadAroundTripsSpectralProfile()
        {
            // hashes carry spectral profile in Properties; track is constructed without MetaFields
            const string encoded = "ENCODED-SPECTRAL-PROFILE";
            var audio = TestUtilities.GetRandomHashes(120, MediaType.Audio).WithProperty(SpectralProfileKeys.SpectralProfile, encoded);
            var track = new TrackInfo("rt-id", "title", "artist");

            await modelService.InsertAsync(track, new AVHashes(audio, null));

            // Insert path should propagate Hashes.Properties → TrackInfo.MetaFields
            var readTrack = await modelService.ReadTrackByIdAsync("rt-id");
            Assert.That(readTrack!.MetaFields.TryGetValue(SpectralProfileKeys.SpectralProfile, out var fromMeta), Is.True);
            Assert.That(fromMeta, Is.EqualTo(encoded));

            // Read path should propagate TrackInfo.MetaFields → Hashes.Properties even though
            // the stored hashes don't carry Properties on RAMStorage's freshly-rebuilt Hashes
            var (audioRead, _) = await modelService.ReadHashesByTrackIdAsync("rt-id");
            Assert.That(audioRead!.Properties.TryGetValue(SpectralProfileKeys.SpectralProfile, out var fromHashes), Is.True);
            Assert.That(fromHashes, Is.EqualTo(encoded));
        }

        [Test]
        public async Task ReadTrackByTrackIdTest()
        {
            var track = new TrackInfo("id", "title", "artist");
            await modelService.InsertAsync(track, AVHashes.Empty);
            
            var first = await modelService.ReadTrackByIdAsync("id");
            AssertTracksAreEqual(track, first);

            await modelService.DeleteTrackAsync("id");
            var result = await modelService.ReadTrackByIdAsync("id");

			Assert.That(result, Is.Null);
        }

        [Test]
        public async Task ReadMultipleTracksTest()
        {
            const int numberOfTracks = 100;
            for (int i = 0; i < numberOfTracks; i++)
            {
                var track = new TrackInfo($"id-{i}", "title", "artist");
                await modelService.InsertAsync(track, AVHashes.Empty);
            }

            var actualTracks = (await modelService.GetTrackIdsAsync()).ToList();

			Assert.That(actualTracks, Has.Count.EqualTo(numberOfTracks));
        }

        [Test]
        public async Task DeleteTrackTest()
        {
            var track = new TrackInfo("id", "title", "artist");
            await modelService.InsertAsync(track, AVHashes.Empty);

            await modelService.DeleteTrackAsync("id");

            var candidates = await modelService.QueryEfficientlyAsync(GetGenericHashes(), new DefaultQueryConfiguration());
			Assert.That(candidates.IsEmpty, Is.True);
            var actualTrack = await modelService.ReadTrackByIdAsync("id");
			Assert.That(actualTrack, Is.Null);
        }

        [Test]
        public async Task InsertHashDataTest()
        {
            var expectedTrack = new TrackInfo("id", "title", "artist");
            var hashes = new Hashes([new HashedFingerprint(GenericHashBuckets(), 0, 0f, Array.Empty<byte>())], 1.48, MediaType.Audio);
            
            await modelService.InsertAsync(expectedTrack, new AVHashes(hashes, null));
            var candidates = await modelService.QueryEfficientlyAsync(GetGenericHashes(MediaType.Audio), new DefaultQueryConfiguration());

			Assert.That(candidates.Count, Is.EqualTo(1));
            AssertHashesAreTheSame(hashes, candidates.GetMatches().SelectMany(_ => _.Value));
            var references = (await modelService.ReadTracksByReferencesAsync(candidates.GetMatchedTracks())).ToList();
			Assert.That(references, Has.Count.EqualTo(1));
            var trackReference = references.First().TrackReference;
			Assert.That(candidates.Count, Is.EqualTo(1));
			Assert.That(candidates.GetMatchedTracks().FirstOrDefault(), Is.EqualTo(trackReference));
        }

        [Test]
        public async Task ReadSubFingerprintsByHashBucketsHavingThresholdTest()
        {
            var t1 = new TrackInfo("id1", "title", "artist");
            var t2 = new TrackInfo("id2", "title", "artist");
            int[] firstTrackBuckets = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 };
            int[] secondTrackBuckets = { 2, 2, 4, 5, 6, 7, 7, 9, 10, 11, 12, 13, 14, 14, 16, 17, 18, 19, 20, 20, 22, 23, 24, 25, 26 };

            var firstHashData = new HashedFingerprint(firstTrackBuckets, 1, 0.928f, Array.Empty<byte>());
            var secondHashData = new HashedFingerprint(secondTrackBuckets, 1, 0.928f, Array.Empty<byte>());

            await modelService.InsertAsync(t1, new AVHashes(new Hashes(new[] { firstHashData }, 1.48d, MediaType.Audio), null));
            await modelService.InsertAsync(t2, new AVHashes(new Hashes(new[] { secondHashData }, 1.48d, MediaType.Audio), null));

            // query buckets are similar with 5 elements from first track and 4 elements from second track
            int[] queryBuckets = { 3, 2, 5, 6, 7, 8, 7, 10, 11, 12, 13, 14, 15, 14, 17, 18, 19, 20, 21, 20, 23, 24, 25, 26, 25 };
            var queryHashes = new Hashes(new[] { new HashedFingerprint(queryBuckets, 0, 0f, Array.Empty<byte>()), }, 1.48d, MediaType.Audio);

            var candidates = await modelService.QueryEfficientlyAsync(queryHashes, new DefaultQueryConfiguration
            {
                ThresholdVotes = 5
            });

			Assert.That(candidates.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task ReadSubFingerprintsByHashBucketsHavingThresholdWithClustersTest()
        {
            var firstTrack = new TrackInfo("id1", "title", "artist", new Dictionary<string, string>{{ "group-id", "first-group-id" }});
            var secondTrack = new TrackInfo("id2", "title", "artist", new Dictionary<string, string>{{ "group-id", "second-group-id" }});
            int[] firstTrackBuckets = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 };
            int[] secondTrackBuckets = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 };
            var firstHashData = new HashedFingerprint(firstTrackBuckets, 1, 0.928f, Array.Empty<byte>());
            var secondHashData = new HashedFingerprint(secondTrackBuckets, 1, 0.928f, Array.Empty<byte>());

            await modelService.InsertAsync(firstTrack, new AVHashes(new Hashes(new[] { firstHashData }, 1.48d, MediaType.Audio), null));
            await modelService.InsertAsync(secondTrack, new AVHashes(new Hashes(new[] { secondHashData }, 1.48d, MediaType.Audio), null));

            // query buckets are similar with 5 elements from first track and 4 elements from second track
            int[] queryBuckets = { 3, 2, 5, 6, 7, 8, 7, 10, 11, 12, 13, 14, 15, 14, 17, 18, 19, 20, 21, 20, 23, 24, 25, 26, 25 };
            var queryHashes = new Hashes([new HashedFingerprint(queryBuckets, 0, 0f, Array.Empty<byte>())], 1.48d, MediaType.Audio);
            var candidates = await modelService.QueryEfficientlyAsync(queryHashes, new DefaultQueryConfiguration
            {
                YesMetaFieldsFilters = new Dictionary<string, string> { { "group-id", "first-group-id" } }
            });

			Assert.That(candidates.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task ShouldUpdateTrackMetadata()
        {
            var track = new TrackInfo("id1", "title", "artist", new Dictionary<string, string>{{ "group-id", "first-group-id" }});
            var hashes = TestUtilities.GetRandomHashes(100, new Random(), true);

            await modelService.InsertAsync(track, new AVHashes(hashes, null));
            var oldTrack = await modelService.ReadTrackByIdAsync(track.Id);
			Assert.That(oldTrack, Is.Not.Null);

            var updateTrack = new TrackInfo(track.Id, "new_title", "new_artist", new Dictionary<string, string> {{"group-id", "second-group-id"}});
            await modelService.UpdateTrackAsync(updateTrack);

            var newTrack = await modelService.ReadTrackByIdAsync(track.Id);
			Assert.That(newTrack, Is.Not.Null);
			Assert.Multiple(() =>
			{
				Assert.That(newTrack.Title, Is.EqualTo("new_title"));
				Assert.That(newTrack.Artist, Is.EqualTo("new_artist"));
				Assert.That(newTrack.MetaFields["group-id"], Is.EqualTo("second-group-id"));
			});

			var candidates = await modelService.QueryEfficientlyAsync(hashes, new DefaultQueryConfiguration());
			Assert.That(candidates.GetMatches().SelectMany(_ => _.Value).Count(), Is.EqualTo(100));
        }

        private static void AssertHashesAreTheSame(Hashes expected, IEnumerable<MatchedWith> actual)
        {
            var tuples = expected.Join(actual, _ => _.SequenceNumber, _ => _.QuerySequenceNumber, (a, b) => (a, b)).ToList();
			Assert.That(expected, Has.Count.EqualTo(tuples.Count));
            foreach (var (first, second) in tuples)
            {
				Assert.Multiple(() =>
				{
					Assert.That(second.QueryMatchAt, Is.EqualTo(first.StartsAt));
					Assert.That(second.QuerySequenceNumber, Is.EqualTo(first.SequenceNumber));
				});
			} 
        }
    }
}