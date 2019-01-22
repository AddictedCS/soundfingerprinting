namespace SoundFingerprinting.Tests.Unit.Query
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using NUnit.Framework;
    using SoundFingerprinting.Query;

    [TestFixture]
    public class MatchesTest
    {
        [Test]
        public void ShouldCollapseByQueryAt1()
        {
            // a   -------      0|10 + 10 = 10|20
            // b        ------- 8|18 + 10 = 18|28
            var a = GetMatches(0f, 10f, 10d, 512f / 5512);
            var b = GetMatches(8f, 18f, 10d, 512f / 5512);
            
            Assert.IsTrue(a.TryCollapseWith(b, 1.48d, out var c));
            Assert.AreEqual(19.5d, c.TotalLength, 0.1);
            Assert.AreEqual(a.Count() + b.Count(), c.Count());
            Assert.AreEqual(0d, c.QueryAtStartsAt);
            Assert.AreEqual(10f, c.TrackAtStartsAt);
        }
        
        [Test]
        public void ShouldCollapseByQueryAt2()
        {
            // a    ------------- 0|12 + 12 = 12|24
            // b        ----      4|16 + 4  = 8 |20
            var a = GetMatches(0f, 12f, 12d, 512f / 5512);
            var b = GetMatches(4f, 16f, 4d, 512f / 5512);
            
            Assert.IsTrue(a.TryCollapseWith(b, 1.48d, out var c));
            Assert.AreEqual(a.Count() + b.Count(), c.Count());
            Assert.AreEqual(13.5d, c.TotalLength, 0.1);
        }

        [Test]
        public void ShouldNotCollapseByQueryAtAsGapIsTooBig()
        {
            // a    ---------                0|10 + 10 = 10|20
            // b                   -------- 12|22 + 10 = 22|32
            var a = GetMatches(0f, 10f, 10d, 512f / 5512);
            var b = GetMatches(12f, 22f, 10d, 512f / 5512);
            
            Assert.IsFalse(a.TryCollapseWith(b, 1.48d, out _));
        }
        
        [Test]
        public void ShouldNotCollapseAsQueryMatchCorrespondsTo2DifferentTracksLocation2()
        {
            // query  -------- (matches both but in different TrackAt locations)
            // a      --------
            // b                  --------

            var a = GetMatches(0f, 10f, 10d, 512f / 5512);
            var b = GetMatches(0f, 25f, 10d, 512f / 5512);

            Assert.IsFalse(a.TryCollapseWith(b, 1.48f, out _));
        }

        [Test]
        [Ignore("Gap between consecutive matches is too small")]
        public void ShouldNotCollapseAsQueryMatchCorrespondsTo2DifferentTracksLocation()
        {
            // query  -------- (matches both but in different TrackAt locations)
            // a      --------
            // b               --------

            var a = GetMatches(0f, 10f, 10d, 512f / 5512);
            var b = GetMatches(0f, 20f, 10d, 512f / 5512);

            Assert.IsFalse(a.TryCollapseWith(b, 1.48f, out _));
        }

        [Test]
        public void ShouldNotCollapseAsQueryMatchesSameTrackMultipleTimes()
        {
            // query  ----------------------
            // track  --------
            // track                --------
            
            var a = GetMatches(0f, 0f, 15d, 512f / 5512);
            var b = GetMatches(60f, 0f, 15d, 512f / 5512);

            Assert.IsFalse(a.TryCollapseWith(b, 1.48f, out _));
        }

        private static Matches GetMatches(float startQueryAt, float startTrackAt, double length, float stride)
        {
            const int hammingSimilarity = 100;
            float startAt = 0f;
            var matches = new List<MatchedWith>();
            
            while (startAt <= length)
            {
                var match = new MatchedWith(startQueryAt + startAt, startTrackAt + startAt, hammingSimilarity);
                matches.Add(match);
                startAt += stride;
            }
            
            return new Matches(matches);
        }
    }
}