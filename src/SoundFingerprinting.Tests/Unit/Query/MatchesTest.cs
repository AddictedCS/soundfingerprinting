namespace SoundFingerprinting.Tests.Unit.Query
{
    using System.Collections.Generic;
    using NUnit.Framework;
    using SoundFingerprinting.Query;

    [TestFixture]
    public class MatchesTest
    {
        [Test]
        public void ShouldCollapse1()
        {
            // a   -------
            // b        -------
            
            var a = GetMatches(0f, 10f, 10d, 512f / 5512);
            var b = GetMatches(8f, 18, 10, 512f / 5512);
            
            Assert.IsTrue(a.TryCollapseWith(b, 1.48d, out var c));
            
            
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