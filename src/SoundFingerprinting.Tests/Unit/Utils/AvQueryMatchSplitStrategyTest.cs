namespace SoundFingerprinting.Tests.Unit.Utils;

using System;
using System.Linq;
using NUnit.Framework;
using SoundFingerprinting.Data;
using SoundFingerprinting.LCS;
using SoundFingerprinting.Query;
using SoundFingerprinting.Utils;

[TestFixture]
public class AvQueryMatchSplitStrategyTest
{
    [Test]
    public void ShouldSplitAvQueryMatch()
    {
        int queryLength = 120;
        var gaps = TestUtilities.GetGaps([40d, 60]);
        var coverage = TestUtilities.GetCoverage(queryLength, queryLength, queryLength, gaps);
        
        var track = new TrackInfo("1", string.Empty, string.Empty);
        var avQueryMatch = new AVQueryMatch("123", new QueryMatch("321", track, coverage, DateTime.UnixEpoch), null, "CNN");
        var split = AvQueryMatchSplitStrategy.Split(avQueryMatch,
            [
                new TrackInfo("2", string.Empty, string.Empty),
                new TrackInfo("3", string.Empty, string.Empty),
                new TrackInfo("4", string.Empty, string.Empty)
            ], [
                new TimeSegment(0, 30),
                new TimeSegment(30, 60),
                new TimeSegment(60, 120)
            ], 0.4d)
            .ToList();

		Assert.That(split, Has.Count.EqualTo(2));
		Assert.Multiple(() =>
		{
			Assert.That(split[0].Audio?.Coverage.TrackLength ?? 0, Is.EqualTo(30).Within(0.1));
			Assert.That(split[0].TrackId, Is.EqualTo("2"));
			Assert.That(split[0].StreamId, Is.EqualTo("CNN"));
			Assert.That(split[0].Audio?.MatchedAt, Is.EqualTo(DateTime.UnixEpoch));
			Assert.That(split[0].Video, Is.Null);

			Assert.That(split[1].Audio?.Coverage.TrackLength ?? 0, Is.EqualTo(60).Within(0.1));
			Assert.That(split[1].StreamId, Is.EqualTo("CNN"));
			Assert.That(split[1].TrackId, Is.EqualTo("4"));
			Assert.That(DateTime.UnixEpoch.AddSeconds(60).Subtract(split[1].Audio?.MatchedAt ?? DateTime.MinValue).TotalSeconds, Is.EqualTo(0).Within(0.1));
			Assert.That(split[1].Video, Is.Null);
		});
	}
}