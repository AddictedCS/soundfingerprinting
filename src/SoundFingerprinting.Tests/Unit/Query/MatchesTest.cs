namespace SoundFingerprinting.Tests.Unit.Query
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using NUnit.Framework;
    using SoundFingerprinting.Query;

    [TestFixture]
    [Ignore("not relevant")]
    public class MatchesTest
    {
        [Test]
        public void ShouldCollapseByQueryAt1()
        {
            // a   -------
            // b        -------
            var a = GetMatches(0f, 10f, 10d, 512f / 5512);
            var b = GetMatches(8f, 18f, 10d, 512f / 5512);
            
            Assert.IsTrue(a.TryCollapseWith(b, 1.48d, out var c));
            Assert.AreEqual(18d, c.TotalLength);
            Assert.AreEqual(a.Count() + b.Count(), c.Count());
            Assert.AreEqual(0d, c.QueryAtStartsAt);
            Assert.AreEqual(10f, c.TrackAtStartAt);
        }
        
        [Test]
        public void ShouldCollapseByQueryAt2()
        {
            // a    -------------
            // b        ----
            var a = GetMatches(0f, 12f, 12d, 512f / 5512);
            var b = GetMatches(4f, 18f, 4d, 512f / 5512);
            
            Assert.IsTrue(a.TryCollapseWith(b, 1.48d, out var c));
            Assert.AreEqual(a.Count() + b.Count(), c.Count());
            Assert.AreEqual(12d, c.TotalLength);
        }

        [Test]
        public void ShouldNotCollapseByQueryAtAsGapIsTooBig()
        {
            // a    ---------
            // b                   --------
            var a = GetMatches(0f, 10f, 10d, 512f / 5512);
            var b = GetMatches(12f, 10f, 10d, 512f / 5512);
            
            Assert.IsFalse(a.TryCollapseWith(b, 1.48d, out _));
            
            // TODO add additional checks here
        }

        private static Matches GetMatches(float startQueryAt, float startTrackAt, double length, float stride)
        {
            float startAt = 0f;

            var matches = new List<MatchedWith>();
            while (startAt <= length)
            {
                var match = new MatchedWith(startQueryAt + startAt, startTrackAt + startAt, 100);
                matches.Add(match);
                startAt += stride;
            }
            
            return new Matches(matches);
        }
    }
}