namespace SoundFingerprinting.Tests.Integration
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Audio;
    using Audio.NAudio;
    using Builder;
    using DAO.Data;
    using InMemory;

    using NUnit.Framework;

    [TestFixture]
    public class FingerprintCommandBuilderNativeIntTest : IntegrationWithSampleFilesTest
    {
        private readonly IModelService modelService = new InMemoryModelService();
        private readonly IFingerprintCommandBuilder fingerprintCommandBuilder = new FingerprintCommandBuilder();
        private readonly IQueryCommandBuilder queryCommandBuilder = new QueryCommandBuilder();
        private readonly IAudioService audioService = new NAudioService();

        [Test]
        public void ShouldCreateFingerprintsFromAudioSamplesQueryAndGetTheRightResult()
        {
            const int SecondsToProcess = 10;
            const int StartAtSecond = 30;
            var audioSamples = GetAudioSamples();
            var track = new TrackData(string.Empty, audioSamples.Origin, audioSamples.Origin, string.Empty, 1986, audioSamples.Duration);
            var trackReference = modelService.InsertTrack(track);
            var hashDatas = fingerprintCommandBuilder.BuildFingerprintCommand()
                    .From(audioSamples)
                    .UsingServices(audioService)
                    .Hash()
                    .Result;

            modelService.InsertHashDataForTrack(hashDatas, trackReference);

            var querySamples = GetQuerySamples(GetAudioSamples(), StartAtSecond, SecondsToProcess);

            var queryResult = queryCommandBuilder.BuildQueryCommand()
                    .From(new AudioSamples(querySamples, string.Empty, audioSamples.SampleRate))
                    .UsingServices(modelService, audioService)
                    .Query()
                    .Result;

            Assert.IsTrue(queryResult.ContainsMatches);
            Assert.AreEqual(1, queryResult.ResultEntries.Count());
            var bestMatch = queryResult.BestMatch;
            Assert.AreEqual(trackReference, bestMatch.Track.TrackReference);
            Assert.IsTrue(bestMatch.QueryMatchLength > SecondsToProcess - 3, $"QueryMatchLength:{bestMatch.QueryLength}");
            Assert.AreEqual(StartAtSecond, Math.Abs(bestMatch.TrackStartsAt), 0.1d);
            Assert.IsTrue(bestMatch.Confidence > 0.7, $"Confidence:{bestMatch.Confidence}");
        }

        [Test]
        public async Task ShouldCreateSameFingerprintsDuringDifferentParallelRuns()
        {
            var hashDatas1 = await fingerprintCommandBuilder.BuildFingerprintCommand()
                    .From(GetAudioSamples())
                    .UsingServices(audioService)
                    .Hash();

            var hashDatas2 = await fingerprintCommandBuilder.BuildFingerprintCommand()
                .From(GetAudioSamples())
                .UsingServices(audioService)
                .Hash();

            var hashDatas3 = await fingerprintCommandBuilder.BuildFingerprintCommand()
                .From(GetAudioSamples())
                .UsingServices(audioService)
                .Hash();

            var hashDatas4 = await fingerprintCommandBuilder.BuildFingerprintCommand()
                .From(GetAudioSamples())
                .UsingServices(audioService)
                .Hash();

            AssertHashDatasAreTheSame(hashDatas1, hashDatas2);
            AssertHashDatasAreTheSame(hashDatas2, hashDatas3);
            AssertHashDatasAreTheSame(hashDatas3, hashDatas4);
        }

        private static float[] GetQuerySamples(AudioSamples audioSamples, int startAtSecond, int secondsToProcess)
        {
            int sampleRate = audioSamples.SampleRate;
            float[] querySamples = new float[sampleRate * secondsToProcess];
            int startAt = startAtSecond * sampleRate;
            Array.Copy(audioSamples.Samples, startAt, querySamples, 0, querySamples.Length);
            return querySamples;
        }
    }
}
