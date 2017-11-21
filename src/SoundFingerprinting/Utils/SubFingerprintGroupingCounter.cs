using System.Collections.Generic;

namespace SoundFingerprinting.Utils
{
    using System.Linq;

    internal static class SubFingerprintGroupingCounter
    {
        public static unsafe IEnumerable<ulong> GroupByAndCount(List<ulong>[] subFingerprints, int threshold)
        {
            int totalCount = subFingerprints.Select(sub => sub.Count).Sum();
            int x = totalCount + 1;
            byte * collisions = stackalloc byte[x + 1];

            for (int i = 0; i < subFingerprints.Length; ++i)
            {
                for (int j = 0; j < subFingerprints[i].Count; ++j)
                {
                    ulong key = subFingerprints[i][j];
                    collisions[(int)key % x]++;
                }
            }
            
            var counter = new Dictionary<ulong, int>((int) (totalCount * 0.2));
            for (int i = 0; i < subFingerprints.Length; ++i)
            {
                for (int j = 0; j < subFingerprints[i].Count; ++j)
                {
                    ulong key = subFingerprints[i][j];
                    if (collisions[(int)key % x] >= threshold) 
                    {
                        counter.TryGetValue(key, out var count);
                        counter[key] = count + 1;
                    }
                }
            }

            return counter.Where(pair => pair.Value >= threshold).Select(p => p.Key);
        }
    }
}
