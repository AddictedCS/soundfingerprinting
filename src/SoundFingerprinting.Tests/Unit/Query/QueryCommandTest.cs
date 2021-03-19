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
    using SoundFingerprinting.Query;

    [TestFixture]
    public class QueryCommandTest
    {
        /**
         * query repeats consecutively two times in the track, without any pause
         * t     -----10-----
         * track 111111011111
         * query 111111
         */
        [Test]
        public async Task ShouldIdentifyConsecutiveRepeatingSequencesInTrack()
        {
            float[] match = TestUtilities.GenerateRandomFloatArray(10 * 5512, 123);
            float[] twoCopies = new float[match.Length * 2];
            match.CopyTo(twoCopies, 0);
            match.CopyTo(twoCopies, match.Length);

            var modelService = new InMemoryModelService();
            var audioService = new SoundFingerprintingAudioService();
            await InsertFingerprints(twoCopies, audioService, modelService);

            var result = await GetQueryResult(match, audioService, modelService, permittedGap: 0.5);

            Assert.AreEqual(2, result.ResultEntries.Count());
            foreach (var entry in result.ResultEntries)
            {
                Assert.AreEqual(1, entry.Confidence, 0.01);
                Assert.AreEqual(10, entry.TrackCoverageWithPermittedGapsLength, 1);
            }
        }

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

            await InsertFingerprints(withJitter, audioService, modelService);
            var result = await GetQueryResult(withJitter, audioService, modelService);
            
            Assert.IsTrue(result.ContainsMatches);
            var entries = result.ResultEntries.OrderBy(entry => entry.QueryMatchStartsAt).ToList();
            
            Assert.AreEqual(1, entries.Count);
            Assert.AreEqual(0d, entries[0].QueryMatchStartsAt, 1f);
            Assert.AreEqual(0d, entries[0].TrackMatchStartsAt, 1f);   
        }

        /**
         * Long queries, short matches within the same track
         * t     -----15-----25-----35-----45-----
         * query 000000000000011111100000000000000
         * track 000000111111100000011111110000000
         */
        [Test]
        public async Task ShouldIdentifyTwoSeparateMatchesDueToShiftBetweenThem()
        {
            float[] match = TestUtilities.GenerateRandomFloatArray(10 * 5512);
            float[] track = AddJitter(match, beforeSec: 10, betweenSec: 10, afterSec: 10);
            float[] query = AddJitter(match, beforeSec: 15, betweenSec: 0, afterSec: 15);

            var modelService = new InMemoryModelService();
            var audioService = new SoundFingerprintingAudioService();

            await InsertFingerprints(track, audioService, modelService);
            var result = await GetQueryResult(query, audioService, modelService);
            
            Assert.IsTrue(result.ContainsMatches);
            var entries = result.ResultEntries.OrderBy(entry => entry.TrackMatchStartsAt).ToList();
            
            Assert.AreEqual(2, entries.Count);
            Assert.AreEqual(15d, entries[0].QueryMatchStartsAt, 1f);
            Assert.AreEqual(15d, entries[1].QueryMatchStartsAt, 1f);
            Assert.AreEqual(10d, entries[0].TrackMatchStartsAt, 1f);   
            Assert.AreEqual(30d, entries[1].TrackMatchStartsAt, 1f);   
        }
        
        /**
         * Very long queries, short tracks
         * query  000000111110000000000000011111000000000000
         * track  11111    
         */
        [Test]
        public async Task ShouldIdentifyMultipleTracksInSameQuery()
        {
            float[] match = TestUtilities.GenerateRandomFloatArray(10 * 5512, 1);

            float[] withJitter = AddJitter(match, 15, 20);

            var modelService = new InMemoryModelService();
            var audioService = new SoundFingerprintingAudioService();

            await InsertFingerprints(match, audioService, modelService);

            var result = await GetQueryResult(withJitter, audioService, modelService);

            Assert.IsTrue(result.ContainsMatches);
            var entries = result.ResultEntries.OrderBy(entry => entry.QueryMatchStartsAt).ToList();
            Assert.AreEqual(2, entries.Count);
            Assert.AreEqual(15d, entries[0].QueryMatchStartsAt, 1f);
            Assert.AreEqual(45d, entries[1].QueryMatchStartsAt, 1f);
        }
        
        /**
         * Similar to chorus repetition
         * query      000000
         * stored 11110000002222200000
         */
        [Test]
        public async Task ShouldIdentifyMultipleRegionsOfTheSameMatch()
        {
            float[] match = TestUtilities.GenerateRandomFloatArray(10 * 5512, 1);

            float[] withJitter = AddJitter(match, 15, 20);

            var modelService = new InMemoryModelService();
            var audioService = new SoundFingerprintingAudioService();

            await InsertFingerprints(withJitter, audioService, modelService);

            var result = await GetQueryResult(match, audioService, modelService);
            
            Assert.IsTrue(result.ContainsMatches);
            Assert.AreEqual(2, result.ResultEntries.Count());
            var entries = result.ResultEntries.OrderBy(entry => entry.TrackMatchStartsAt).ToList();
            CollectionAssert.IsOrdered(entries[1]
                            .Coverage
                            .BestPath
                            .Select(_ => _.TrackSequenceNumber));
            CollectionAssert.IsOrdered(entries[1]
                .Coverage
                .BestPath
                .Select(_ => _.QuerySequenceNumber));
            
            Assert.AreEqual(2, entries.Count);
            Assert.AreEqual(10, entries[0].TrackCoverageWithPermittedGapsLength, 1f);
            Assert.AreEqual(10, entries[1].TrackCoverageWithPermittedGapsLength, 1f);
            Assert.AreEqual(15d, entries[0].TrackMatchStartsAt, 1f);
            Assert.AreEqual(45d, entries[1].TrackMatchStartsAt, 1f);
        }

        private static float[] AddJitter(float[] match, int beforeSec = 15, int betweenSec = 10, int afterSec = 15)
        {
            float[] before = TestUtilities.GenerateRandomFloatArray(beforeSec * 5512);
            float[] between = TestUtilities.GenerateRandomFloatArray(betweenSec * 5512);
            float[] after = TestUtilities.GenerateRandomFloatArray(afterSec * 5512);
            float[] total = new float[before.Length + between.Length + after.Length + match.Length * 2];

            Buffer.BlockCopy(before, 0, total, 0, sizeof(float) * before.Length);
            Buffer.BlockCopy(match, 0, total, sizeof(float) * before.Length, sizeof(float) * match.Length);
            if (betweenSec > 0)
            {
                Buffer.BlockCopy(between, 0, total, sizeof(float) * (before.Length + match.Length), sizeof(float) * between.Length);
                Buffer.BlockCopy(match, 0, total, sizeof(float) * (before.Length + match.Length + between.Length), sizeof(float) * match.Length);
            }

            Buffer.BlockCopy(after, 0, total, sizeof(float) * (before.Length + 2 * match.Length + between.Length), sizeof(float) * after.Length);
            return total;
        }

        private static async Task<QueryResult> GetQueryResult(float[] match, IAudioService audioService, IModelService modelService, double permittedGap = 2)
        {
            return await QueryCommandBuilder.Instance
                .BuildQueryCommand()
                .From(new AudioSamples(match, "cnn", 5512))
                .WithQueryConfig(config =>
                {
                    config.AllowMultipleMatchesOfTheSameTrackInQuery = true;
                    config.PermittedGap = permittedGap;
                    return config;
                })
                .UsingServices(modelService, audioService)
                .Query();
        }

        private static async Task InsertFingerprints(float[] audioSamples, IAudioService audioService, IModelService modelService)
        {
            var hashes = await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(new AudioSamples(audioSamples, "Queen", 5512))
                .UsingServices(audioService)
                .Hash();

            modelService.Insert(new TrackInfo("123", "Bohemian Rhapsody", "Queen"), new Hashes(hashes, audioSamples.Length / 5512f, MediaType.Audio, DateTime.Now, Enumerable.Empty<string>()));
        }
    }
}