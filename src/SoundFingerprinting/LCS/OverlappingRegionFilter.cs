namespace SoundFingerprinting.LCS
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Query;

    internal static class OverlappingRegionFilter
    {
        public static IEnumerable<List<MatchedWith>> FilterOverlappingSequences(List<List<MatchedWith>> sequences)
        {
            for (int i = 1; i < sequences.Count; ++i)
                if (sequences[i].Count > sequences[i - 1].Count)
                    throw new ArgumentException($"{nameof(sequences)} should be sorted by length with longest sequences comming first");

            yield return sequences[0];

            for (int i = 1; i < sequences.Count; ++i)
            {
                bool yield = true;
                for (int j = 0; j <= i - 1; ++j)
                {
                    var current = sequences[i];
                    float start = current.First().QueryAt;
                    float end = current.Last().QueryAt;

                    var previous = sequences[j];
                    float prevStart = previous.First().QueryAt;
                    float prevEnd = previous.Last().QueryAt;

                    if ((prevStart < end && end < prevEnd) || (prevStart < start && start < prevEnd))
                    {
                        yield = false;
                        break;
                    }
                }

                if (yield)
                {
                    yield return sequences[i];
                }
            }
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
