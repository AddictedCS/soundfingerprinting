namespace SoundFingerprinting.Tests.Unit.LCS;

using System.Linq;
using NUnit.Framework;
using SoundFingerprinting.LCS;
using SoundFingerprinting.Query;

public class SingleQueryPathReconstructionStrategyTest
{
    private readonly IQueryPathReconstructionStrategy singlePathReconstructionStrategy = new SingleQueryPathReconstructionStrategy();
    
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
        
        var result = singlePathReconstructionStrategy.GetBestPaths(matchedWiths, int.MaxValue).First().ToList();
        
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
        
        var result = singlePathReconstructionStrategy.GetBestPaths(matchedWiths, int.MaxValue).First().ToList();
        
        CollectionAssert.AreEqual(new[] {1, 2, 3, 4}, result.Select(_ => (int)_.QuerySequenceNumber));
        CollectionAssert.AreEqual(new[] {1, 1, 1, 4}, result.Select(_ => (int)_.TrackSequenceNumber));
    }
}