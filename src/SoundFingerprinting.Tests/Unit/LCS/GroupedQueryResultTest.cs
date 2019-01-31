﻿namespace SoundFingerprinting.Tests.Unit.LCS
{
    using System.Linq;
    using System.Threading.Tasks;

    using NUnit.Framework;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Query;

    [TestFixture]
    public class GroupedQueryResultTest
    {
        [Test]
        public void ShouldAccumulateResults()
        {
            int runs = 1000;

            var groupedQueryResults = new GroupedQueryResults(5d);
            var references = new[] { 1, 2, 3, 4, 5 }.Select(id => new ModelReference<int>(id)).ToArray();

            Parallel.For(0, runs, i =>
            {
                var hashed = new HashedFingerprint(new int[0], (uint)i, i * 0.05f, new string[0]);
                var candidate = new SubFingerprintData(new int[0], (uint)i, i * 0.07f, Enumerable.Empty<string>(), new ModelReference<uint>((uint)i), references[i % references.Length]);
                groupedQueryResults.Add(hashed, candidate, i);
            });

            Assert.IsTrue(groupedQueryResults.ContainsMatches);
            for(int i = 0; i < references.Length; ++i)
            {
                int perTrack = runs / references.Length;
                int ham = (perTrack - 1) * runs / 2 + perTrack * i;
                Assert.AreEqual(ham, groupedQueryResults.GetHammingSimilaritySumForTrack(references[i]));
            }

            var modelReferences = groupedQueryResults.GetTopTracksByHammingSimilarity(references.Length * 2).ToList();

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

            var groupedQueryResults = new GroupedQueryResults(5d);
            var reference = new ModelReference<int>(1);

            Parallel.For(0, runs, i =>
            {
                var hashed = new HashedFingerprint(new int[0], (uint)i, i, new string[0]);
                var candidate = new SubFingerprintData(new int[0], (uint)i, runs - i, Enumerable.Empty<string>(), new ModelReference<uint>((uint)i), reference);
                groupedQueryResults.Add(hashed, candidate, i);
            });

            var matchedWith = groupedQueryResults.GetMatchesForTrack(reference);

            var ordered = matchedWith.Select(with => (int)with.QueryMatchAt).ToList();

            CollectionAssert.AreEqual(Enumerable.Range(0, runs), ordered);
        }
    }
}
