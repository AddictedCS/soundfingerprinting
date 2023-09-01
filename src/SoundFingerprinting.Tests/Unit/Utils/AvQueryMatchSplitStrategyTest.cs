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
        var gaps = TestUtilities.GetGaps(new[] {40d, 60});
        var coverage = TestUtilities.GetCoverage(queryLength, gaps);
        
        var track = new TrackInfo("1", string.Empty, string.Empty);
        var avQueryMatch = new AVQueryMatch("123", new QueryMatch("321", track, coverage, DateTime.UnixEpoch), null, "CNN");

        var split = AvQueryMatchSplitStrategy.Split(avQueryMatch,
                new TrackInfo[]
                {
                    new TrackInfo("2", string.Empty, string.Empty),
                    new TrackInfo("3", string.Empty, string.Empty),
                    new TrackInfo("4", string.Empty, string.Empty)
                }, new[]
                {
                    new TimeSegment(0, 30),
                    new TimeSegment(30, 60),
                    new TimeSegment(60, 120)
                }, 0.4d)
            .ToList();
        
        Assert.AreEqual(2, split.Count);
        Assert.AreEqual(30, split[0].Audio?.Coverage.TrackLength ?? 0, 0.1);
        Assert.AreEqual("2", split[0].TrackId);
        Assert.AreEqual("CNN", split[0].StreamId);
        Assert.AreEqual(DateTime.UnixEpoch, split[0].Audio?.MatchedAt);
        Assert.IsNull(split[0].Video);
        
        Assert.AreEqual(60, split[1].Audio?.Coverage.TrackLength ?? 0, 0.1);
        Assert.AreEqual("CNN", split[1].StreamId);
        Assert.AreEqual("4", split[1].TrackId);
        Assert.AreEqual(DateTime.UnixEpoch.AddSeconds(60).Subtract(split[1].Audio?.MatchedAt ?? DateTime.MinValue).TotalSeconds, 0, 0.1);
        Assert.IsNull(split[1].Video);
    }
}