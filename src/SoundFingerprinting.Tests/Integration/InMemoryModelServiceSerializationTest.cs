namespace SoundFingerprinting.Tests.Integration
{
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using NUnit.Framework;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
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

            var trackData = new TrackData("isrc", "artist", "title", "album", 2017, 200, ModelReference<object>.Null);
            var trackReferences = modelService.InsertTrack(trackData);

            modelService.InsertHashDataForTrack(hashedFingerprints, trackReferences);

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

            var trackData = new TrackData("isrc", "artist", "title", "album", 2017, 200, ModelReference<object>.Null);
            var trackReferences = modelService.InsertTrack(trackData);

            var tempFile = Path.GetTempFileName();
            modelService.Snapshot(tempFile);

            var fromFileService = new InMemoryModelService(tempFile);

            var newTrackReference = fromFileService.InsertTrack(trackData);

            File.Delete(tempFile);
            Assert.AreNotEqual(trackReferences, newTrackReference);
        }

        [Test]
        public void ShouldSerializeSpectralImages()
        {
            var spectrumService = new SpectrumService(new LomontFFT(), new LogUtility());

            var spectrums = spectrumService.CreateLogSpectrogram(GetAudioSamples(), new DefaultSpectrogramConfig())
                .Select(spectrum => spectrum.Image)
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
