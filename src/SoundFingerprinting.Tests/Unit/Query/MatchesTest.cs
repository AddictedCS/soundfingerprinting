namespace SoundFingerprinting.Tests.Unit.Query
{
    using System.Collections.Generic;
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
        public void ShouldIdentifyAsContainedWithinItself()
        {
            // q ------------------------ 
            // a ------------------------
            // b    -----

            var a = GetMatches(0, 0, 120, 512f / 5512);
            var b = GetMatches(10, 20, 10, 512f / 5512);

            Assert.IsFalse(a.TryCollapseWith(b, 1.48d, out _));
            Assert.IsTrue(a.Contains(b));
            Assert.IsFalse(b.Contains(a));
            Assert.IsFalse(b.TryCollapseWith(a, 1.48, out _));
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
        public void ShouldNotCollapseAsQueryMatchCorrespondsTo2DifferentTracksLocation()
        {
            // query  -------- (matches both but in different TrackAt locations)
            // a      --------
            // b               --------

            var a = GetMatches(0f, 0f, 10d, 512f / 5512);
            var b = GetMatches(0f, 10f, 10d, 512f / 5512);

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

        [Test]
        public void ShouldNotMergeWithTrackAtReversed()
        {
            // a   -------          0|15 + 10 = 10|25
            // b            ------- 0|35 + 10 = 10|45
            var a = GetMatches(0f, 15f, 10d, 512f / 5512);
            var b = GetMatches(0f, 35f, 10d, 512f / 5512);

            Assert.IsFalse(b.TryCollapseWith(a, 1.48f, out _));
        }

        [Test]
        public void ShouldSplitMatchedRegionsToGapRegions()
        {
            // a    ------     -------  -------
            // t    0    10   20     30 32   40
            float stride = 1;
            var matches = GetMatches(0f, 0f, 10d, stride)
                .Concat(GetMatches(20f, 20f, 10, stride, 20))
                .Concat(GetMatches(32f, 32f, 8, stride, 32));

            float fingerprintLengthInSeconds = 1f;
            double permittedGap = 2.5;
            double trackLength = 40;
            double queryLength = 40;
            var coverages = matches.SplitTrackMatchedRegions(queryLength, trackLength, fingerprintLengthInSeconds, permittedGap).ToList();

            Assert.AreEqual(2, coverages.Count);
            Assert.AreEqual(10 + fingerprintLengthInSeconds, coverages.First().TrackCoverageWithPermittedGapsLength);
            Assert.AreEqual(20 + fingerprintLengthInSeconds, coverages.Last().TrackCoverageWithPermittedGapsLength);
        }

        private static Matches GetMatches(float startQueryAt, float startTrackAt, double length, float stride, int startIndex = 0)
        {
            const int hammingSimilarity = 100;
            float startAt = 0f;
            var matches = new List<MatchedWith>();
            uint index = (uint)startIndex;
            while (startAt <= length)
            {
                var match = new MatchedWith(index, startQueryAt + startAt, index, startTrackAt + startAt, hammingSimilarity);
                matches.Add(match);
                startAt += stride;
                index++;
            }

            return new Matches(matches);
        }
    }
}