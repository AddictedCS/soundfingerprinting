namespace SoundFingerprinting.Tests.Unit.LCS;

using System.Linq;
using NUnit.Framework;
using SoundFingerprinting.LCS;
using SoundFingerprinting.Query;

public class SingleQueryPathReconstructionStrategyTest
{
    private readonly IQueryPathReconstructionStrategy singlePathReconstructionStrategy = new QueryPathReconstructionStrategy();

    [Test]
    public void ShouldNotThrowWhenEmptyIsPassed()
    {
        var result = singlePathReconstructionStrategy.GetBestPaths(Enumerable.Empty<MatchedWith>(), int.MaxValue, limit: 1);
        
        CollectionAssert.IsEmpty(result);
    }

    /*
     * q         1 2 3 7 8 4 5 6 7 8 2 3 9
     * t         1 2 3 2 3 4 5 6 7 8 7 8 9
     */
    [Test(Description = "Cross match (2,7) and (3,8) between query and track should be ignored")]
    public void ShouldIgnoreRepeatingCrossMatches()
    {
        var matchedWiths = new[] { (1, 1), (2, 2), (3, 3), (7, 2), (8, 3), (4, 4), (5, 5), (6, 6), (7, 7), (8, 8), (2, 7), (3, 8), (9, 9) }
            .Select(tuple => new MatchedWith((uint)tuple.Item1, tuple.Item1, (uint)tuple.Item2, tuple.Item2, 0d));
        
        var result = singlePathReconstructionStrategy.GetBestPaths(matchedWiths, maxGap: 10, limit: 1).First().ToList();
        
        CollectionAssert.AreEqual(new[] {1, 2, 3, 4, 5, 6, 7, 8, 9}, result.Select(_ => (int)_.QuerySequenceNumber));
        CollectionAssert.AreEqual(new[] {1, 2, 3, 4, 5, 6, 7, 8, 9}, result.Select(_ => (int)_.TrackSequenceNumber)); 
    }
    
    /*
     * q         1 1 1 4
     * t         1 2 3 4
     * expected  x x x x
     * max       1 1 1 2
     */
    [Test]
    public void ShouldPickAllQueryCandidates()
    {
        var matchedWiths = new[] { (1, 1), (1, 2), (1, 3), (4, 4) }.Select(tuple => new MatchedWith((uint)tuple.Item1, tuple.Item1, (uint)tuple.Item2, tuple.Item2, 0d));
        
        var result = singlePathReconstructionStrategy.GetBestPaths(matchedWiths, maxGap: 5, limit: 1).First().ToList();
        
        CollectionAssert.AreEqual(new[] {1, 1, 1, 4}, result.Select(_ => (int)_.QuerySequenceNumber));
        CollectionAssert.AreEqual(new[] {1, 2, 3, 4}, result.Select(_ => (int)_.TrackSequenceNumber));
    }
    
    /*
    * q         1 2 3 4
    * t         1 1 1 4
    * expected  x x x x
    * max       1 1 1 2
    */
    [Test]
    public void ShouldPickAllTrackCandidates()
    {
        var matchedWiths = new[] { (1, 1), (2, 1), (3, 1), (4, 4) }.Select(tuple => new MatchedWith((uint)tuple.Item1, tuple.Item1, (uint)tuple.Item2, tuple.Item2, 0d));
        
        var result = singlePathReconstructionStrategy.GetBestPaths(matchedWiths, maxGap: 5, limit: 1).First().ToList();
        
        CollectionAssert.AreEqual(new[] {1, 2, 3, 4}, result.Select(_ => (int)_.QuerySequenceNumber));
        CollectionAssert.AreEqual(new[] {1, 1, 1, 4}, result.Select(_ => (int)_.TrackSequenceNumber));
    }
}