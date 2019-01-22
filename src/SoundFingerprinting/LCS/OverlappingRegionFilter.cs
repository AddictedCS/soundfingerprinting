namespace SoundFingerprinting.LCS
{
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.Query;

    internal static class OverlappingRegionFilter
    {
        private const double PermittedGap = 8192d / 5512;

        public static IEnumerable<Matches> FilterOverlappingSequences(List<Matches> sequences)
        {
            for (int i = 0; i < sequences.Count; ++i)
            {
                for (int j = i + 1; j < sequences.Count; ++j)
                {
                    if (sequences[i].TryCollapseWith(sequences[j], PermittedGap, out var c))
                    {
                        sequences.RemoveAt(j);
                        sequences.RemoveAt(i);
                        sequences.Add(c);
                        i = -1;
                        j = 0;
                        break;
                    }
                }
            }

            return sequences.OrderByDescending(sequence => sequence.EntriesCount);
        }
    }
}
