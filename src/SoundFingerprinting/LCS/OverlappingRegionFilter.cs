namespace SoundFingerprinting.LCS
{
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Query;

    internal static class OverlappingRegionFilter
    {
        public static IEnumerable<List<MatchedWith>> FilterOverlappingSequences(List<List<MatchedWith>> sequences)
        {
            for (int i = 0; i < sequences.Count; ++i)
            {
                var a = sequences[i];
                for (int j = i + 1; j < sequences.Count; ++j)
                {
                    var b = sequences[j];
                    float start = b.First().QueryAt;
                    float end = b.Last().QueryAt;
                    var previous = a;

                    float prevStart = previous.First().QueryAt;
                    float prevEnd = previous.Last().QueryAt;

                    if (prevStart < end && end <= prevEnd || prevStart < start && start <= prevEnd || start < prevStart && prevEnd <= end)
                    {
                        a = Concatenate(a, b);
                        sequences.RemoveAt(j);
                        sequences.RemoveAt(i);
                        sequences.Add(a);
                        return FilterOverlappingSequences(sequences.OrderByDescending(seq => seq.Count).ToList());
                    }
                }
            }

            return sequences;
        }

        private static List<MatchedWith> Concatenate(List<MatchedWith> a, List<MatchedWith> b)
        {
            var concatenated = new List<MatchedWith>();
            int ai = 0, bi = 0;
            while (ai < a.Count || bi < b.Count)
            {
                if (ai >= a.Count)
                    concatenated.Add(b[bi++]);
                else if (bi >= b.Count)
                    concatenated.Add(a[ai++]);
                else if (a[ai].QueryAt < b[bi].QueryAt)
                    concatenated.Add(a[ai++]);
                else concatenated.Add(b[bi++]);
            }

            return concatenated;
        }
    }
}
