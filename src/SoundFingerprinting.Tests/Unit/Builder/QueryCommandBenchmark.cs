namespace SoundFingerprinting.Tests.Unit.Builder
{
    using System;
    using System.Threading.Tasks;
    using NUnit.Framework;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.InMemory;

    [TestFixture]
    public class QueryCommandBenchmark
    {
        private readonly IAudioService audioService = new SoundFingerprintingAudioService();

        [Test]
        public async Task ShouldFingerprintAndQuerySuccessfully()
        {
            var modelService = new InMemoryModelService();

            for (int i = 0; i < 30; ++i)
            {
                var samples = new AudioSamples(TestUtilities.GenerateRandomFloatArray(120 * 5512), "${i}", 5512);
                var hashes = await FingerprintCommandBuilder.Instance.BuildFingerprintCommand()
                    .From(samples)
                    .UsingServices(audioService)
                    .Hash();

                var track = new TrackInfo($"{i}", string.Empty, string.Empty);

                modelService.Insert(track, hashes);
            }

            Console.WriteLine("Fingerprinting Time, Query Time, Candidates Found");
            double avgFingerprinting = 0, avgQuery = 0;
            int totalRuns = 10;
            for (int i = 0; i < totalRuns; ++i)
            {
                var samples = new AudioSamples(TestUtilities.GenerateRandomFloatArray(120 * 5512), "${i}", 5512);
                var queryResult = await QueryCommandBuilder.Instance.BuildQueryCommand()
                    .From(samples)
                    .UsingServices(modelService, audioService)
                    .Query();

                Console.WriteLine("{0,10}ms{1,15}ms{2,15}", queryResult.Stats.FingerprintingDuration, queryResult.Stats.QueryDuration, queryResult.Stats.TotalFingerprintsAnalyzed);
                avgFingerprinting += queryResult.Stats.FingerprintingDuration;
                avgQuery += queryResult.Stats.QueryDuration;
            }

            Console.WriteLine("Avg. Fingerprinting: {0,0:000}ms, Avg. Query: {1, 0:000}ms", avgFingerprinting / totalRuns, avgQuery / totalRuns);
        }
    }
}
