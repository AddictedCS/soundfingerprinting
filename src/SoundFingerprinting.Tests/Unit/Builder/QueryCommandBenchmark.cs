namespace SoundFingerprinting.Tests.Unit.Builder
{
    using System;

    using NUnit.Framework;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.InMemory;

    [TestFixture]
    public class QueryCommandBenchmark
    {
        private readonly FingerprintCommandBuilder fcb = new FingerprintCommandBuilder();
        private readonly QueryCommandBuilder qcb = new QueryCommandBuilder();
        private readonly IAudioService audioService = new SoundFingerprintingAudioService();

        [Test]
        public void ShouldFingerprintAndQuerySuccessfully()
        {
            var modelService = new InMemoryModelService();

            for (int i = 0; i < 30; ++i)
            {
                var samples = new AudioSamples(GetRandomSamples(120), "${i}", 5512);
                var hashes = fcb.BuildFingerprintCommand()
                    .From(samples)
                    .UsingServices(audioService)
                    .Hash()
                    .Result;
                var track = new TrackData("${i}", "", "", "", 2017, 120);
                var trackReference = modelService.InsertTrack(track);
                modelService.InsertHashDataForTrack(hashes, trackReference);
            }

            Console.WriteLine("Fingerprinting Time, Query Time, Candidates Found");
            double avgFingerprinting = 0, avgQuery = 0;
            int totalRuns = 10;
            for (int i = 0; i < totalRuns; ++i)
            {
                var samples = new AudioSamples(GetRandomSamples(120), "${i}", 5512);
                var queryResult = qcb.BuildQueryCommand()
                    .From(samples)
                    .UsingServices(modelService, this.audioService)
                    .Query()
                    .Result;

                Console.WriteLine("{0,10}ms{1,15}ms{2,15}", queryResult.Stats.FingerprintingDuration, queryResult.Stats.QueryDuration, queryResult.Stats.TotalFingerprintsAnalyzed);
                avgFingerprinting += queryResult.Stats.FingerprintingDuration;
                avgQuery += queryResult.Stats.QueryDuration;
            }

            Console.WriteLine("Avg. Fingerprinting: {0,0:000}ms, Avg. Query: {1, 0:000}ms", avgFingerprinting / totalRuns, avgQuery/ totalRuns);
        }

        private float[] GetRandomSamples(int length)
        {
            int totalLength = length * 5512;
            var random = new Random();
            float[] samples = new float[totalLength];

            for (int i = 0; i < totalLength; ++i)
            {
                samples[i] = (float)random.NextDouble();
            }

            return samples;
        }
    }
}
