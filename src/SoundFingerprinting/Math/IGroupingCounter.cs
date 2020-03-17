namespace SoundFingerprinting.Math
{
    using System.Collections.Generic;

    public interface IGroupingCounter
    {
        IEnumerable<uint> GroupByAndCount(List<uint>[] results, int thresholdVotes);
    }
}
