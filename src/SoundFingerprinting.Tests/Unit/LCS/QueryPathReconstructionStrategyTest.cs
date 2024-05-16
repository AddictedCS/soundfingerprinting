namespace SoundFingerprinting.Tests.Unit.LCS
{
    using System.Collections.Generic;
    using System.Linq;

    using NUnit.Framework;

    using SoundFingerprinting.LCS;
    using SoundFingerprinting.Query;

    [TestFixture]
    public class QueryPathReconstructionStrategyTest
    {
        private readonly IQueryPathReconstructionStrategy queryPathReconstructionStrategy = new QueryPathReconstructionStrategy();

        [Test]
        public void ShouldNotThrowWhenEmptyIsPassed()
        {
            var result = queryPathReconstructionStrategy.GetBestPaths([], permittedGap: 0);

            CollectionAssert.IsEmpty(result);
        }
        
        [Test]
        public void ShouldFindLongestIncreasingSequenceWithOneElement()
        {
            var result = queryPathReconstructionStrategy.GetBestPaths(TestUtilities.GetMatchedWith(new[] { 0 }, new [] { 0 }), permittedGap: 0).ToList();

            Assert.AreEqual(1, result.Count);
            CollectionAssert.AreEqual(new float[] { 0 }, result[0].Select(with => with.TrackMatchAt));
        }

        /*
         * q         1 2 3 7 8 4 5 6 7 8 2 3 9
         * t         1 2 3 2 3 4 5 6 7 8 7 8 9
         */
        [Test(Description = "Cross match (2,7) and (3,8) between query and track should be ignored")]
        public void ShouldIgnoreRepeatingCrossMatches()
        {
            var matchedWiths = new[] { (1, 1), (2, 2), (3, 3), (7, 2), (8, 3), (4, 4), (5, 5), (6, 6), (7, 7), (8, 8), (2, 7), (3, 8), (9, 9) }
                .Select(tuple => new MatchedWith((uint)tuple.Item1, tuple.Item1, (uint)tuple.Item2, tuple.Item2, 0d));

            var result = queryPathReconstructionStrategy.GetBestPaths(matchedWiths, permittedGap: 0).First().ToList();

            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, result.Select(_ => (int)_.QuerySequenceNumber));
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, result.Select(_ => (int)_.TrackSequenceNumber));
        }

        /*
         * q         1 1 1 4
         * t         1 2 3 4
         * expected  x     x
         * max       1 1 1 2
         */
        [Test]
        public void ShouldPickAllQueryCandidates()
        {
            var matchedWiths = new[] { (1, 1), (1, 2), (1, 3), (4, 4) }.Select(tuple => new MatchedWith((uint)tuple.Item1, tuple.Item1, (uint)tuple.Item2, tuple.Item2, 0d));

            var result = queryPathReconstructionStrategy.GetBestPaths(matchedWiths, permittedGap: 0).First().ToList();

            CollectionAssert.AreEqual(new[] { 1, 4 }, result.Select(_ => (int)_.QuerySequenceNumber));
            CollectionAssert.AreEqual(new[] { 1, 4 }, result.Select(_ => (int)_.TrackSequenceNumber));
        }

        /*
        * q         1 2 3 4
        * t         1 1 1 4
        * expected  x x x x
        * max       1 1 1 2
        */
        [Test]
        public void ShouldPickAllTrackCandidates()
        {
            var matchedWiths = new[] { (1, 1), (2, 1), (3, 1), (4, 4) }.Select(tuple =>
                new MatchedWith((uint)tuple.Item1, tuple.Item1, (uint)tuple.Item2, tuple.Item2, 0d));

            var result = queryPathReconstructionStrategy.GetBestPaths(matchedWiths, permittedGap: 0).First().ToList();

            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, result.Select(_ => (int)_.QuerySequenceNumber));
            CollectionAssert.AreEqual(new[] { 1, 1, 1, 4 }, result.Select(_ => (int)_.TrackSequenceNumber));
        }
        
        /*
         * q         1 2 3 4 7 4 5 6 
         * t         1 2 3 4 6 6 6 6
         * expected  x x x x
         * max       1 2 3 4 5 4 5 6
         */
        [Test]
        public void ShouldNotUpdateIfQueryMatchReversalDetected()
        {
            var matchedWiths = new[] { (1, 1), (2, 2), (3, 3), (4, 4), (7, 6), (4, 6), (5, 6), (6, 6) }
                .Select(tuple => new MatchedWith((uint)tuple.Item1, tuple.Item1, (uint)tuple.Item2, tuple.Item2, 0d));

            var result = queryPathReconstructionStrategy.GetBestPaths(matchedWiths, permittedGap: 0).First().ToList();

            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5, 6 }, result.Select(_ => (int)_.QuerySequenceNumber));
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 6, 6 }, result.Select(_ => (int)_.TrackSequenceNumber));
        }
        
        [Test]
        public void ShouldFindLongestIncreasingSequence()
        {
            var matches = TestUtilities.GetMatchedWith(
                queryAt: new[] { 0, 1, 2, 10, 11, 12, 13, 14, 15, 16 }, 
                trackAt: new[] { 1, 2, 3, 1,  2,  3,  4,  5,  6,  7 });

            var result = queryPathReconstructionStrategy.GetBestPaths(matches, permittedGap: 0).ToList();

            Assert.AreEqual(2, result.Count);
            CollectionAssert.AreEqual(new float[] { 1, 2, 3, 4, 5, 6, 7 }, result[0].Select(pair => pair.TrackMatchAt));
            CollectionAssert.AreEqual(new float[] { 1, 2, 3 }, result[1].Select(pair => pair.TrackMatchAt));
        }

        [Test]
        public void ShouldFindLongestIncreasingSequenceComplex()
        {
            var matches = TestUtilities.GetMatchedWith(
                new[] { 0, 1, 2,   10, 11, 12, 13,  24, 25, 26 }, 
                new[] { 1, 2, 3,   1,  2,  3,  4,   1, 2, 3 });

            var result = queryPathReconstructionStrategy.GetBestPaths(matches, permittedGap: 0).ToList();

            Assert.AreEqual(3, result.Count);
            CollectionAssert.AreEqual(new float[] { 1, 2, 3, 4 }, result[0].Select(pair => pair.TrackMatchAt));
            CollectionAssert.AreEqual(new float[] { 1, 2, 3 }, result[1].Select(pair => pair.TrackMatchAt));
            CollectionAssert.AreEqual(new float[] { 1, 2, 3 }, result[2].Select(pair => pair.TrackMatchAt));
        }

        [Test]
        public void ShouldFindLongestIncreasingSequenceComplex2()
        {
            var matches = TestUtilities.GetMatchedWith(
                new[] { 7, 8, 9, 10, 21, 22, 23, 24, 25, 36, 37, 38 }, 
                new[] { 1, 2, 3, 4,  1,  2,  3,  4,  5,  1,  2,  3 });

            var result = queryPathReconstructionStrategy.GetBestPaths(matches, permittedGap: 0).ToList();

            Assert.AreEqual(3, result.Count);
            CollectionAssert.AreEqual(new float[] { 1, 2, 3, 4, 5 }, result[0].Select(pair => pair.TrackMatchAt));
            CollectionAssert.AreEqual(new float[] { 1, 2, 3, 4 }, result[1].Select(pair => pair.TrackMatchAt));
            CollectionAssert.AreEqual(new float[] { 1, 2, 3 }, result[2].Select(pair => pair.TrackMatchAt));
        }

        [Test]
        public void ShouldBuildOneCoverageWithBigGap()
        {
            var matches = TestUtilities.GetMatchedWith(new[] {1, 2, 3, 10, 12, 13, 14}, new[] {1, 2, 3, 10, 12, 13, 14});

            var result = queryPathReconstructionStrategy.GetBestPaths(matches, permittedGap: 0).ToList();
            
            Assert.AreEqual(1, result.Count);
            CollectionAssert.AreEqual(new float[] { 1, 2, 3, 10, 12, 13, 14 }, result.First().Select(_ => _.QueryMatchAt));
        }
        
         [Test]
        public void ShouldFindLongestIncreasingSequenceEmpty()
        {
            var result = queryPathReconstructionStrategy.GetBestPaths(Enumerable.Empty<MatchedWith>(), permittedGap: 0);
            Assert.IsFalse(result.Any());
        }

        [Test]
        public void ShouldFindLongestIncreasingSequenceTrivial()
        {
            var pairs = new[] {(1, 1)};
            var result = queryPathReconstructionStrategy.GetBestPaths(Generate(pairs), permittedGap: 0).ToList();

            AssertResult(pairs, result[0]);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence0()
        {
            /*
             * q         1 2 3
             * t         1 2 3
             * expected  x x x
             */
            var pairs = new[] {(1, 1), (2, 2), (3, 3)};
            var result = queryPathReconstructionStrategy.GetBestPaths(Generate(pairs), permittedGap: 0).First();

            AssertResult(pairs, result);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence1()
        {
            /*
             * q         1 2 3 4
             * t         1 1 1 2
             * expected  x     x
             * max       1 2 3 4
             */

            var pairs = new[] {(1, 1), (2, 1), (3, 1), (4, 2)};
            var result = queryPathReconstructionStrategy.GetBestPaths(Generate(pairs), permittedGap: 0).ToList();

            AssertResult(pairs, result[0]);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence2()
        {
            /*
             * q         1 1 1 4
             * t         1 2 3 4
             * expected  x     x
             * max       1 1 1 2
             */

            var pairs = new[] {(1, 1), (1, 2), (1, 3), (4, 4)};
            var expected = new[] {(1, 1), (4, 4)};
            var result = queryPathReconstructionStrategy.GetBestPaths(Generate(pairs), permittedGap: 0).First();

            AssertResult(expected, result);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence3()
        {
            /*
             *
             * q         1 2 3 4
             * t         4 3 2 1
             * expected        x
             * max       1 1 1 1
             * case max' stays the same, t is decreasing
             */

            var pairs = new[] {(1, 4), (2, 3), (3, 2), (4, 1)};
            var result = queryPathReconstructionStrategy.GetBestPaths(Generate(pairs), permittedGap: 0).ToList();

            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence4()
        {
            /*         ignore reverse
             *               |
             * q         1 2 0 3 4
             * t         1 2 3 3 4
             * expected  x x   x x
             * max       1 2 1 3 4
             * max' is decreasing (by 2), t stays the same 2 elements (need to skip sorting as max indicates we need to skip)
             */

            var pairs = new[] {(1, 1), (2, 2), (0, 3), (3, 3), (4, 4)};
            var result = queryPathReconstructionStrategy.GetBestPaths(Generate(pairs), permittedGap: 0).ToList();

            var expected = new[] {(1, 1), (2, 2), (3, 3), (4, 4)};

            AssertResult(expected, result[0]);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence5()
        {
            /*
             * q         1 2 3 4 4
             * t         1 2 3 3 4
             * expected  x x x   x
             * max       1 2 3 4 4
             * maxs' stays the same pick by score
             */

            var pairs = new[] {(1, 1), (2, 2), (3, 3), (4, 3), (4, 4)};
            var expected = new[] {(1, 1), (2, 2), (3, 3), (4, 4)};
            var result = queryPathReconstructionStrategy.GetBestPaths(Generate(pairs), permittedGap: 0).ToList();

            AssertResult(expected, result[0]);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence6()
        {
            /*
             * same song appears twice
             * in the query in 2 different locations
             *
             * q          1 20 2 3 21 3 22
             * t          1 1  2 2 2  3 3
             * expected   x y  x   y  x y
             * max        1 2  2 3 4  3 5
             * max (c.)   1 1  2 3 2  3 3
             */

            var pairs = new[] {(1, 1), (20, 1), (2, 2), (3, 2), (21, 2), (3, 3), (22, 3)};
            var results = queryPathReconstructionStrategy.GetBestPaths(Generate(pairs), permittedGap: 0).ToArray();

            var expected1 = new[] {(1, 1), (2, 2), (3, 3)};
            var expected2 = new[] {(20, 1), (21, 2), (22, 3)};

            Assert.AreEqual(2, results.Length);
            AssertResult(expected1, results[0]);
            AssertResult(expected2, results[1]);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence7()
        {
            /*
             * q         1 2 4 3 3
             * t         1 2 3 4 5
             * expected  x x   x  
             * max       1 2 3 3 3
             */

            var pairs = new[] {(1, 1), (2, 2), (4, 3), (3, 4), (3, 5)};
            var result = queryPathReconstructionStrategy.GetBestPaths(Generate(pairs), permittedGap: 0).ToList();

            Assert.AreEqual(1, result.Count);
            var expected1 = new[] {(1, 1), (2, 2), (3, 4)};
            AssertResult(expected1, result[0]);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence8()
        {
            /*
             * q          20 1 2 21 3 22 4 0 5
             * t          1  1 2 2  3 3  4 4 5
             * expected   y  x x y  x y  x   x
             * max        1  2 2 3  3 4  4 1 5
             * max (c.)   1  1 2 2  3 3  4 1 5
             */

            var pairs = new[] {(20, 1), (1, 1), (2, 2), (21, 2), (3, 3), (22, 3), (4, 4), (0, 4), (5, 5)};

            var results = queryPathReconstructionStrategy.GetBestPaths(Generate(pairs), permittedGap: 0).ToArray();

            var expected1 = new[] {(1, 1), (2, 2), (3, 3), (4, 4), (5, 5)};
            var expected2 = new[] {(20, 1), (21, 2), (22, 3)};
            var expected3 = new[] { (0, 4) };

            Assert.AreEqual(3, results.Length);
            AssertResult(expected1, results[0]);
            AssertResult(expected2, results[1]);
            AssertResult(expected3, results[2]);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence9()
        {
            /*
             * q        1 2 3 4 5  1  2  3  4  5  6
             * t        1 2 3 4 5  20 21 22 23 24 25
             * max (c.) 1 2 3 4 5  1  2  3  4  5  6
             */

            var pairs = new[] {(1, 1), (2, 2), (3, 3), (4, 4), (1, 20), (2, 21), (3, 22), (4, 23), (5, 24), (6, 25)};
            var results = queryPathReconstructionStrategy.GetBestPaths(Generate(pairs), permittedGap: 0).ToArray();

            Assert.AreEqual(2, results.Length);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence10()
        {
            /*
              * q        1 2 3 4 5 6 0  2  3  4  5  7
              * t        1 2 3 4 5 6 20 21 22 23 24 25
              * max (c.) 1 2 3 4 5 6 1  2  3  4  5  7
              */

            var pairs = new[] {(1, 1), (2, 2), (3, 3), (4, 4), (5, 5), (6, 6), (0, 20), (2, 21), (3, 22), (4, 23), (5, 24), (7, 25)};
            var results = queryPathReconstructionStrategy.GetBestPaths(Generate(pairs), permittedGap: 0).ToArray();

            Assert.AreEqual(2, results.Length);
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5, 6 }, results[0].Select(_ => (int)_.TrackSequenceNumber));
            CollectionAssert.AreEqual(new[] { 20, 21, 22, 23, 24, 25 }, results[1].Select(_ => (int)_.TrackSequenceNumber));
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence11()
        {
            /*
              * q        1 2 0  1  2
              * t        1 2 20 21 22
              * max (c.) 1 2 1  2  3
              */

            var pairs = new[] {(1, 1), (0, 20), (2, 2)};
            var results = queryPathReconstructionStrategy.GetBestPaths(Generate(pairs), permittedGap: 0).ToArray();

            var expected1 = new[] {(1, 1), (2, 2)};
            var expected2 = new[] {(0, 20)};
            Assert.AreEqual(2, results.Length);

            AssertResult(expected1, results[0]);
            AssertResult(expected2, results[1]);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence12()
        {
            /*
             * q            1 2 2 2
             * t            1 2 3 4
             * max (c.)     1 2 2 2
             */
            var pairs = new[] { (1, 1), (2, 2), (2, 3), (2, 4) };
            var results = queryPathReconstructionStrategy.GetBestPaths(Generate(pairs), permittedGap: 0).ToArray();

            Assert.AreEqual(3, results.Length);
            AssertResult(new[] { (1, 1), (2, 2) }, results[0]);
            AssertResult(new[] { (2, 3) }, results[1]);
            AssertResult(new[] { (2, 4) }, results[2]);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence13()
        {
            /*
            * q            1 5 4 3
            * t            1 2 3 4
            * max (c.)     1 2 2 2
            */
            var pairs = new[] {(1, 1), (5, 2), (4, 3), (3, 4)};
            var results = queryPathReconstructionStrategy.GetBestPaths(Generate(pairs), permittedGap: 0).ToArray();

            var expected1 = new[] {(1, 1), (3, 4)};

            AssertResult(expected1, results[0]);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence14()
        {
            /*
            * q            1 10 2 11 3 12 1  10 2  11 3  12
            * t            1 1  2 2  3 3  10 10 11 11 12 12
             * max (c.)    1 2  2 3  3 4  1  4  2  5  3  6
            */
            var pairs = new[]
            {
                (1, 1), (10, 1), (2, 2), (11, 2), (3, 3), (12, 3), (1, 10), (10, 10), (2, 11), (11, 11), (3, 12), (12, 12)
            };
            var results = queryPathReconstructionStrategy.GetBestPaths(Generate(pairs), permittedGap: 0).ToArray();

            var expected1 = new[] {(1, 1), (2, 2), (3, 3), (10, 10), (11, 11), (12, 12)};

            AssertResult(expected1, results[0]);
        }

        /*
         * q         1 2 3 7 8 4 5 6 7 8 2 3 9
         * t         1 2 3 2 3 4 5 6 7 8 7 8 9
         */
        [Test(Description = "Cross matches should be ignored as they break the increasing order of the sequence")]
        public void ShouldNotIgnoreRepeatingCrossMatches()
        {
            var matchedWiths = new[]
            {
                (1, 1), (2, 2), (3, 3), (7, 2), (8, 3), (4, 4), (5, 5), (6, 6), (7, 7), (8, 8), (2, 7), (3, 8), (9, 9)
            }
            .Select(tuple => new MatchedWith((uint)tuple.Item1, tuple.Item1, (uint)tuple.Item2, tuple.Item2, 0d));

            var bestPaths = queryPathReconstructionStrategy.GetBestPaths(matchedWiths, permittedGap: 0).ToList();
            Assert.AreEqual(1, bestPaths.Count);

            var first = bestPaths[0].ToList();
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, first.Select(_ => (int)_.QuerySequenceNumber));
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, first.Select(_ => (int)_.TrackSequenceNumber));
        }

        [Test(Description = "Permitted gap allows artifacts to have a small mismatch")]
        public void ShouldNotGenerateSmallArtifactsWhenMatchesDoNotMatchExactly()
        {
            var pairs = new[] {(1, 1), (2, 2), (5, 3)};
            var results = queryPathReconstructionStrategy.GetBestPaths(Generate(pairs), permittedGap: 3).ToArray();

            Assert.AreEqual(1, results.Length);
            var expected1 = new[] {(1, 1), (2, 2), (5, 3)};

            AssertResult(expected1, results[0]); 
        }

        private static void AssertResult((int q, int t)[] pairs, IEnumerable<MatchedWith> result)
        {
            var matches = result as MatchedWith[] ?? result.ToArray();
            CollectionAssert.AreEqual(pairs.Select(_ => _.q), matches.Select(_ => _.QuerySequenceNumber));
            CollectionAssert.AreEqual(pairs.Select(_ => _.t), matches.Select(_ => _.TrackSequenceNumber));
        }

        private static IEnumerable<MatchedWith> Generate((int q, int t)[] queryTrackPairs)
        {
            return queryTrackPairs.Select(pair =>
            {
                (int q, int t) = pair;
                return new MatchedWith((uint) q, q, (uint) t, t, 0d);
            });
        }
    }
}
