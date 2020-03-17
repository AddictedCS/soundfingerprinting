namespace SoundFingerprinting.Tests.Unit.LCS
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using NUnit.Framework;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Query;

    [TestFixture]
    public class GroupedQueryResultsTest
    {
        [Test]
        public void ShouldAccumulateResults()
        {
            int runs = 1000;

            var groupedQueryResults = new GroupedQueryResults(5d, DateTime.Now);
            var references = new[] { 1, 2, 3, 4, 5 }.Select(id => new ModelReference<int>(id)).ToArray();

            Parallel.For(0, runs, i =>
            {
                var hashed = new HashedFingerprint(new int[0], (uint)i, i * 0.05f);
                var candidate = new SubFingerprintData(new int[0], (uint)i, i * 0.07f, new ModelReference<uint>((uint)i), references[i % references.Length]);
                groupedQueryResults.Add(hashed, candidate, i);
            });

            Assert.IsTrue(groupedQueryResults.ContainsMatches);
            for(int i = 0; i < references.Length; ++i)
            {
                int perTrack = runs / references.Length;
                int ham = (perTrack - 1) * runs / 2 + perTrack * i;
                Assert.AreEqual(ham, groupedQueryResults.GetScoreSumForTrack(references[i]));
            }

            var modelReferences = groupedQueryResults.GetTopTracksByScore(references.Length * 2).ToList();

            for (int i = 0; i < references.Length; ++i)
            {
                Assert.AreEqual(references[references.Length - i - 1], modelReferences[i]);
            }

            var bestMatch = groupedQueryResults.GetBestMatchForTrack(references.Last());

            Assert.AreEqual((runs - 1) * 0.05f, bestMatch.QueryMatchAt, 0.000001);
            Assert.AreEqual((runs - 1) * 0.07f, bestMatch.TrackMatchAt, 0.000001);

            for (int i = 0; i < references.Length; ++i)
            {
                var matchedWith = groupedQueryResults.GetMatchesForTrack(references[i]).ToList();
                Assert.AreEqual(runs / references.Length, matchedWith.Count);
            }
        }

        [Test]
        public void MatchesShouldBeOrderedByQueryAt()
        {
            int runs = 1000;

            var groupedQueryResults = new GroupedQueryResults(5d, DateTime.Now);
            var reference = new ModelReference<int>(1);

            Parallel.For(0, runs, i =>
            {
                var hashed = new HashedFingerprint(new int[0], (uint)i, i);
                var candidate = new SubFingerprintData(new int[0], (uint)i, runs - i, new ModelReference<uint>((uint)i), reference);
                groupedQueryResults.Add(hashed, candidate, i);
            });

            var matchedWith = groupedQueryResults.GetMatchesForTrack(reference);

            var ordered = matchedWith.Select(with => (int)with.QueryMatchAt).ToList();

            CollectionAssert.AreEqual(Enumerable.Range(0, runs), ordered);
        }

        [Test]
        public void SameQueryHashGeneratesMultipleTrackMatches()
        {
            var groupedQueryResults = new GroupedQueryResults(10d, DateTime.Now);

            var random = new Random(1);
            int runs = 100;
            int[] counts = new int[runs];
            var trackRef = new ModelReference<uint>(1);
            int k = 0;
            Parallel.For(0, runs, i =>
            {
                counts[i] = random.Next(5, 10);
                var queryPoint = new HashedFingerprint(new int[25], (uint)i, i * 1.48f);
                for (int j = 0; j < counts[i]; ++j)
                {
                    var dbPoint = new SubFingerprintData(new int[25], (uint)k, k * 0.01f, new ModelReference<uint>((uint)Interlocked.Increment(ref k)), trackRef);
                    groupedQueryResults.Add(queryPoint, dbPoint, i);
                }
            });

            var allMatches = groupedQueryResults.GetMatchesForTrack(trackRef).ToList();
            
            Assert.AreEqual(counts.Sum(), allMatches.Count);
            Assert.AreEqual(runs, allMatches.Select(m => m.QuerySequenceNumber).Distinct().Count());
        }
    }
}
