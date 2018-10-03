namespace SoundFingerprinting.Math
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Utils;

    public class StandardGroupingCounter : IGroupingCounter
    {
        public IEnumerable<SubFingerprintData> GroupByAndCount(List<uint>[] results, int thresholdVotes, Func<uint, SubFingerprintData> resolver)
        {
            var ids = SubFingerprintGroupingCounter.GroupByAndCount(results, thresholdVotes);
            return ids.Select(resolver);
        }
    }
}
