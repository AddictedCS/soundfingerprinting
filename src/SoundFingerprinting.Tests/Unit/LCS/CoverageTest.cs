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
		Assert.That(gapsStartEnd.Length % 2, Is.EqualTo(0));
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

		Assert.That(splits.Length, Is.EqualTo(results.Count));
        foreach (var result in results)
        {
			Assert.Multiple(() =>
			{
				Assert.That(original.QueryLength, Is.EqualTo(result.QueryLength));
				Assert.That(original.TrackLength, Is.Not.EqualTo(result.TrackLength));
			});
		}

        double startsAt = 0;
        for (int index = 0; index < splits.Length; index++)
        {
            double endsAt = splits[index];
            double totalLength = endsAt - startsAt;
            var result = results[index];
			Assert.That(endsAt - startsAt, Is.EqualTo(result.TrackLength).Within(0.0001));
            var expectedGaps = gaps.Where(_ => _.Start >= startsAt && _.End <= endsAt).ToList();
            var actualGaps = result.TrackGaps.ToList();
			Assert.That(actualGaps, Has.Count.EqualTo(expectedGaps.Count));
            for (int i = 0; i < expectedGaps.Count; i++)
            {
				Assert.Multiple(() =>
				{
					Assert.That(actualGaps[i].Start, Is.EqualTo(expectedGaps[i].Start - startsAt).Within(0.1));
					Assert.That(actualGaps[i].End, Is.EqualTo(expectedGaps[i].End - startsAt).Within(0.1));
				});
			}

			Assert.That(totalLength - expectedGaps.Sum(_ => _.LengthInSeconds), Is.EqualTo(result.TrackCoverageWithPermittedGapsLength).Within(0.15));
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

		Assert.That(results.Count(_ => _.BestPath.Any()), Is.EqualTo(1));
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

		Assert.That(results, Has.Count.EqualTo(2));

		Assert.Multiple(() =>
		{
			Assert.That(results[0].TrackCoverageWithPermittedGapsLength, Is.EqualTo(12.5).Within(0.2));
			Assert.That(results[0].QueryMatchStartsAt, Is.EqualTo(0));
			Assert.That(results[1].TrackCoverageWithPermittedGapsLength, Is.EqualTo(12.5).Within(0.2));
			Assert.That(results[1].QueryMatchStartsAt, Is.EqualTo(17.5).Within(0.1));
		});
	}

    [Test]
    public void SplitIsInsideTheCoverage()
    {
        var gaps = TestUtilities.GetGaps([12.5d, 17.5d]);
        var original = TestUtilities.GetCoverage(30, 30, 30, gaps);

        var results = original.SplitByTrackLength([new TimeSegment(2.5, 32.5)]).ToList();

		Assert.That(results, Has.Count.EqualTo(1));
        var coverage = results[0];
		Assert.Multiple(() =>
		{
			Assert.That(coverage.TrackCoverageWithPermittedGapsLength, Is.EqualTo(30 - 5 /*mid gap*/ - 2.5 /*end gap*/ + 1.48).Within(0.25));
			Assert.That(coverage.TrackLength, Is.EqualTo(30).Within(0.2));
		});
	}
}