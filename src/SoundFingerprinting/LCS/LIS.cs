namespace SoundFingerprinting.LCS
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.Query;

    public class LIS
    {
        /// <summary>
        ///  Gets longest increasing sequences in the matches candidates
        /// </summary>
        /// <param name="matched">All matched candidates</param>
        /// <param name="maxGap">Max gap (i.e. Math.Min(trackLength, queryLength)</param>
        /// <returns>All available sequences</returns>
        public static IEnumerable<IEnumerable<MatchedWith>> GetIncreasingSequences(IEnumerable<MatchedWith> matched, double maxGap = int.MaxValue)
        {
            var matchedWiths = matched.ToList();
            var results = new List<IEnumerable<MatchedWith>>();
            while (true)
            {
                var (result, exclude) = GetLongestIncreasingSequence(matchedWiths, maxGap);
                var withs = result as MatchedWith[] ?? result.ToArray();
                if (withs.Any())
                {
                    results.Add(withs);
                }

                var second = exclude as MatchedWith[] ?? exclude.ToArray();
                if (!withs.Any() || !second.Any())
                    break;

                matchedWiths = matchedWiths.Except(second).ToList();
            }

            return results.OrderByDescending(_ => _.Count());
        }

        private static IEnumerable<IEnumerable<MatchedWith>> GetLongestIncreasingSequences(IEnumerable<MatchedWith> matched, double maxGap)
        {
            var matches = matched.OrderBy(x => x.TrackSequenceNumber).ToList();
            var dp = new MatchedWith[matches.Count][];
            int[] len = new int[matches.Count];
            var comparer = Comparer<MatchedWith>.Create((a, b) =>
            {
                if (a.QuerySequenceNumber > b.QuerySequenceNumber)
                    return 1;
                return -1;
            });

            int totalSequences = 0;
            foreach (var x in matches)
            {
                int t = totalSequences;
                for (int j = 0; j < t + 1 /*allow one sequence to be added at a time*/; ++j)
                {
                    if (dp[j] == null)
                    {
                        // if not initialized, initialize
                        dp[j] = new MatchedWith[matches.Count];
                        totalSequences++; // one more sequence added
                    }

                    int i = Array.BinarySearch(dp[j], 0, len[j], x, comparer);

                    if (i < 0)
                        i = -(i + 1);

                    // if not same sequence, create new one, or find the sequence which corresponds to it
                    if (i > 0 && !IsSameSequence(x, dp[j][i - 1], maxGap))
                        continue;

                    // sequence to be increase found
                    dp[j][i] = x;
                    if (i == len[j])
                        len[j]++; // new max found
                    break;
                }
            }

            return dp.Take(totalSequences)
                .Select((x, index) => x.Take(len[index]));
        }


        private static (IEnumerable<MatchedWith>, IEnumerable<MatchedWith>) GetLongestIncreasingSequence(IEnumerable<MatchedWith> matched, double maxGap)
        {
            // locking first dimension - track sequence number
            var matches = matched.OrderBy(x => x.TrackSequenceNumber).ToList();

            if (!matches.Any())
            {
                return (Enumerable.Empty<MatchedWith>(), Enumerable.Empty<MatchedWith>());
            }

            var maxs = MaxIncreasingQuerySequenceOptimal(matches, maxGap, out int max, out int maxIndex);
            var exclude = new List<MatchedWith>();
            var lis = new Stack<MaxAt>();
            for (int i = maxIndex; i >= 0 && max > 0; --i)
            {
                var current = maxs[i];
                if (current.Length == max) // found a potential entry to insert into the final list
                {
                    if (lis.TryPeek(out var lastPicked) && !IsSameSequence(current.MatchedWith, lastPicked.MatchedWith, maxGap))
                    {
                        // entry is not from the same list of increasing candidates
                        continue;
                    }

                    max--;
                    exclude.Add(current.MatchedWith);
                    
                    // get all candidates in the current region that did not increase max length
                    // pick best by score
                    for (int j = i - 1; j >= 0 && current.Length == maxs[j].Length; --j)
                    {
                        // select best candidate by score and from the same sequence
                        if (IsSameSequence(current.MatchedWith, maxs[j].MatchedWith, maxGap))
                        {
                            exclude.Add(maxs[j].MatchedWith);
                            current = current.MatchedWith.Score < maxs[j].MatchedWith.Score ? maxs[j] : current;
                        }
                    }

                    lis.Push(current);
                }
                else
                {
                    // out of order element need to be excluded during next iteration
                    var lastPicked = lis.Peek();
                    if (IsSameSequence(lastPicked.MatchedWith, current.MatchedWith, maxGap))
                    {
                        exclude.Add(current.MatchedWith);
                    }
                }
            }

            return (CaptureResult(lis), exclude);
        }

        private static IEnumerable<MatchedWith> CaptureResult(Stack<MaxAt> lis)
        {
            var result = new List<MatchedWith>();
            while (lis.TryPop(out var current))
            {
                while (lis.TryPeek(out var lookAhead) && current.MatchedWith.TrackSequenceNumber == lookAhead.MatchedWith.TrackSequenceNumber)
                {
                    // lis peaked same track sequence entries, let's find best candidate of them all
                    current = current.MatchedWith.Score < lookAhead.MatchedWith.Score ? lookAhead : current;
                    lis.Pop();
                }

                result.Add(current.MatchedWith);
            }

            return result;
        }

        private static MaxAt[] MaxIncreasingQuerySequenceOptimal(IReadOnlyList<MatchedWith> matches, double maxGap, out int max, out int maxIndex)
        {
            var maxs = matches.Select(_ => new MaxAt(1, _)).ToArray();
            var dp = new MatchedWith[matches.Count];
            int len = 0;
            max = 1;
            maxIndex = 0;

            var comparer = Comparer<MatchedWith>.Create((a, b) => a.QuerySequenceNumber.CompareTo(b.QuerySequenceNumber));

            for (int j = 0; j < matches.Count; ++j)
            {
                int i = Array.BinarySearch(dp, 0, len, matches[j], comparer);
                if (i < 0)
                    i = -(i + 1);

                if (i > 0 && !IsSameSequence(dp[i - 1], matches[j], maxGap))
                    continue;
                if (i == 0 && dp[i] != null && !IsSameSequence(dp[0], matches[j], maxGap))
                    continue;

                dp[i] = matches[j];

                if (i >= len - 1)
                {
                    // the sequence has increased or found an equal element at the very end
                    len = i == len ? len + 1 : len;
                    maxs[j] = new MaxAt(len, matches[j]);
                    maxIndex = j;
                    max = len;
                }
                else
                {
                    // the sequence does not increase
                    maxs[j] = new MaxAt(maxs[j].Length /*copy value so far*/, matches[j]);
                }
            }

            return maxs;
        }

        private static bool IsSameSequence(MatchedWith a, MatchedWith b, double maxGap)
        {
            return Math.Abs(a.QueryMatchAt - b.QueryMatchAt) <= maxGap && Math.Abs(a.TrackMatchAt - b.TrackMatchAt) <= maxGap;
        }
    }
}