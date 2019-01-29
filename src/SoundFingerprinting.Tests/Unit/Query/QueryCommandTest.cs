namespace SoundFingerprinting.Tests.Unit.Query
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.InMemory;

    [TestFixture]
    public class QueryCommandTest
    {
        /**
         * Long queries, long matches
         * t     -----15-----25-----35-----45-----
         * query 000000111111100000011111110000000
         * track 000000111111100000011111110000000
         */
        [Test]
        public async Task ShouldIdentifyOnlyOneMatch()
        {
            float[] match = TestUtilities.GenerateRandomFloatArray(10 * 5512);

            float[] withJitter = AddJitter(match);

            var modelService = new InMemoryModelService();
            var audioService = new SoundFingerprintingAudioService();

            var hashes = await FingerprintCommandBuilder.Instance
                                    .BuildFingerprintCommand()
                                    .From(new AudioSamples(withJitter, "Queen", 5512))
                                    .UsingServices(audioService)
                                    .Hash();

            modelService.Insert(new TrackInfo("123", "Bohemian Rhapsody", "Queen", withJitter.Length / 5512f), hashes);

            var result = await QueryCommandBuilder.Instance
                                    .BuildQueryCommand()
                                    .From(new AudioSamples(withJitter, "cnn", 5512))
                                    .WithQueryConfig(config =>
                                    {
                                        config.AllowMultipleMatchesOfTheSameTrackInQuery = true;
                                        return config;
                                    })
                                    .UsingServices(modelService, audioService)
                                    .Query();
            
            Assert.IsTrue(result.ContainsMatches);
            var entries = result.ResultEntries.OrderBy(entry => entry.QueryMatchStartsAt).ToList();
            
            Assert.AreEqual(1, entries.Count);
            Assert.AreEqual(0d, entries[0].QueryMatchStartsAt, 1f);
            Assert.AreEqual(0d, entries[0].TrackMatchStartsAt, 1f);   
        }
        
        /**
         * Very long queries, short tracks
         * query  000000111110000000000000011111000000000000
         * track  11111    
         */
        [Test]
        public async Task ShouldIdentifyMultipleTracksInSameQuery()
        {
            float[] match = TestUtilities.GenerateRandomFloatArray(10 * 5512);

            float[] withJitter = AddJitter(match);

            var modelService = new InMemoryModelService();
            var audioService = new SoundFingerprintingAudioService();

            var hashes = await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(new AudioSamples(match, "Queen", 5512))
                .UsingServices(audioService)
                .Hash();

            modelService.Insert(new TrackInfo("123", "Bohemian Rhapsody", "Queen", withJitter.Length / 5512f), hashes);

            var result = await QueryCommandBuilder.Instance
                .BuildQueryCommand()
                .From(new AudioSamples(withJitter, "cnn", 5512))
                .WithQueryConfig(config =>
                {
                    config.AllowMultipleMatchesOfTheSameTrackInQuery = true;
                    return config;
                })
                .UsingServices(modelService, audioService)
                .Query();
            
            Assert.IsTrue(result.ContainsMatches);
            var entries = result.ResultEntries.OrderBy(entry => entry.QueryMatchStartsAt).ToList();
            
            Assert.AreEqual(2, entries.Count);
            Assert.AreEqual(15d, entries[0].QueryMatchStartsAt, 1f);
            Assert.AreEqual(35d, entries[1].QueryMatchStartsAt, 1f);
        }
        
        /**
         * Similar to chorus repetition
         * query      000000
         * stored 11110000002222200000
         */
        [Test]
        public async Task ShouldIdentifyMultipleRegionsOfTheSameMatch()
        {
            float[] match = TestUtilities.GenerateRandomFloatArray(10 * 5512);

            float[] withJitter = AddJitter(match);

            var modelService = new InMemoryModelService();
            var audioService = new SoundFingerprintingAudioService();

            var hashes = await FingerprintCommandBuilder.Instance
                                    .BuildFingerprintCommand()
                                    .From(new AudioSamples(withJitter, "Queen", 5512))
                                    .UsingServices(audioService)
                                    .Hash();

            modelService.Insert(new TrackInfo("123", "Bohemian Rhapsody", "Queen", withJitter.Length / 5512f), hashes);

            var result = await QueryCommandBuilder.Instance
                .BuildQueryCommand()
                .From(new AudioSamples(match, "cnn", 5512))
                .WithQueryConfig(config =>
                {
                    config.AllowMultipleMatchesOfTheSameTrackInQuery = true;
                    return config;
                })
                .UsingServices(modelService, audioService)
                .Query();
            
            Assert.IsTrue(result.ContainsMatches);
            var entries = result.ResultEntries.OrderBy(entry => entry.TrackMatchStartsAt).ToList();
            Assert.AreEqual(2, entries.Count);
            Assert.AreEqual(15d, entries[0].TrackMatchStartsAt, 1f);
            Assert.AreEqual(35d, entries[1].TrackMatchStartsAt, 1f);
        }

        private float[] AddJitter(float[] match)
        {
            float[] before = TestUtilities.GenerateRandomFloatArray(15 * 5512);
            float[] between = TestUtilities.GenerateRandomFloatArray(10 * 5512);
            float[] after = TestUtilities.GenerateRandomFloatArray(15 * 5512);
            float[] total = new float[before.Length + between.Length + after.Length + match.Length * 2];

            Buffer.BlockCopy(before, 0, total, 0, sizeof(float) * before.Length);
            Buffer.BlockCopy(match, 0, total, sizeof(float) * before.Length, sizeof(float) * match.Length);
            Buffer.BlockCopy(between, 0, total, sizeof(float) * (before.Length + match.Length), sizeof(float) * between.Length);
            Buffer.BlockCopy(match, 0, total, sizeof(float) * (before.Length + match.Length + between.Length),sizeof(float) * match.Length);
            Buffer.BlockCopy(after, 0, total, sizeof(float) * (before.Length + 2 * match.Length + between.Length),sizeof(float) * after.Length);
            return total;
        }
    }
}