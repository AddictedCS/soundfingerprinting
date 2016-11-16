namespace SoundFingerprinting.LCS
{
    using System.Collections.Generic;

    using SoundFingerprinting.Query;

    internal interface IQueryResultCoverageCalculator
    {
        Coverage GetLongestMatch(SortedSet<MatchedPair> matches, double queryLength, double oneFingerprintCoverage);
    }
}