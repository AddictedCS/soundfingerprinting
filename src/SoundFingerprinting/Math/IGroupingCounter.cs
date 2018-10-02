namespace SoundFingerprinting.Math
{
    using System;
    using System.Collections.Generic;

    using SoundFingerprinting.DAO.Data;

    public interface IGroupingCounter
    {
        IEnumerable<SubFingerprintData> GroupByAndCount(List<ulong>[] results, int thresholdVotes, Func<ulong, SubFingerprintData> resolver);
    }
}
