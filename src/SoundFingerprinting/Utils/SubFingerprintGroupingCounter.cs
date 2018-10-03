namespace SoundFingerprinting.Utils
{
    using System.Collections.Generic;
    using System.Linq;

    internal static class SubFingerprintGroupingCounter
    {
        public static IEnumerable<uint> GroupByAndCount(List<uint>[] subFingerprints, int threshold)
        {
            var counter = new Dictionary<uint, int>();
            for (int i = 0; i < subFingerprints.Length; ++i)
            {
                for (int j = 0; j < subFingerprints[i].Count; ++j)
                {
                    uint key = subFingerprints[i][j];
                    counter.TryGetValue(key, out var count);
                    counter[key] = count + 1;
                }
            }

            return counter.Where(pair => pair.Value >= threshold).Select(p => p.Key);
        }
    }
}
