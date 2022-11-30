namespace SoundFingerprinting.Tests.Unit.LCS;

using System.Linq;
using NUnit.Framework;
using SoundFingerprinting.Query;

[TestFixture]
public class MatchedWithExtensionsTest
{
    [Test]
    public void ShouldSplitByMaxGapEmpty()
    {
        var result = Enumerable.Empty<MatchedWith>().SplitBestPathByMaxGap(5d);
        
        Assert.IsEmpty(result);
    }

    [Test]
    public void ShouldSplitByMaxGap1()
    {
        var matches = new[]
        {
            (1, 1), (2, 2), (3, 3), (4, 4), (5, 5), (6, 6), (7, 7), (8, 8), (9, 9)
        }
        .Select(tuple => new MatchedWith((uint)tuple.Item1, tuple.Item1, (uint)tuple.Item2, tuple.Item2, 0d))
        .SplitBestPathByMaxGap(maxGap: 5)
        .ToList();
        
        Assert.AreEqual(1, matches.Count);
        Assert.AreEqual(9, matches[0].Count());
    }

    [Test]
    public void ShouldSplitByMaxGap2()
    {
        var matches = TestUtilities
            .GetMatchedWith(
                new[] { 0, 1, 2, 10, 11, 12, 13, 24, 25, 26 },
                new[] { 1, 2, 3, 1, 2, 3, 4, 1, 2, 3 })
            .SplitBestPathByMaxGap(maxGap: 5)
            .ToList();
        
        Assert.AreEqual(3, matches.Count);
        CollectionAssert.AreEqual(new float[] { 0, 1, 2 }, matches[0].Select(_ => _.QueryMatchAt));
        CollectionAssert.AreEqual(new float[] { 10, 11, 12, 13 }, matches[1].Select(_ => _.QueryMatchAt));
        CollectionAssert.AreEqual(new float[] { 24, 25, 26 }, matches[2].Select(_ => _.QueryMatchAt));
    }

    [Test]
    public void ShouldSplitByMaxGap3()
    {
        var matches = TestUtilities
            .GetMatchedWith(
                new[] { 0, 1, 2, 10, 11, 12 },
                new[] { 1, 2, 3, 4, 5, 6 })
            .SplitBestPathByMaxGap(maxGap: 5)
            .ToList();

        Assert.AreEqual(2, matches.Count);
        CollectionAssert.AreEqual(new float[] { 0, 1, 2 }, matches[0].Select(_ => _.QueryMatchAt));
        CollectionAssert.AreEqual(new float[] { 10, 11, 12 }, matches[1].Select(_ => _.QueryMatchAt));
    }
    
    [Test]
    public void ShouldSplitByMaxGap4()
    {
        var matches = TestUtilities
            .GetMatchedWith(
                new[] { 0, 1, 2, 10},
                new[] { 1, 2, 3, 11 })
            .SplitBestPathByMaxGap(maxGap: 5)
            .ToList();

        Assert.AreEqual(2, matches.Count);
        CollectionAssert.AreEqual(new float[] { 0, 1, 2 }, matches[0].Select(_ => _.QueryMatchAt));
        CollectionAssert.AreEqual(new float[] { 10 }, matches[1].Select(_ => _.QueryMatchAt));
    }
}