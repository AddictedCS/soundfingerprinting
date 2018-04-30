using System.Collections.Generic;

namespace SoundFingerprinting.LCS
{
    using System.Linq;

    using SoundFingerprinting.Query;

    internal class LongestIncreasingTrackSequence : ILongestIncreasingTrackSequence
    {
        private const float AllowedMismatchLength = 1.48f;

        public List<List<MatchedWith>> FindAllIncreasingTrackSequences(IEnumerable<MatchedWith> matches)
        {
            var matchedWiths = new List<List<MatchedWith>>();
            var orderedByQueryAt = matches.ToList();

            while (orderedByQueryAt.Any())
            {
                int[] maxLength = new int[orderedByQueryAt.Count];
                int max = 0, maxIndex = 0;

                for (int i = 1; i < orderedByQueryAt.Count; ++i)
                {
                    for (int j = 0; j < i; ++j)
                    {
                        if (orderedByQueryAt[j].ResultAt < orderedByQueryAt[i].ResultAt && maxLength[j] + 1 > maxLength[i])
                        {
                            float queryAt = System.Math.Abs(orderedByQueryAt[i].QueryAt - orderedByQueryAt[j].QueryAt);
                            float resultAt = System.Math.Abs(orderedByQueryAt[i].ResultAt - orderedByQueryAt[j].ResultAt);
                            if (System.Math.Abs(queryAt - resultAt) < AllowedMismatchLength)
                            {
                                maxLength[i] = maxLength[j] + 1;
                                if (maxLength[i] > max)
                                {
                                    max = maxLength[i];
                                    maxIndex = i;
                                }
                            }
                        }
                    }
                }

                var used = new HashSet<int>();
                var currentList = new List<MatchedWith>();

                for (int i = maxIndex; i >= 0; --i)
                {
                    if (maxLength[i] == max)
                    {
                        currentList.Add(orderedByQueryAt[i]);
                        max--;
                        used.Add(i);
                    }
                }

                foreach (var toRemove in used)
                {
                    orderedByQueryAt.RemoveAt(toRemove);
                }

                currentList.Reverse();
                matchedWiths.Add(currentList);
            }

            return matchedWiths;
        }
    }
}
