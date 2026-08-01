namespace SoundFingerprinting.Tests.Integration
{
    using System.Threading.Tasks;
    using NUnit.Framework;
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.InMemory;

    [TestFixture]
    public class PlaybackSpeedQueryTest : IntegrationWithSampleFilesTest
    {
        [TestCase(8d)]
        [TestCase(-8d)]
        public async Task CompensatingPlaybackRateShouldRecoverOriginalTrack(double sourcePlaybackSpeedPercentage)
        {
            var modelService = new InMemoryModelService();
            var original = GetAudioSamples();
            var originalHashes = await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(original)
                .Hash();
            modelService.Insert(new TrackInfo("original", "Nocturne", "Chopin"), originalHashes);

            var pitched = Resample(original, 1d + (sourcePlaybackSpeedPercentage / 100d));
            var uncompensated = await Query(pitched, modelService);

            var recovered = await Query(pitched, sourcePlaybackSpeedPercentage, modelService);

            Assert.Multiple(() =>
            {
                Assert.That(uncompensated.ContainsMatches, Is.False);
                Assert.That(recovered.ContainsMatches, Is.True);
                Assert.That(recovered.BestMatch!.Track.Id, Is.EqualTo("original"));
                Assert.That(recovered.BestMatch.Confidence, Is.GreaterThan(0.8d));
            });
        }

        private static async Task<SoundFingerprinting.Query.QueryResult> Query(
            AudioSamples samples,
            InMemoryModelService modelService)
        {
            return await Query(samples, null, modelService);
        }

        private static async Task<SoundFingerprinting.Query.QueryResult> Query(
            AudioSamples samples,
            double? sourcePlaybackSpeedPercentage,
            InMemoryModelService modelService)
        {
            var source = QueryCommandBuilder.Instance.BuildQueryCommand();
            var configured = sourcePlaybackSpeedPercentage.HasValue
                ? source.From(samples, sourcePlaybackSpeedPercentage.Value)
                : source.From(samples);
            var result = await configured
                .UsingServices(modelService)
                .Query();
            return result.Audio!;
        }

        private static AudioSamples Resample(AudioSamples input, double playbackRate)
        {
            int outputLength = System.Math.Max(
                2,
                (int)System.Math.Floor((input.Samples.Length - 1) / playbackRate) + 1);
            float[] output = new float[outputLength];
            for (int outputIndex = 0; outputIndex < output.Length; outputIndex++)
            {
                double sourcePosition = outputIndex * playbackRate;
                int lowerIndex = System.Math.Min((int)sourcePosition, input.Samples.Length - 1);
                int upperIndex = System.Math.Min(lowerIndex + 1, input.Samples.Length - 1);
                double fraction = sourcePosition - lowerIndex;
                output[outputIndex] = (float)(
                    input.Samples[lowerIndex] +
                    ((input.Samples[upperIndex] - input.Samples[lowerIndex]) * fraction));
            }

            return new AudioSamples(output, input.Origin, input.SampleRate, input.RelativeTo, input.TimeOffset);
        }
    }
}
