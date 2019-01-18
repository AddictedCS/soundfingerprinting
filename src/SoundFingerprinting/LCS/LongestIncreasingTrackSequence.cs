namespace SoundFingerprinting.LCS
{
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Query;

    using Math = System.Math;
        
    internal class LongestIncreasingTrackSequence : ILongestIncreasingTrackSequence
    {
        private const float AllowedMismatchLength = 1.48f;

        public List<List<MatchedWith>> FindAllIncreasingTrackSequences(IEnumerable<MatchedWith> matches)
        {
            var matchedWiths = new List<List<MatchedWith>>();
            var list = matches.OrderBy(match => match.QueryAt).ToList();
            while (list.Any())
            {
                var orderedByQueryAt = list.ToArray();
                int[] maxLength = BuildMaxLengthIndexArray(orderedByQueryAt, out var max, out var maxIndex);
                var longestSequence = FindLongestSequence(orderedByQueryAt, maxLength, max, maxIndex);
                matchedWiths.Add(longestSequence);
                list = list.Except(longestSequence)
                           .OrderBy(match => match.QueryAt)
                           .ToList();
            }

            return matchedWiths;
        }

        private static List<MatchedWith> FindLongestSequence(MatchedWith[] matches, int[] maxLength, int max, int maxIndex)
        {
            var currentList = new List<MatchedWith>();
            for (int i = maxIndex; i >= 0; --i)
            {
                if (maxLength[i] == max)
                {
                    currentList.Add(matches[i]);
                    max--;
                }
            }

            currentList.Reverse();
            return currentList;
        }

        private static int[] BuildMaxLengthIndexArray(IReadOnlyList<MatchedWith> matches, out int max, out int maxIndex)
        {
            int[] maxLength = new int[matches.Count];
            
            max = 0;
            maxIndex = 0;
            
            for (int i = 1; i < matches.Count; ++i)
            {
                for (int j = 0; j < i; ++j)
                {
                    if (matches[j].ResultAt < matches[i].ResultAt && maxLength[j] + 1 > maxLength[i])
                    {
                        float queryAt = Math.Abs(matches[i].QueryAt - matches[j].QueryAt);
                        float resultAt = Math.Abs(matches[i].ResultAt - matches[j].ResultAt);
                        if (Math.Abs(queryAt - resultAt) < AllowedMismatchLength)
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

            return maxLength;
        }
    }
}
