namespace SoundFingerprinting.Tests.Unit.LCS;

using System.Linq;
using NUnit.Framework;

[TestFixture]
public class CoverageTest
{
    [TestCase(30, new double[0], new[] { 15d, 30d }, TestName = "Clean split no gaps")]
    [TestCase(30, new[] { 5d, 10d }, new[] { 15d, 30d }, TestName = "First split contains a gap, second does not")]
    [TestCase(30, new[] { 5d, 10d, 20d, 25d }, new[] { 15d, 30d }, TestName = "Both splits contain gaps")]
    [TestCase(30, new[] {0d, 5, 15, 20}, new[] {15d, 30}, TestName = "Both splits contain left edge gaps")]
    [TestCase(30, new[] {10d, 15, 25, 30}, new[]{15d, 30}, TestName = "Both splits contain right edge gaps")]
    [TestCase(60, new[] {10d, 15, 25, 30}, new[]{30d, 60}, TestName = "First split contains two gaps")]
    public void ShouldSplitByTrackLength(double length, double[] gapsStartEnd, double[] splits)
    {
        Assert.IsTrue(gapsStartEnd.Length % 2 == 0);
        var gaps = TestUtilities.GetGaps(gapsStartEnd);
        var original = TestUtilities.GetCoverage(length, gaps);
        
        var results = original.SplitByTrackLength(splits).ToList();
        
        Assert.AreEqual(results.Count, splits.Length);
        foreach (var result in results)
        {
            Assert.AreEqual(result.QueryLength, original.QueryLength);
            Assert.AreNotEqual(result.TrackLength, original.TrackLength);
        }

        double startsAt = 0;
        for (int index = 0; index < splits.Length; index++)
        {
            double endsAt = splits[index];
            double totalLength = endsAt - startsAt;
            var result = results[index];
            Assert.AreEqual(result.TrackLength, endsAt - startsAt, 0.0001);
            var expectedGaps = gaps.Where(_ => _.Start >= startsAt && _.End <= endsAt).ToList();
            var actualGaps = result.TrackGaps.ToList();
            Assert.AreEqual(expectedGaps.Count, actualGaps.Count);
            for (int i = 0; i < expectedGaps.Count; i++)
            {
                Assert.AreEqual(expectedGaps[i].Start - startsAt, actualGaps[i].Start, 0.1);
                Assert.AreEqual(expectedGaps[i].End - startsAt, actualGaps[i].End, 0.1);
            }
            
            Assert.AreEqual(result.TrackCoverageWithPermittedGapsLength, totalLength - expectedGaps.Sum(_ => _.LengthInSeconds), 0.15);
            startsAt = endsAt;
        }
    }

    [Test]
    public void GapIsLongerThanTheSplit()
    {
        var gaps = TestUtilities.GetGaps(new[] {15d, 30});
        var original = TestUtilities.GetCoverage(30, gaps);

        var results = original.SplitByTrackLength(new[] { 15d, 30d }).ToList();
        
        Assert.AreEqual(1, results.Count(_ => _.BestPath.Any()));
    }

    [Test]
    public void GapIsOnTheSplit()
    {
        var gaps = TestUtilities.GetGaps(new[] {12.5d, 17.5d});
        var original = TestUtilities.GetCoverage(30, gaps);

        var results = original.SplitByTrackLength(new[] { 15d, 30d }).ToList();
        
        Assert.AreEqual(2, results.Count);
        
        Assert.AreEqual(12.5, results[0].TrackCoverageWithPermittedGapsLength, 0.2);
        Assert.AreEqual(0, results[0].QueryMatchStartsAt);
        Assert.AreEqual(12.5, results[1].TrackCoverageWithPermittedGapsLength, 0.2);
        Assert.AreEqual(17.5, results[1].QueryMatchStartsAt, 0.1);
    }
}