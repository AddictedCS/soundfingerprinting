namespace SoundFingerprinting.Tests.Integration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using NUnit.Framework;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.InMemory;

    [TestFixture]
    public class InMemoryModelServiceSerializationTest : IntegrationWithSampleFilesTest
    {
        [Test]
        public async Task ShouldSerializeAndDeserialize()
        {
            var modelService = new InMemoryModelService();

            var hashedFingerprints = await FingerprintCommandBuilder.Instance.BuildFingerprintCommand()
                .From(GetAudioSamples())
                .Hash();

            var trackData = new TrackInfo("id", "title", "artist", new Dictionary<string, string> {{"key", "value"}}, MediaType.Audio);

            modelService.Insert(trackData, hashedFingerprints);

            var tempDirectory = Path.Combine(Path.GetTempPath(), "sftests");
            modelService.Snapshot(tempDirectory);

            var (queryResult, _) = await QueryCommandBuilder.Instance.BuildQueryCommand()
                .From(GetAudioSamples())
                .UsingServices(new InMemoryModelService(tempDirectory))
                .Query();

            Directory.Delete(tempDirectory, true);

            Assert.That(queryResult, Is.Not.Null);
            Assert.That(queryResult.ContainsMatches, Is.True);
            AssertTracksAreEqual(trackData, queryResult.BestMatch!.Track);
            Assert.That(queryResult.BestMatch.Confidence > 0.9, Is.True);
       }

        [Test]
        public void ShouldSerializeAndIncrementNextIdCorrectly()
        {
            var modelService = new InMemoryModelService();

            var firstTrack = new TrackInfo("id1", "title", "artist");
            modelService.Insert(firstTrack, new AVHashes(new Hashes(new[] { new HashedFingerprint(GenericHashBuckets(), 1, 0f, Array.Empty<byte>()) }, 1.48, MediaType.Audio), null));

            var tempDirectory = Path.Combine(Path.GetTempPath(), "sftests");
            modelService.Snapshot(tempDirectory);

            var fromFileService = new InMemoryModelService(tempDirectory);

            var secondTrack = new TrackInfo("id2", "title", "artist");
            fromFileService.Insert(secondTrack, new AVHashes(new Hashes(new[] { new HashedFingerprint(GenericHashBuckets(), 1, 0f, Array.Empty<byte>()) }, 1.48, MediaType.Audio), null));

            var tracks = fromFileService.GetTrackIds().ToList();

            Directory.Delete(tempDirectory, true);

            Assert.That(tracks.Any(track => track == "id1", Is.True));
            Assert.That(tracks.Any(track => track == "id2", Is.True));
        }
    }
}