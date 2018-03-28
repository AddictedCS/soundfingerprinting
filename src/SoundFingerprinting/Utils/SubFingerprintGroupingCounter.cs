using System.Collections.Generic;

namespace SoundFingerprinting.Utils
{
    using System.Linq;

    internal static class SubFingerprintGroupingCounter
    {
        public static unsafe IEnumerable<ulong> GroupByAndCount(List<ulong>[] subFingerprints, int threshold)
        {
            var counter = new Dictionary<ulong, int>();
            for (int i = 0; i < subFingerprints.Length; ++i)
            {
                for (int j = 0; j < subFingerprints[i].Count; ++j)
                {
                    ulong key = subFingerprints[i][j];
                    counter.TryGetValue(key, out var count);
                    counter[key] = count + 1;
                }
            }

            return counter.Where(pair => pair.Value >= threshold).Select(p => p.Key);
        }
    }
}
