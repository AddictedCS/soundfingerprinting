namespace SoundFingerprinting.Tests.Unit.LCS;

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SoundFingerprinting.LCS;

[TestFixture]
public class CoverageTest
{
    [TestCase(30, new double[0],               new[] { 15d, 30d }, TestName = "Clean split no gaps")]
    [TestCase(30, new[] { 5d, 10d },           new[] { 15d, 30d }, TestName = "First split contains a gap, second does not")]
    [TestCase(30, new[] { 5d, 10d, 20d, 25d }, new[] { 15d, 30d }, TestName = "Both splits contain gaps")]
    [TestCase(30, new[] { 0d, 5, 15, 20 },     new[] { 15d, 30 },  TestName = "Both splits contain left edge gaps")]
    [TestCase(30, new[] { 10d, 15, 25, 30 },   new[] { 15d, 30 },  TestName = "Both splits contain right edge gaps")]
    [TestCase(60, new[] { 10d, 15, 25, 30 },   new[] { 30d, 60 },  TestName = "First split contains two gaps")]
    public void ShouldSplitByTrackLength(double length, double[] gapsStartEnd, double[] splits)
    {
        Assert.IsTrue(gapsStartEnd.Length % 2 == 0);
        var gaps = TestUtilities.GetGaps(gapsStartEnd);
        var original = TestUtilities.GetCoverage(length, queryLength: length, trackLength: length, gaps);
        var timeSegments = splits.Aggregate(new List<TimeSegment>(), (acc, next) =>
        {
            if (!acc.Any())
            {
                acc.Add(new TimeSegment(0, next));
                return acc;
            }

            var last = acc.Last();
            acc.Add(new TimeSegment(last.EndsAt, next));
            return acc;
        });
        
        var results = original.SplitByTrackLength(timeSegments).ToList();
        
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
        var gaps = TestUtilities.GetGaps([15d, 30]);
        var original = TestUtilities.GetCoverage(30, 30, 30, gaps);

        var results = original.SplitByTrackLength([
            new TimeSegment(0, 15), 
            new TimeSegment(15, 30)
        ]).ToList();
        
        Assert.AreEqual(1, results.Count(_ => _.BestPath.Any()));
    }

    [Test]
    public void GapIsOnTheSplit()
    {
        var gaps = TestUtilities.GetGaps([12.5d, 17.5d]);
        var original = TestUtilities.GetCoverage(30, 30, 30, gaps);

        var results = original.SplitByTrackLength([
            new TimeSegment(0, 15), 
            new TimeSegment(15, 30)
        ]).ToList();
        
        Assert.AreEqual(2, results.Count);
        
        Assert.AreEqual(12.5, results[0].TrackCoverageWithPermittedGapsLength, 0.2);
        Assert.AreEqual(0, results[0].QueryMatchStartsAt);
        Assert.AreEqual(12.5, results[1].TrackCoverageWithPermittedGapsLength, 0.2);
        Assert.AreEqual(17.5, results[1].QueryMatchStartsAt, 0.1);
    }

    [Test]
    public void SplitIsInsideTheCoverage()
    {
        var gaps = TestUtilities.GetGaps([12.5d, 17.5d]);
        var original = TestUtilities.GetCoverage(30, 30, 30, gaps);

        var results = original.SplitByTrackLength([new TimeSegment(2.5, 32.5)]).ToList();
        
        Assert.AreEqual(1, results.Count);
        var coverage = results[0];
        Assert.AreEqual(30 - 5 /*mid gap*/ - 2.5 /*end gap*/ + 1.48 /*last fingerprint*/, coverage.TrackCoverageWithPermittedGapsLength, 0.25);
        Assert.AreEqual(30, coverage.TrackLength, 0.2);
    }
}