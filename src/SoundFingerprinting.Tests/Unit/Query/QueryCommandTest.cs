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
            await InsertFingerprints(twoCopies, modelService);

            var result = await GetQueryResult(match, modelService, permittedGap: 0.5);

            Assert.AreEqual(2, result.ResultEntries.Count());
            foreach (var entry in result.ResultEntries)
            {
                Assert.AreEqual(1, entry.Confidence, 0.1);
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

            await InsertFingerprints(withJitter, modelService);
            var result = await GetQueryResult(withJitter, modelService);
            
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

            await InsertFingerprints(track, modelService);
            var result = await GetQueryResult(query, modelService);
            
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

            await InsertFingerprints(match, modelService);

            var result = await GetQueryResult(withJitter, modelService);

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

            await InsertFingerprints(withJitter, modelService);

            var result = await GetQueryResult(match, modelService);
            
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

        /**
         * Query is a remix with a different 30 seconds piece.
         * query  11110000003333300000
         * stored 11110000002222200000
         */
        
        [Test]
        public async Task ShouldIdentifyTrackRemixAndNotGenerateTwoMatches()
        {
            float[] track = TestUtilities.GenerateRandomFloatArray(120 * 5512, 1);
            float[] query = new float[track.Length];
            Buffer.BlockCopy(track, 0, query, 0, track.Length * sizeof(float));
            // query is different from second 60->90
            for (int i = 60 * 5512; i < 90 * 5512; ++i)
            {
                query[i] = 0;
            }
            
            var modelService = new InMemoryModelService();

            await InsertFingerprints(track, modelService);

            var result = await GetQueryResult(query, modelService, permittedGap: 5d, allowMultipleMatches: false);
            
            Assert.IsTrue(result.ContainsMatches);
            Assert.AreEqual(1, result.ResultEntries.Count());
            var entry = result.ResultEntries.First();
            Assert.AreEqual(1, entry.Coverage.QueryGaps.Count());
            Assert.AreEqual(1, entry.Coverage.TrackGaps.Count());
            double queryGap = entry.Coverage.QueryGaps.First().LengthInSeconds;
            double trackGap = entry.Coverage.TrackGaps.First().LengthInSeconds;
            Assert.AreEqual(30, queryGap, 0.5);
            Assert.AreEqual(30, trackGap, 0.5);
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

        private static async Task<QueryResult> GetQueryResult(float[] match, IModelService modelService, double permittedGap = 2, bool allowMultipleMatches = true)
        {
            var avQueryResult = await QueryCommandBuilder.Instance
                .BuildQueryCommand()
                .From(new AudioSamples(match, "cnn", 5512))
                .WithQueryConfig(config =>
                {
                    config.Audio.AllowMultipleMatchesOfTheSameTrackInQuery = allowMultipleMatches;
                    config.Audio.PermittedGap = permittedGap;
                    return config;
                })
                .UsingServices(modelService)
                .Query();
            return avQueryResult.Audio;
        }

        private static async Task InsertFingerprints(float[] audioSamples, IModelService modelService)
        {
            var hashes = await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(new AudioSamples(audioSamples, "Queen", 5512))
                .Hash();

            modelService.Insert(new TrackInfo("123", "Bohemian Rhapsody", "Queen"), hashes);
        }
    }
}