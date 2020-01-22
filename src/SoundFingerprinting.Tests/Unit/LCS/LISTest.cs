namespace SoundFingerprinting.Tests.Unit.LCS
{
    using System.Collections.Generic;
    using System.Linq;
    using NUnit.Framework;
    using SoundFingerprinting.LCS;
    using SoundFingerprinting.Query;

    [TestFixture]
    public class LISTest
    {
        [Test]
        public void ShouldFindLongestIncreasingSequenceEmpty()
        {
            var result = LIS.GetIncreasingSequences(Enumerable.Empty<MatchedWith>());
            
            Assert.IsFalse(result.Any());
        }
        
        [Test]
        public void ShouldFindLongestIncreasingSequenceTrivial()
        {
            var pairs = new[] {(1, 1, 0d)};
            var result = LIS.GetIncreasingSequences(Generate(pairs)).First();
            
            AssertResult(pairs, result);
        }
        
        [Test]
        public void ShouldFindLongestIncreasingSequence0()
        {
            /*
             * q         1 2 3 
             * t         1 2 3
             * expected  x x x
             */
            var pairs = new[] {(1, 1, 0d), (2, 2, 0d), (3, 3, 0d)};
            var result = LIS.GetIncreasingSequences(Generate(pairs)).First();

            AssertResult(pairs, result);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence1()
        {
            /* s         4 3 2 1   
             * q         1 2 3 4
             * t         1 1 1 2    
             * expected  x     x
             * max       1 2 3 4
             * case max' is decreasing, t stays the same last 3 elements (need to pick only one)
             */

            var pairs = new[] {(1, 1, 4d), (2, 1, 3), (3, 1, 2), (4, 2, 1)};
            var result = LIS.GetIncreasingSequences(Generate(pairs)).First();

            var expected = new[] {(1, 1, 4d), (4, 2, 1)};
            AssertResult(expected, result);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence2()
        {
            /*
             *      pick best score
             *           |
             * score     3 2 1 1 
             * q         1 1 1 4
             * t         1 2 3 4
             * expected  x     x
             * max       1 1 1 2
             * case max' stays the same last 3 elements, t' is decreasing (need to pick only one by score)
             */

            var pairs = new[] {(1, 1, 3d), (1, 2, 2), (1, 3, 1), (4, 4, 1)};
            var result = LIS.GetIncreasingSequences(Generate(pairs)).First();

            var expected = new[] {(1, 1, 3), (4, 4, 1d)};
            AssertResult(expected, result);
        }


        [Test]
        public void ShouldFindLongestIncreasingSequence3()
        {
            /* reversed pick best score
             * 
             * score     0 1 2 3
             * q         1 2 3 4
             * t         4 3 2 1
             * expected        x
             * max       1 1 1 1
             * case max' stays the same, t is decreasing, (need to pick only one by score)
             */

            var pairs = new[] {(1, 4, 0d), (2, 3, 1), (3, 2, 2), (4, 1, 3)};
            var result = LIS.GetIncreasingSequences(Generate(pairs)).First();

            var expected = new[] {(4, 1, 3d)};

            AssertResult(expected, result);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence4()
        {
            /*         ignore reverse   
             *               |
             * score     1 1 2 1 1
             * q         1 2 0 3 4
             * t         1 2 3 3 4
             * expected  x x   x x
             * max       1 2 1 3 4
             * max' is decreasing (by 2), t stays the same 2 elements (need to skip sorting as max indicates we need to skip)
             */

            var pairs = new[] {(1, 1, 1d), (2, 2, 1), (0, 3, 2), (3, 3, 1), (4, 4, 1)};
            var result = LIS.GetIncreasingSequences(Generate(pairs)).First();

            var expected = new[] {(1, 1, 1d), (2, 2, 1), (3, 3, 1), (4, 4, 1)};

            AssertResult(expected, result);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence5()
        {
            /*
             *         pick best score
             *               |
             * score     1 1 2 1 1
             * q         1 2 3 4 4
             * t         1 2 3 3 4
             * expected  x x x   x
             * max       1 2 3 4 4
             * maxs' stays the same pick by score
             */

            var pairs = new[] {(1, 1, 1d), (2, 2, 1), (3, 3, 2), (4, 3, 1), (4, 4, 1)};
            var result = LIS.GetIncreasingSequences(Generate(pairs)).First();

            var expected = new[] {(1, 1, 1d), (2, 2, 1), (3, 3, 2), (4, 4, 1)};

            AssertResult(expected, result);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence6()
        {
            /*
             * same song appears twice
             * in the query in 2 different locations
             *
             *   pick best scoring option for first encounter
             *                 |
             * s          0  0 1 0 0  0 1
             * q          1 20 2 3 21 3 22  
             * t          1 1  2 2 2  3 3
             * expected   x y  x   y  x y
             * max        1 2  2 3 4  3 5
             * max (c.)   1 1  2 3 2  3 3
             */

            var pairs = new[] {(1, 1, 0d), (20, 1, 0), (2, 2, 1), (3, 2, 0), (21, 2, 0), (3, 3, 0), (22, 3, 1)};
            var results = LIS.GetIncreasingSequences(Generate(pairs), 10).ToArray();

            var expected1 = new[] {(1, 1, 0d), (2, 2, 1), (3, 3, 0)};
            var expected2 = new[] {(20, 1, 0d), (21, 2, 0), (22, 3, 1)};

            Assert.AreEqual(2, results.Length);
            AssertResult(expected1, results[0]);
            AssertResult(expected2, results[1]);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence7()
        {
            /* reversed pick best score
             * s         0 0 2 1 0
             * q         1 2 4 3 3
             * t         1 2 3 4 5
             * expected  x x x
             * max       1 2 3 3 3
             * maxs' stays the same pick only one (by score)
             */

            var pairs = new[] {(1, 1, 0d), (2, 2, 0), (4, 3, 2), (3, 4, 1), (3, 5, 0)};
            var result = LIS.GetIncreasingSequences(Generate(pairs)).First();

            var expected = new[] {(1, 1, 0d), (2, 2, 0), (4, 3, 2)};

            AssertResult(expected, result);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence8()
        {
            /*
             * s
             * q          20 1 2 21 3 22 4 0 5 
             * t          1  1 2 2  3 3  4 4 5 
             * expected   y  x x y  x y  x   x
             * max        1  2 2 3  3 4  4 1 5
             * max (c.)   1  1 2 2  3 3  4 1 5
             */

            var pairs = new[] {(20, 1, 0d), (1, 1, 0), (2, 2, 0), (21, 2, 0), (3, 3, 0), (22, 3, 1), (4, 4, 0), (0, 4, 0), (5, 5, 0)};

            var results = LIS.GetIncreasingSequences(Generate(pairs), 10).ToArray();

            var expected1 = new[] {(1, 1, 0d), (2, 2, 0), (3, 3, 0), (4, 4, 0), (5, 5, 0)};
            var expected2 = new[] {(20, 1, 0), (21, 2, 0), (22, 3, 1d)};

            Assert.AreEqual(2, results.Length);
            AssertResult(expected2, results[0]);
            AssertResult(expected1, results[1]);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence9()
        {
            /*
             * q        1 2 3 4 5  1  2  3  4  5  6
             * t        1 2 3 4 5  20 21 22 23 24 25
             * max (c.) 1 2 3 4 5  1  2  3  4  5  6
             */

            var pairs = new[] {(1, 1, 0d), (2, 2, 0), (3, 3, 0), (4, 4, 0), (1, 20, 0), (2, 21, 0), (3, 22, 0), (4, 23, 0), (5, 24, 0), (6, 25, 0)};
            var results = LIS.GetIncreasingSequences(Generate(pairs), 6).ToArray();
            
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
            
            var pairs = new[] {(1, 1, 0d), (2, 2, 0), (3, 3, 0), (4, 4, 0), (5, 5, 0), (6, 6, 0), (0, 20, 0), (2, 21, 0), (3, 22, 0), (4, 23, 0), (5, 24, 0), (7, 25, 0)};
            var results = LIS.GetIncreasingSequences(Generate(pairs), 7).ToArray();
            
            Assert.AreEqual(2, results.Length);
        }
        
        [Test]
        public void ShouldFindLongestIncreasingSequence11()
        {
            /*
              * q        1 2 0  1  2
              * t        1 2 20 21 22  
              * max (c.) 1 2 1  2  3
              */

            var pairs = new[] {(1, 1, 0d), (0, 20, 0), (2, 2, 0)};
            var results = LIS.GetIncreasingSequences(Generate(pairs), 5).ToArray();

            var expected1 = new[] {(1, 1, 0d), (2, 2, 0)};
            var expected2 = new[] {(0, 20, 0d)};
            Assert.AreEqual(2, results.Length);
            
            AssertResult(expected1, results[0]);
            AssertResult(expected2, results[1]);
        }

        private static void AssertResult((int q, int t, double s)[] pairs, IEnumerable<MatchedWith> result)
        {
            var matches = result as MatchedWith[] ?? result.ToArray();
            CollectionAssert.AreEqual(pairs.Select(_ => _.q), matches.Select(_ => _.QuerySequenceNumber));
            CollectionAssert.AreEqual(pairs.Select(_ => _.t), matches.Select(_ => _.TrackSequenceNumber));
            CollectionAssert.AreEqual(pairs.Select(_ => _.s), matches.Select(_ => _.Score));
        }

        private static IEnumerable<MatchedWith> Generate((int q, int t, double s)[] queryTrackPairs)
        {
            return queryTrackPairs.Select(pair =>
            {
                (int q, int t, double s) = pair;
                return new MatchedWith((uint) q, q, (uint) t, t, s);
            });
        }
    }
}