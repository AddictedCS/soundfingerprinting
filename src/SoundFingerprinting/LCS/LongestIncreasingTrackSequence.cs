using System.Collections.Generic;

namespace SoundFingerprinting.LCS
{
    using SoundFingerprinting.Query;

    internal class LongestIncreasingTrackSequence : ILongestIncreasingTrackSequence
    {
        public List<MatchedWith[]> FindAllIncreasingTrackSequences(IEnumerable<MatchedWith> matches)
        {
            return null;
        }
    }

    internal interface ILongestIncreasingTrackSequence
    {
        List<MatchedWith[]> FindAllIncreasingTrackSequences(IEnumerable<MatchedWith> matches);
    }
}
