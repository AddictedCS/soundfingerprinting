namespace SoundFingerprinting.LCS
{
    using System.Collections.Generic;

    using SoundFingerprinting.Query;

    internal interface ILongestIncreasingTrackSequence
    {
        List<Matches> FindAllIncreasingTrackSequences(IEnumerable<MatchedWith> matches, double permittedGap);
    }
}