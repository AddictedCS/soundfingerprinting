namespace SoundFingerprinting.LCS
{
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.Query;

    using Math = System.Math;
        
    internal class LongestIncreasingTrackSequence : ILongestIncreasingTrackSequence
    {
        public List<Matches> FindAllIncreasingTrackSequences(IEnumerable<MatchedWith> matches, double maxGap)
        {
            var matchedWiths = new List<Matches>();
            var list = matches.OrderBy(match => match.TrackSequenceNumber)
                              .ToList();
            while (list.Any())
            {
                var orderedByTrackSequence = list.ToArray();
                MaxAt[] maxLength = BuildMaxLengthIndexArray(orderedByTrackSequence, maxGap, out var max, out var maxIndex);
                var longestSequence = FindLongestSequence(orderedByTrackSequence, maxLength, max, maxIndex, maxGap).ToList();
                matchedWiths.Add(new Matches(longestSequence));
                list = list.Except(longestSequence)
                           .OrderBy(match => match.QueryMatchAt)
                           .ToList();
            }

            return matchedWiths;
        }

        private static IEnumerable<MatchedWith> FindLongestSequence(MatchedWith[] matches, MaxAt[] maxLength, int max, int maxIndex, double allowedGap)
        {
            var lis = new Stack<MatchedWith>();
            lis.Push(matches[maxIndex]);
            max--;
            
            for (int i = maxIndex - 1; i >= 0; --i)
            {
                if (maxLength[i].Length == max)
                {
                    var prev = lis.Peek();
                    if (Math.Abs(prev.TrackMatchAt - maxLength[i].MatchedWith.TrackSequenceNumber) <= allowedGap)
                    {
                        lis.Push(matches[i]);
                        max--;
                    }
                }
            }

            while (lis.Any())
            {
                yield return lis.Pop();
            }
        }

        private static MaxAt[] BuildMaxLengthIndexArray(IReadOnlyList<MatchedWith> matches, double maxGap, out int max, out int maxIndex)
        {
            var maxLength = new MaxAt[matches.Count];
            for (int i = 0; i < maxLength.Length; ++i)
            {
                maxLength[i] = new MaxAt(0, matches[i]);
            }
            
            max = 0;
            maxIndex = 0;
            
            for (int i = 1; i < matches.Count; ++i)
            {
                for (int j = 0; j < i; ++j)
                {
                    if (matches[j].QuerySequenceNumber < matches[i].QuerySequenceNumber && maxLength[j].Length + 1 > maxLength[i].Length)
                    {
                        float queryAt = Math.Abs(matches[i].QueryMatchAt - matches[j].QueryMatchAt);
                        float resultAt = Math.Abs(matches[i].TrackMatchAt - matches[j].TrackMatchAt);
                        
                        if (queryAt <= maxGap && resultAt <= maxGap)
                        {
                            maxLength[i] = new MaxAt(maxLength[j].Length + 1, matches[i]);
                            if (maxLength[i].Length > max)
                            {
                                max = maxLength[i].Length;
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
