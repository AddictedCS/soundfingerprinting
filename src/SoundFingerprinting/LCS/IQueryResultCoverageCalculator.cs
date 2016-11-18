namespace SoundFingerprinting.LCS
{
    using System.Collections.Generic;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Query;

    internal interface IQueryResultCoverageCalculator
    {
        Coverage GetCoverage(SortedSet<MatchedPair> matches, double queryLength, FingerprintConfiguration configuration);
    }
}