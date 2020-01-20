namespace SoundFingerprinting.Tests.Unit.LCS
{
    using System.Collections.Generic;
    using System.Linq;
    using NUnit.Framework;
    using SoundFingerprinting.LCS;
    using SoundFingerprinting.Query;

    [TestFixture]
    public class LisOldTest
    {
        [Test]
        public void ShouldFindLongestIncreasingSequence0()
        {
            /*
             * q         1 2 3 
             * t         1 2 3
             * expected  x x x
             */
            var pairs = new[] {(1, 1, 0d), (2, 2, 0d), (3, 3, 0d)};
            var result = LisOld.GetBestPath(Generate(pairs), 3, 1);

            AssertResult(pairs, result);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence1()
        {
            /* s         4 3 2 1   
             * q         1 2 3 4
             * t         1 1 1 2
             * expected  x     x
             */

            var pairs = new[] {(1, 1, 4d), (2, 1, 3), (3, 1, 2), (4, 2, 1)};
            var result = LisOld.GetBestPath(Generate(pairs), 4, 1);

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
             */

            var pairs = new[] {(1, 1, 3d), (1, 2, 2), (1, 3, 1), (4, 4, 1)};
            var result = LisOld.GetBestPath(Generate(pairs), 4, 1);

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
             */
            
            var pairs  = new[] {(1, 4, 0d), (2, 3, 1), (3, 2, 2), (4, 1, 3)};
            var result = LisOld.GetBestPath(Generate(pairs), 4, 1);

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
             */
            
            var pairs  = new[] {(1, 1, 1d), (2, 2, 1), (0, 3, 2), (3, 3, 1), (4, 4, 1)};
            var result = LisOld.GetBestPath(Generate(pairs), 5, 1).ToList();

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
             */
            
            var pairs  = new[] {(1, 1, 1d), (2, 2, 1), (0, 3, 2), (3, 3, 1), (4, 4, 1)};
            var result = LisOld.GetBestPath(Generate(pairs), 5, 1);

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
             */

            var pairs = new[] {(1, 1, 0d), (20, 1, 0), (2, 2, 1), (3, 2, 0), (21, 2, 0), (3, 3, 0), (22, 3, 1)};
            var results = LisOld.GetBestPath(Generate(pairs), 7, 1).ToList();

            var expected1 = new[] {(1, 1, 0d), (2, 2, 1), (3, 3, 0)};
            // var expected2 = new[] {(20, 1, 0d), (21, 2, 0), (22, 3, 0)};
            
            // AssertResult(expected1, results[0]);
            AssertResult(expected1, results);
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
             */
            
            var pairs  = new[] {(1, 1, 0d), (2, 2, 0), (4, 3, 2), (3, 4, 1), (3, 5, 0)};
            var result = LisOld.GetBestPath(Generate(pairs), 5, 1);

            var expected = new[] {(1, 1, 0d), (2, 2, 0), (4, 3, 2)};
            
            AssertResult(expected, result);
        }

        private void AssertResult((int q, int t, double s)[] pairs, IEnumerable<MatchedWith> result)
        {
            var matchedWiths = result as MatchedWith[] ?? result.ToArray();
            Assert.AreEqual(pairs.Length, matchedWiths.Length);

            for (int i = 0; i < matchedWiths.Length; ++i)
            {
                Assert.AreEqual(pairs[i].q, matchedWiths[i].QuerySequenceNumber);
                Assert.AreEqual(pairs[i].t, matchedWiths[i].TrackSequenceNumber);
                Assert.AreEqual(pairs[i].s, matchedWiths[i].Score);
            }
        }

        private static IEnumerable<MatchedWith> Generate((int q, int t, double s)[] queryTrackPairs)
        {
            return queryTrackPairs.Select(pair =>
            {
                (int q, int t, double s) = pair;
                return new MatchedWith((uint) q, q * 1.48f, (uint) t, t * 1.48f, s);
            });
        }
    }
}