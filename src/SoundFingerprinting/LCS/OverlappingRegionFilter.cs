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
    }
}
