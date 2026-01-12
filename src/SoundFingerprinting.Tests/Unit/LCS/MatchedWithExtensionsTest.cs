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

		Assert.That(result, Is.Empty);
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

		Assert.That(matches, Has.Count.EqualTo(1));
		Assert.That(matches[0].Count(), Is.EqualTo(9));
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

		Assert.That(matches, Has.Count.EqualTo(3));
		Assert.That(matches[0].Select(_ => _.QueryMatchAt), Is.EqualTo(new float[] { 0, 1, 2 }).AsCollection);
		Assert.That(matches[1].Select(_ => _.QueryMatchAt), Is.EqualTo(new float[] { 10, 11, 12, 13 }).AsCollection);
		Assert.That(matches[2].Select(_ => _.QueryMatchAt), Is.EqualTo(new float[] { 24, 25, 26 }).AsCollection);
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

		Assert.That(matches, Has.Count.EqualTo(2));
		Assert.That(matches[0].Select(_ => _.QueryMatchAt), Is.EqualTo(new float[] { 0, 1, 2 }).AsCollection);
		Assert.That(matches[1].Select(_ => _.QueryMatchAt), Is.EqualTo(new float[] { 10, 11, 12 }).AsCollection);
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

		Assert.That(matches, Has.Count.EqualTo(2));
		Assert.That(matches[0].Select(_ => _.QueryMatchAt), Is.EqualTo(new float[] { 0, 1, 2 }).AsCollection);
		Assert.That(matches[1].Select(_ => _.QueryMatchAt), Is.EqualTo(new float[] { 10 }).AsCollection);
    }
}