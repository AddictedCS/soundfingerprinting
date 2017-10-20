namespace SoundFingerprinting.Tests.Integration
{
    using System;
    using System.IO;
    using System.Linq;

    using NUnit.Framework;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Audio.NAudio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.FFT;
    using SoundFingerprinting.InMemory;

    [TestFixture]
    public class InMemoryModelServiceSerializationTest : IntegrationWithSampleFilesTest
    {
        private readonly IFingerprintCommandBuilder fcb = new FingerprintCommandBuilder();
        private readonly IQueryCommandBuilder qcb = new QueryCommandBuilder();

        private readonly IAudioService audioService = new NAudioService();

        [Test]
        public void ShouldSerializeAndDeserialize()
        {
            var modelService = new InMemoryModelService();

            var hashedFingerprints = fcb.BuildFingerprintCommand()
                .From(GetAudioSamples())
                .UsingServices(audioService)
                .Hash()
                .Result;

            var trackData = new TrackData("isrc", "artist", "title", "album", 2017, 200);
            var trackReferences = modelService.InsertTrack(trackData);

            modelService.InsertHashDataForTrack(hashedFingerprints, trackReferences);

            var tempFile = Path.GetTempFileName();
            modelService.Snapshot(tempFile);

            var queryResult = qcb.BuildQueryCommand()
                .From(GetAudioSamples())
                .UsingServices(new InMemoryModelService(tempFile), audioService)
                .Query()
                .Result;

            File.Delete(tempFile);

            Assert.IsTrue(queryResult.ContainsMatches);
            AssertTracksAreEqual(trackData, queryResult.BestMatch.Track);
            Assert.IsTrue(queryResult.BestMatch.Confidence > 0.9);
       }

        [Test]
        public void ShouldSerializeAndIncrementNextIdCorrectly()
        {
            var modelService = new InMemoryModelService();

            var trackData = new TrackData("isrc", "artist", "title", "album", 2017, 200);
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
            var spectrumService = new SpectrumService();

            var spectrums = spectrumService.CreateLogSpectrogram(GetAudioSamples(), new DefaultSpectrogramConfig())
                .Select(spectrum =>
                {
                    float[] fullLength = new float[spectrum.Image.Length * spectrum.Image[0].Length];
                    for (int index = 0; index < spectrum.Image.Length; ++index)
                    {
                        Buffer.BlockCopy(spectrum.Image[index], 0,
                            fullLength,
                            index * spectrum.Image[index].Length * sizeof(float),
                            spectrum.Image[index].Length * sizeof(float));
                    }

                    return fullLength;
                })
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
