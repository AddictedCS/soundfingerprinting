namespace SoundFingerprinting.Utils
{
    using System.Collections.Generic;
    using System.Linq;

    internal static class SubFingerprintGroupingCounter
    {
        public static IEnumerable<uint> GroupByAndCount(IEnumerable<uint>[] subFingerprints, int threshold)
        {
            var counter = new Dictionary<uint, int>();
            foreach (var subFingerprint in subFingerprints)
            {
                foreach (var key in subFingerprint)
                {
                    counter.TryGetValue(key, out var count);
                    counter[key] = count + 1;
                }
            }

            return counter.Where(pair => pair.Value >= threshold).Select(p => p.Key);
        }
    }
}