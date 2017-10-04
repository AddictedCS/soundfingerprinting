namespace SoundFingerprinting.Tests.Integration
{
    using System.IO;

    using NUnit.Framework;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Audio.NAudio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.DAO.Data;
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
    }
}
