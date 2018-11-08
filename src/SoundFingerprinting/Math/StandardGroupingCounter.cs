namespace SoundFingerprinting.Math
{
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Utils;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class StandardGroupingCounter : IGroupingCounter
    {
        public IEnumerable<SubFingerprintData> GroupByAndCount(IEnumerable<uint>[] results, int thresholdVotes, Func<uint, SubFingerprintData> resolver)
        {
            var ids = SubFingerprintGroupingCounter.GroupByAndCount(results, thresholdVotes);
            return ids.Select(resolver);
        }
    }
}