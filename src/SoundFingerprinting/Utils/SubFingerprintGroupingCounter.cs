using System.Collections.Generic;

namespace SoundFingerprinting.Utils
{
    using System.Linq;

    public static class SubFingerprintGroupingCounter
    {
        public static Dictionary<ulong, int> GroupByAndCount(List<ulong>[] subFingerprints)
        {
            int maxLength = subFingerprints.Select(sub => sub.Count).Sum();
            var counter = new Dictionary<ulong, int>(maxLength);
            for (int i = 0; i < subFingerprints.Length; ++i)
            {
                for (int j = 0; j < subFingerprints[i].Count; ++j)
                {
                    ulong key = subFingerprints[i][j];
                    counter.TryGetValue(key, out var count);
                    counter[key] = count + 1;
                }
            }

            return counter;
        }
    }
}
