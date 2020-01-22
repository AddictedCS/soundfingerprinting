namespace SoundFingerprinting.LCS
{
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.Query;

    internal class LongestIncreasingTrackSequence : ILongestIncreasingTrackSequence
    {
        public List<Matches> FindAllIncreasingTrackSequences(IEnumerable<MatchedWith> matches, double maxGap)
        {
            return LIS.GetIncreasingSequences(matches, maxGap)
                      .Select(list => new Matches(list))
                      .OrderByDescending(list => list.Count())
                      .ToList();
        }
    }
}
