namespace SoundFingerprinting.Tests.Unit.Builder
{
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using NLog.Extensions.Logging;
    using NUnit.Framework;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.InMemory;

    [TestFixture]
    public class QueryCommandBenchmark
    {
        private readonly ILogger<QueryCommandBenchmark> logger = new NLogLoggerFactory().CreateLogger<QueryCommandBenchmark>();
        
        [Test]
        public async Task ShouldFingerprintAndQuerySuccessfully()
        {
            var modelService = new InMemoryModelService();

            for (int i = 0; i < 30; ++i)
            {
                var samples = new AudioSamples(TestUtilities.GenerateRandomFloatArray(120 * 5512), "${i}", 5512);
                var hashes = await FingerprintCommandBuilder.Instance
                    .BuildFingerprintCommand()
                    .From(samples)
                    .Hash();

                var track = new TrackInfo($"{i}", string.Empty, string.Empty);

                modelService.Insert(track, hashes);
            }

            logger.LogInformation("Fingerprinting Time, Query Time, Candidates Found");
            double avgFingerprinting = 0, avgQuery = 0;
            int totalRuns = 10;
            for (int i = 0; i < totalRuns; ++i)
            {
                var samples = new AudioSamples(TestUtilities.GenerateRandomFloatArray(120 * 5512), "${i}", 5512);
                var (queryResult, _) = await QueryCommandBuilder.Instance
                    .BuildQueryCommand()
                    .From(samples)
                    .UsingServices(modelService)
                    .Query();

                logger.LogInformation("{0,10}ms{1,15}ms{2,15}", queryResult.CommandStats.FingerprintingDurationMilliseconds, queryResult.CommandStats.QueryDurationMilliseconds, queryResult.CommandStats.TotalFingerprintsAnalyzed);
                avgFingerprinting += queryResult.CommandStats.FingerprintingDurationMilliseconds;
                avgQuery += queryResult.CommandStats.QueryDurationMilliseconds;
            }

            logger.LogInformation("Avg. Fingerprinting: {0,0:000}ms, Avg. Query: {1, 0:000}ms", avgFingerprinting / totalRuns, avgQuery / totalRuns);
        }
    }
}
