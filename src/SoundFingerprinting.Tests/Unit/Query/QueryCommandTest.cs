namespace SoundFingerprinting.Tests.Unit.Query
{
    using System;
    using System.IO;
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
        [Test]
        public void ShouldThrowOnInvalidMediaTypeMediaServiceConfiguration()
        {
            Assert.ThrowsAsync<ArgumentException>(() => QueryCommandBuilder.Instance
                .BuildQueryCommand()
                .From(Path.GetTempPath(), MediaType.Audio | MediaType.Video)
                .Query());
        }
        
        [Test]
        public void ShouldThrowArgumentNullExceptionSinceModelServiceIsNotSet()
        {
            Assert.ThrowsAsync<ArgumentException>(() => QueryCommandBuilder.Instance
                .BuildQueryCommand()
                .From(TestUtilities.GenerateRandomAudioSamples(10 * 5512))
                .Query()); 
        }
        
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

            var result = await GetQueryResult(match, modelService);

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
         * query      000000
         * stored 11110000002222200000
         */
        [Test(Description = "Chorus is identified multiple times")]
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
         * query  11110000003333300000
         * track  11110000002222200000
         */
        
        [Test(Description = "Query is a remix with a different 30 seconds piece (sec 60->90)")]
        public async Task ShouldIdentifyTrackRemixAndNotGenerateTwoMatches()
        {
            float[] track = TestUtilities.GenerateRandomFloatArray(120 * 5512, 1);
            float[] query = new float[track.Length];
            Buffer.BlockCopy(track, 0, query, 0, track.Length * sizeof(float));
            // query is different from second 60->90
            for (int i = 60 * 5512; i < 90 * 5512; ++i)
            {
                query[i] = Random.Shared.NextSingle();
            }
            
            var modelService = new InMemoryModelService();

            await InsertFingerprints(track, modelService);

            var result = await GetQueryResult(query, modelService);
            
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

        /**
         * query -xxx--xxx-
         * track -xxx--xxx-
         * c1(✓)  234 with 234
         * c2(✓)  789 with 789
         * c3(✗)  234 with 789
         * c4(✗)  789 with 234
         */
        [Test(Description = "Cross-matched coverages have to be removed as it may yield a negative query coverage")]
        public async Task ShouldRemoveCrossMatches()
        {
            float[] m1 = TestUtilities.GenerateRandomFloatArray(10 * 5512, seed: 1);
            float[] track = GetRandomSamplesWithRegions(m1, m1);
            Assert.AreEqual(40 * 5512, track.Length);
            float[] query = GetRandomSamplesWithRegions(m1, m1);
            Assert.AreEqual(40 * 5512, query.Length);
            
            var modelService = new InMemoryModelService();

            await InsertFingerprints(track, modelService);

            var singleMatch = await GetQueryResult(query, modelService);
            Assert.AreEqual(1, singleMatch.ResultEntries.Count());
            var coverage = singleMatch.ResultEntries.First().Coverage;
            Assert.AreEqual(5, coverage.TrackMatchStartsAt, 1, "TrackMatchStartsAt did not match");
            Assert.AreEqual(5, coverage.QueryMatchStartsAt, 1, "QueryMatchStartsAt did not match");
            Assert.AreEqual(30, coverage.TrackDiscreteCoverageLength, 1.5);
            Assert.AreEqual(20, coverage.TrackCoverageWithPermittedGapsLength, 2);
            Assert.AreEqual(20, coverage.TrackGapsCoverageLength, 2);
            Assert.AreEqual(30, coverage.QueryDiscreteCoverageLength, 1.5);
            Assert.AreEqual(20, coverage.QueryCoverageWithPermittedGapsLength, 2);
            Assert.AreEqual(20, coverage.QueryGapsCoverageLength, 2);
        }

         /**
         * query xxx
         * track ---xxx-----xxx---
          */
        [Test(Description = "For use-cases when query length is smaller than the distance between matches we can identify same match multiple times")]
        public async Task ShouldIdentifySameMatchTwiceQueryLengthIsSmall()
        {
            // 10 seconds
            float[] match = TestUtilities.GenerateRandomFloatArray(10 * 5512, 1);

            // 20 seconds -> 10 seconds match -> 30 seconds -> 10 seconds match.
            float[] withJitter = AddJitter(match, beforeSec: 20, betweenSec: 30);

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
            Assert.AreEqual(10, entries[0].Coverage.QueryCoverageWithPermittedGapsLength, 1f);
            Assert.AreEqual(10, entries[1].TrackCoverageWithPermittedGapsLength, 1f);
            Assert.AreEqual(10, entries[1].Coverage.QueryCoverageWithPermittedGapsLength, 1f);
            Assert.AreEqual(20d, entries[0].TrackMatchStartsAt, 1f);
            Assert.AreEqual(0d, entries[0].QueryMatchStartsAt, 1f);
            Assert.AreEqual(60d, entries[1].TrackMatchStartsAt, 1f); 
            Assert.AreEqual(0d, entries[1].QueryMatchStartsAt, 1f);
        }
         
        [Test(Description = "Should cross match tone signal")]
        public async Task ShouldBeAbleToCrossMatchToneSignal()
        {
            var first = TestUtilities.GenerateRandomAudioSamples(15 * 5512);
            var silenceGap = new AudioSamples(Enumerable.Repeat((float)0.5, 10 * 5512).ToArray(), string.Empty, 5512);
            var second = TestUtilities.GenerateRandomAudioSamples(15 * 5512);

            var samples = TestUtilities.Concatenate(TestUtilities.Concatenate(first, silenceGap), second);

            var avHashes = await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(samples)
                .WithFingerprintConfig(config =>
                {
                    config.Audio.TreatSilenceAsSignal = true;
                    return config;
                })
                .UsingServices(new SoundFingerprintingAudioService())
                .Hash();

            var modelService = new InMemoryModelService();
            
            modelService.Insert(new TrackInfo("id", "title", "artist"), avHashes);

            var query = await QueryCommandBuilder
                .Instance
                .BuildQueryCommand()
                .From(avHashes)
                .UsingServices(modelService)
                .Query();
            
            var result = query.ResultEntries.First();
            
            Assert.That(result, Is.Not.Null);
            var queryGaps = result.Audio!.Coverage.QueryGaps.ToList();
            var trackGaps = result.Audio.Coverage.TrackGaps.ToList();
            
            Assert.That(trackGaps, Is.Empty);
            Assert.That(queryGaps, Is.Empty);
            
            Assert.That(result.Audio.Confidence, Is.EqualTo(1).Within(0.1));
            Assert.That(result.Audio.TrackRelativeCoverage, Is.EqualTo(1).Within(0.1));
        }

        private static float[] GetRandomSamplesWithRegions(float[] m1, float[] m2)
        {
            float[] c1 = AddJitter(m1, beforeSec: 5, betweenSec: 0, afterSec: 5);
            float[] c2 = AddJitter(m2, beforeSec: 5, betweenSec: 0, afterSec: 5);
            float[] r1 = new float[c1.Length + c2.Length];
            Buffer.BlockCopy(c1, 0, r1, 0, c1.Length * sizeof(float));
            Buffer.BlockCopy(c2, 0, r1, c1.Length * sizeof(float), c2.Length * sizeof(float));
            return r1;
        }

        private static float[] AddJitter(float[] match, int beforeSec = 15, int betweenSec = 10, int afterSec = 15)
        {
            float[] before = TestUtilities.GenerateRandomFloatArray(beforeSec * 5512);
            float[] between = TestUtilities.GenerateRandomFloatArray(betweenSec * 5512);
            float[] after = TestUtilities.GenerateRandomFloatArray(afterSec * 5512);
            float[] total = new float[before.Length + between.Length + after.Length + match.Length * (betweenSec == 0 ? 1 : 2)];

            Buffer.BlockCopy(before, 0, total, 0, sizeof(float) * before.Length);
            Buffer.BlockCopy(match, 0, total, sizeof(float) * before.Length, sizeof(float) * match.Length);
            if (betweenSec > 0)
            {
                Buffer.BlockCopy(between, 0, total, sizeof(float) * (before.Length + match.Length), sizeof(float) * between.Length);
                Buffer.BlockCopy(match, 0, total, sizeof(float) * (before.Length + match.Length + between.Length), sizeof(float) * match.Length);
            }

            Buffer.BlockCopy(after, 0, total, sizeof(float) * (before.Length + (betweenSec == 0 ? 1 : 2) * match.Length + between.Length), sizeof(float) * after.Length);
            return total;
        }

        private static async Task<QueryResult> GetQueryResult(float[] match, IModelService modelService)
        {
            var avQueryResult = await QueryCommandBuilder.Instance
                .BuildQueryCommand()
                .From(new AudioSamples(match, "cnn", 5512))
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