namespace SoundFingerprinting.Math
{
    using System.Collections.Generic;
    using SoundFingerprinting.Utils;

    public class StandardGroupingCounter : IGroupingCounter
    {
        public IEnumerable<uint> GroupByAndCount(List<uint>[] results, int thresholdVotes)
        {
            return SubFingerprintGroupingCounter.GroupByAndCount(results, thresholdVotes);
        }
    }
}
