namespace SoundFingerprinting.LCS
{
    using System.Collections.Generic;

    using SoundFingerprinting.Query;

    internal interface ILongestIncreasingTrackSequence
    {
        List<List<MatchedWith>> FindAllIncreasingTrackSequences(IEnumerable<MatchedWith> matches);
    }
}