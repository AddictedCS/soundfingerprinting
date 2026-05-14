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

			Assert.That(result, Is.Empty);
        }
        
        [Test]
        public void ShouldFindLongestIncreasingSequenceWithOneElement()
        {
            var result = queryPathReconstructionStrategy.GetBestPaths(TestUtilities.GetMatchedWith(new[] { 0 }, new [] { 0 }), permittedGap: 0).ToList();

			Assert.That(result, Has.Count.EqualTo(1));
			Assert.That(result[0].Select(with => with.TrackMatchAt), Is.EqualTo(new float[] { 0 }).AsCollection);
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

			Assert.That(result.Select(_ => (int)_.QuerySequenceNumber), Is.EqualTo(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }).AsCollection);
			Assert.That(result.Select(_ => (int)_.TrackSequenceNumber), Is.EqualTo(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }).AsCollection);
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

			Assert.That(result.Select(_ => (int)_.QuerySequenceNumber), Is.EqualTo(new[] { 1, 4 }).AsCollection);
			Assert.That(result.Select(_ => (int)_.TrackSequenceNumber), Is.EqualTo(new[] { 1, 4 }).AsCollection);
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

			Assert.That(result.Select(_ => (int)_.QuerySequenceNumber), Is.EqualTo(new[] { 1, 2, 3, 4 }).AsCollection);
			Assert.That(result.Select(_ => (int)_.TrackSequenceNumber), Is.EqualTo(new[] { 1, 1, 1, 4 }).AsCollection);
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

			Assert.That(result.Select(_ => (int)_.QuerySequenceNumber), Is.EqualTo(new[] { 1, 2, 3, 4, 5, 6 }).AsCollection);
			Assert.That(result.Select(_ => (int)_.TrackSequenceNumber), Is.EqualTo(new[] { 1, 2, 3, 4, 6, 6 }).AsCollection);
        }
        
        [Test]
        public void ShouldFindLongestIncreasingSequence()
        {
            var matches = TestUtilities.GetMatchedWith(
                queryAt: new[] { 0, 1, 2, 10, 11, 12, 13, 14, 15, 16 }, 
                trackAt: new[] { 1, 2, 3, 1,  2,  3,  4,  5,  6,  7 });

            var result = queryPathReconstructionStrategy.GetBestPaths(matches, permittedGap: 0).ToList();

			Assert.That(result, Has.Count.EqualTo(2));
			Assert.That(result[0].Select(pair => pair.TrackMatchAt), Is.EqualTo(new float[] { 1, 2, 3, 4, 5, 6, 7 }).AsCollection);
			Assert.That(result[1].Select(pair => pair.TrackMatchAt), Is.EqualTo(new float[] { 1, 2, 3 }).AsCollection);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequenceComplex()
        {
            var matches = TestUtilities.GetMatchedWith(
                new[] { 0, 1, 2,   10, 11, 12, 13,  24, 25, 26 }, 
                new[] { 1, 2, 3,   1,  2,  3,  4,   1, 2, 3 });

            var result = queryPathReconstructionStrategy.GetBestPaths(matches, permittedGap: 0).ToList();

			Assert.That(result, Has.Count.EqualTo(3));
			Assert.That(result[0].Select(pair => pair.TrackMatchAt), Is.EqualTo(new float[] { 1, 2, 3, 4 }).AsCollection);
			Assert.That(result[1].Select(pair => pair.TrackMatchAt), Is.EqualTo(new float[] { 1, 2, 3 }).AsCollection);
			Assert.That(result[2].Select(pair => pair.TrackMatchAt), Is.EqualTo(new float[] { 1, 2, 3 }).AsCollection);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequenceComplex2()
        {
            var matches = TestUtilities.GetMatchedWith(
                new[] { 7, 8, 9, 10, 21, 22, 23, 24, 25, 36, 37, 38 }, 
                new[] { 1, 2, 3, 4,  1,  2,  3,  4,  5,  1,  2,  3 });

            var result = queryPathReconstructionStrategy.GetBestPaths(matches, permittedGap: 0).ToList();

			Assert.That(result, Has.Count.EqualTo(3));
			Assert.That(result[0].Select(pair => pair.TrackMatchAt), Is.EqualTo(new float[] { 1, 2, 3, 4, 5 }).AsCollection);
			Assert.That(result[1].Select(pair => pair.TrackMatchAt), Is.EqualTo(new float[] { 1, 2, 3, 4 }).AsCollection);
			Assert.That(result[2].Select(pair => pair.TrackMatchAt), Is.EqualTo(new float[] { 1, 2, 3 }).AsCollection);
        }

        [Test]
        public void ShouldBuildOneCoverageWithBigGap()
        {
            var matches = TestUtilities.GetMatchedWith(new[] {1, 2, 3, 10, 12, 13, 14}, new[] {1, 2, 3, 10, 12, 13, 14});

            var result = queryPathReconstructionStrategy.GetBestPaths(matches, permittedGap: 0).ToList();

			Assert.That(result, Has.Count.EqualTo(1));
			Assert.That(result.First().Select(_ => _.QueryMatchAt), Is.EqualTo(new float[] { 1, 2, 3, 10, 12, 13, 14 }).AsCollection);
        }
        
         [Test]
        public void ShouldFindLongestIncreasingSequenceEmpty()
        {
            var result = queryPathReconstructionStrategy.GetBestPaths(Enumerable.Empty<MatchedWith>(), permittedGap: 0);
			Assert.That(result.Any(), Is.False);
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

			Assert.That(result, Has.Count.EqualTo(1));
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

			Assert.That(results.Length, Is.EqualTo(2));
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

			Assert.That(result, Has.Count.EqualTo(1));
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

			Assert.That(results.Length, Is.EqualTo(3));
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

			Assert.That(results.Length, Is.EqualTo(2));
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

			Assert.That(results.Length, Is.EqualTo(2));
			Assert.That(results[0].Select(_ => (int)_.TrackSequenceNumber), Is.EqualTo(new[] { 1, 2, 3, 4, 5, 6 }).AsCollection);
			Assert.That(results[1].Select(_ => (int)_.TrackSequenceNumber), Is.EqualTo(new[] { 20, 21, 22, 23, 24, 25 }).AsCollection);
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
			Assert.That(results.Length, Is.EqualTo(2));

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

			Assert.That(results.Length, Is.EqualTo(3));
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
			Assert.That(bestPaths, Has.Count.EqualTo(1));

            var first = bestPaths[0].ToList();
			Assert.That(first.Select(_ => (int)_.QuerySequenceNumber), Is.EqualTo(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }).AsCollection);
			Assert.That(first.Select(_ => (int)_.TrackSequenceNumber), Is.EqualTo(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }).AsCollection);
        }

        [Test(Description = "Permitted gap allows artifacts to have a small mismatch")]
        public void ShouldNotGenerateSmallArtifactsWhenMatchesDoNotMatchExactly()
        {
            var pairs = new[] {(1, 1), (2, 2), (5, 3)};
            var results = queryPathReconstructionStrategy.GetBestPaths(Generate(pairs), permittedGap: 3).ToArray();

			Assert.That(results.Length, Is.EqualTo(1));
            var expected1 = new[] {(1, 1), (2, 2), (5, 3)};

            AssertResult(expected1, results[0]);
        }

        // before the fix the walkback's diagonal-distance swap could replace result[L] with a candidate whose
        // TrackSequenceNumber sat below the already-placed result[L-1], producing a path that went BACKWARDS on the
        // track axis (e.g. (q=2, t=465) → (q=267, t=326)). Track ties are tolerated, but a decrease is not.
        [Test(Description = "Track must not decrease across a path returned by GetBestPaths")]
        public void ShouldNotReturnDecreasingTrackSequenceWhenDiagonalReplacementCanBreakChain()
        {
            var matches = new[]
            {
                new MatchedWith(2, 0.18577649f, 465, 43.193035f, 1d),
                new MatchedWith(88, 8.174166f, 889, 82.57765f, 1d),
                new MatchedWith(126, 11.703918f, 276, 25.637156f, 1d),
                new MatchedWith(176, 16.348331f, 767, 71.245285f, 1d),
                new MatchedWith(186, 17.277214f, 976, 90.65893f, 1d),
                new MatchedWith(267, 24.80116f, 568, 52.76052f, 1d),
                new MatchedWith(267, 24.80116f, 326, 30.281567f, 1d),
            };

            var paths = queryPathReconstructionStrategy.GetBestPaths(matches, permittedGap: 3).ToList();

            AssertNoTrackDecreaseInAnyPath(paths);
        }

        // before the fix, a path could decrease on track mid-chain even when its endpoints stayed monotone — e.g.
        // (q=2, t=593) → (q=198, t=420) → (q=222, t=438): t goes 593 → 420 (decrease) → 438. The diagonal swap at
        // result[3] left the predecessor at result[2] dangling against the old, never-reverified anchor.
        [Test(Description = "Mid-path track decrease must not survive diagonal-distance swap")]
        public void ShouldNotReturnDecreasingTrackSequenceWhenChainReplacementMidPath()
        {
            var matches = new[]
            {
                new MatchedWith(2, 0.18577649f, 593, 55.08273f, 1d),
                new MatchedWith(10, 0.9288824f, 400, 37.155296f, 1d),
                new MatchedWith(115, 10.682148f, 198, 18.391872f, 1d),
                new MatchedWith(198, 18.391872f, 676, 62.792454f, 1d),
                new MatchedWith(198, 18.391872f, 420, 39.01306f, 1d),
                new MatchedWith(219, 20.342525f, 276, 25.637156f, 1d),
                new MatchedWith(222, 20.62119f, 694, 64.46444f, 1d),
                new MatchedWith(222, 20.62119f, 438, 40.68505f, 1d),
            };

            var paths = queryPathReconstructionStrategy.GetBestPaths(matches, permittedGap: 3).ToList();

            AssertNoTrackDecreaseInAnyPath(paths);
        }

        private static void AssertNoTrackDecreaseInAnyPath(List<IEnumerable<MatchedWith>> paths)
        {
            for (int p = 0; p < paths.Count; p++)
            {
                var path = paths[p].ToList();
                for (int i = 1; i < path.Count; i++)
                {
                    Assert.That(
                        path[i].TrackSequenceNumber,
                        Is.GreaterThanOrEqualTo(path[i - 1].TrackSequenceNumber),
                        $"path[{p}] has decreasing track sequence at index {i}: t={path[i - 1].TrackSequenceNumber} -> t={path[i].TrackSequenceNumber}");
                }
            }
        }

        private static void AssertResult((int q, int t)[] pairs, IEnumerable<MatchedWith> result)
        {
            var matches = result as MatchedWith[] ?? result.ToArray();
			Assert.That(matches.Select(_ => _.QuerySequenceNumber), Is.EqualTo(pairs.Select(_ => _.q)).AsCollection);
			Assert.That(matches.Select(_ => _.TrackSequenceNumber), Is.EqualTo(pairs.Select(_ => _.t)).AsCollection);
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
