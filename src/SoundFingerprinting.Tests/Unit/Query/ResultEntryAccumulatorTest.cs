using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoundFingerprinting.Tests.Unit.Query
{
    using System.Collections.Concurrent;

    using NUnit.Framework;

    using SoundFingerprinting.Data;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Math;
    using SoundFingerprinting.Query;

    [TestFixture]
    public class ResultEntryAccumulatorTest
    {
        private readonly ISimilarityUtility similarityUtility = new SimilarityUtility();

        [Test]
        public void ShouldCalculateSimilaritiesCorrectly()
        {
            var hammingSimilarities = new ConcurrentDictionary<IModelReference, ResultEntryAccumulator>();
            var totalCount = 10000;
            var countPerTrack = 10;
            var tracksCount = totalCount / countPerTrack;
            var subFingerprints = GetSubFingerprints(totalCount, countPerTrack);
            var queryFingerprint = new HashedFingerprint(new byte[100], new long[25], 0,0, Enumerable.Empty<string>());

            Parallel.ForEach(subFingerprints, subFingerprint => { 
                var toCompare = new List<SubFingerprintData>(new[] { subFingerprint });
                similarityUtility.AccumulateHammingSimilarity(toCompare, queryFingerprint, hammingSimilarities, 4);
            });

            Assert.AreEqual(tracksCount, hammingSimilarities.Count);
            foreach (var pair in hammingSimilarities)
            {
                Assert.AreEqual(countPerTrack, pair.Value.Matches.Count);
                Assert.AreEqual(countPerTrack * 100, pair.Value.HammingSimilaritySum);
                Assert.AreEqual(100, pair.Value.BestMatch.HammingSimilarity);
            }
        }

        private IEnumerable<SubFingerprintData> GetSubFingerprints(int totalCount, int countPerTrack)
        {
            var subFingerprints = new List<SubFingerprintData>();
            int tracksCount = totalCount / countPerTrack;
            for (int i = 0; i < totalCount; ++i)
            {
                int trackId = i % tracksCount;
                var subFingerprint = new SubFingerprintData(new long[25], (uint)i, i * 1.48f, new ModelReference<ulong>((ulong)i), new ModelReference<int>(trackId));
                subFingerprints.Add(subFingerprint);
            }

            return subFingerprints;
        }
    }
}
