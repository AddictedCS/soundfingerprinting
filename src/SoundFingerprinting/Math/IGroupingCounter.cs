namespace SoundFingerprinting.Math
{
    using SoundFingerprinting.DAO.Data;
    using System;
    using System.Collections.Generic;

    public interface IGroupingCounter
    {
        IEnumerable<SubFingerprintData> GroupByAndCount(IEnumerable<uint>[] results, int thresholdVotes, Func<uint, SubFingerprintData> resolver);
    }
}