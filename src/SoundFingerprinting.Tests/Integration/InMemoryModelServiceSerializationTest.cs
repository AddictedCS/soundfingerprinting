namespace SoundFingerprinting.Tests.Integration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using NUnit.Framework;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.FFT;
    using SoundFingerprinting.InMemory;

    [TestFixture]
    public class InMemoryModelServiceSerializationTest : IntegrationWithSampleFilesTest
    {
        private readonly IAudioService audioService = new SoundFingerprintingAudioService();

        [Test]
        public async Task ShouldSerializeAndDeserialize()
        {
            var modelService = new InMemoryModelService();

            var hashedFingerprints = await FingerprintCommandBuilder.Instance.BuildFingerprintCommand()
                .From(GetAudioSamples())
                .UsingServices(audioService)
                .Hash();

            var trackData = new TrackInfo("id", "title", "artist", new Dictionary<string, string> {{"key", "value"}}, MediaType.Audio);

            modelService.Insert(trackData, hashedFingerprints);

            var tempFile = Path.GetTempFileName();
            modelService.Snapshot(tempFile);

            var queryResult = await QueryCommandBuilder.Instance.BuildQueryCommand()
                .From(GetAudioSamples())
                .UsingServices(new InMemoryModelService(tempFile), audioService)
                .Query();

            File.Delete(tempFile);

            Assert.IsTrue(queryResult.ContainsMatches);
            AssertTracksAreEqual(trackData, queryResult.BestMatch.Track);
            Assert.IsTrue(queryResult.BestMatch.Confidence > 0.9);
       }

        [Test]
        public void ShouldSerializeAndIncrementNextIdCorrectly()
        {
            var modelService = new InMemoryModelService();

            var firstTrack = new TrackInfo("id1", "title", "artist");
            modelService.Insert(firstTrack, new Hashes(new[] { new HashedFingerprint(GenericHashBuckets(), 1, 0f, Array.Empty<byte>()) }, 1.48, DateTime.Now, Enumerable.Empty<string>()));

            var tempFile = Path.GetTempFileName();
            modelService.Snapshot(tempFile);

            var fromFileService = new InMemoryModelService(tempFile);

            var secondTrack = new TrackInfo("id2", "title", "artist");
            fromFileService.Insert(secondTrack, new Hashes(new[] { new HashedFingerprint(GenericHashBuckets(), 1, 0f, Array.Empty<byte>()) }, 1.48, DateTime.Now, Enumerable.Empty<string>()));

            var tracks = fromFileService.GetTrackIds().ToList();

            File.Delete(tempFile);

            Assert.IsTrue(tracks.Any(track => track == "id1"));
            Assert.IsTrue(tracks.Any(track => track == "id2"));
        }

        [Test]
        public void ShouldSerializeSpectralImages()
        {
            var spectrumService = new SpectrumService(new LomontFFT(), new LogUtility());

            var spectrums = spectrumService.CreateLogSpectrogram(GetAudioSamples(), new DefaultSpectrogramConfig())
                .Select(spectrum => spectrum.ImageRowCols)
                .ToList();

            var modelService = new InMemoryModelService();

            var track = new TrackInfo("id", string.Empty, string.Empty);
            var hashes = new Hashes(GetGenericHashes(), 10);
            
            modelService.Insert(track, hashes);
            modelService.InsertSpectralImages(spectrums, "id");

            var tempFile = Path.GetTempFileName();
            modelService.Snapshot(tempFile);

            var fromFileService = new InMemoryModelService(tempFile);

            File.Delete(tempFile);

            var allSpectrums = fromFileService.GetSpectralImagesByTrackId("id").ToList();

            Assert.AreEqual(spectrums.Count, allSpectrums.Count);
        }
    }
}