namespace SoundFingerprinting.Tests.Integration
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using NUnit.Framework;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
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

            var trackData = new TrackInfo("id", "title", "artist", 200, new Dictionary<string, string> {{"key", "value"}});

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

            var firstTrack = new TrackInfo("id1", "title", "artist", 200);
            var ref1 = modelService.Insert(firstTrack, new[] { new HashedFingerprint(GenericHashBuckets(), 1, 0f, Enumerable.Empty<string>()) });

            var tempFile = Path.GetTempFileName();
            modelService.Snapshot(tempFile);

            var fromFileService = new InMemoryModelService(tempFile);

            var secondTrack = new TrackInfo("id2", "title", "artist", 200);
            var ref2 = fromFileService.Insert(secondTrack, new[] { new HashedFingerprint(GenericHashBuckets(), 1, 0f, Enumerable.Empty<string>()) });

            var tracks = fromFileService.ReadAllTracks().ToList();

            File.Delete(tempFile);

            Assert.IsTrue(tracks.Any(track => track.ISRC == "id1"));
            Assert.IsTrue(tracks.Any(track => track.ISRC == "id2"));
            Assert.IsTrue(!ref1.Equals(ref2));
        }

        [Test]
        public void ShouldSerializeSpectralImages()
        {
            var spectrumService = new SpectrumService(new LomontFFT(), new LogUtility());

            var spectrums = spectrumService.CreateLogSpectrogram(GetAudioSamples(), new DefaultSpectrogramConfig())
                .Select(spectrum => spectrum.ImageRowCols)
                .ToList();

            var modelService = new InMemoryModelService();
            var trackReference = new ModelReference<int>(10);
            modelService.InsertSpectralImages(spectrums, trackReference);

            var tempFile = Path.GetTempFileName();
            modelService.Snapshot(tempFile);

            var fromFileService = new InMemoryModelService(tempFile);

            File.Delete(tempFile);

            var allSpectrums = fromFileService.GetSpectralImagesByTrackReference(trackReference).ToList();

            Assert.AreEqual(spectrums.Count, allSpectrums.Count);
        }
    }
}