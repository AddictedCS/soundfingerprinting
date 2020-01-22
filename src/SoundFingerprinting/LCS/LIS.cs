namespace SoundFingerprinting.LCS
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
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
            while (true)
            {
                var (result, exclude) = GetLongestIncreasingSequence(matchedWiths, maxGap);
                var withs = result as MatchedWith[] ?? result.ToArray();
                if (withs.Any())
                {
                    yield return withs;
                }

                var second = exclude as MatchedWith[] ?? exclude.ToArray();
                if (!withs.Any() || !second.Any())
                    break;

                matchedWiths = matchedWiths.Except(second).ToList();
            }
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

            MaxAt[] maxIncreasingQuerySequence = MaxIncreasingQuerySequenceOptimal(matches, maxGap, out int max, out int maxIndex);

            var exclude = new HashSet<MatchedWith>();
            var lis = new Stack<MaxAt>();
            for (int index = maxIndex; index >= 0 && max > 0; --index)
            {
                var current = maxIncreasingQuerySequence[index];
                if (current.Length == max) // found entry to insert into the final list
                {
                    var candidates = GetCandidatesWithEqualMaxLength(maxIncreasingQuerySequence, index)
                        .OrderByDescending(_ => _.MatchedWith.Score)
                        .ToList();

                    if (!lis.Any())
                    {
                        // select best candidate by score
                        // since no other elements are present, lets pick first
                        lis.Push(candidates.First());
                    }
                    else
                    {
                        // multiple candidates are available lets pick the best by score
                        var last = lis.Peek();
                        foreach (var maxAt in candidates)
                        {
                            // pick first best candidate from the same sequence
                            bool sameSequence = IsSameSequence(last.MatchedWith, maxAt.MatchedWith, maxGap);
                            if (sameSequence)
                            {
                                lis.Push(maxAt);
                                break;
                            }
                        }
                    }

                    // store candidates that have to be excluded from the list during next iteration
                    var lastPicked = lis.Peek();
                    foreach (var maxAt in candidates)
                    {
                        bool sameSequence = IsSameSequence(lastPicked.MatchedWith, maxAt.MatchedWith, maxGap);
                        if (sameSequence)
                        {
                            exclude.Add(maxAt.MatchedWith);
                        }
                    }

                    max--;
                }
                else
                {
                    // out of order element need to be excluded during next iteration
                    var lastPicked = lis.Peek();
                    bool sameSequence = IsSameSequence(lastPicked.MatchedWith, current.MatchedWith, maxGap);
                    if (sameSequence)
                    {
                        exclude.Add(current.MatchedWith);
                    }
                }
            }

            var result = new List<MatchedWith>();
            while (lis.Any())
            {
                var current = lis.Pop();
                var lookAhead = lis.Any() ? lis.Peek() : new MaxAt(0, null);
                var candidates = new List<MaxAt> {current};
                while (current.MatchedWith.TrackSequenceNumber == lookAhead.MatchedWith?.TrackSequenceNumber)
                {
                    // lis peeked same track sequence entries, let's find best candidate of them all
                    candidates.Add(lis.Pop());
                    lookAhead = lis.Any() ? lis.Peek() : new MaxAt(0, null);
                }

                result.Add(candidates.OrderByDescending(_ => _.MatchedWith.Score).First().MatchedWith);
            }

            return (result, exclude);
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

        private static MaxAt[] MaxIncreasingQuerySequence(IReadOnlyList<MatchedWith> matches, double maxGap, out int max, out int maxIndex)
        {
            var maxIncreasingQuerySequence = matches.Select(_ => new MaxAt(1, _)).ToArray();
            max = 1;
            maxIndex = 0;
            for (int i = 1; i < matches.Count; ++i)
            {
                for (int j = 0; j < i; ++j)
                {
                    bool queryIsIncreasing = matches[j].QuerySequenceNumber < matches[i].QuerySequenceNumber;
                    bool canExtendMax = maxIncreasingQuerySequence[j].Length + 1 > maxIncreasingQuerySequence[i].Length;
                    bool sameSequence = IsSameSequence(matches[j], matches[i], maxGap);
                    if (queryIsIncreasing && canExtendMax && sameSequence)
                    {
                        maxIncreasingQuerySequence[i] = new MaxAt(maxIncreasingQuerySequence[j].Length + 1, matches[i]);
                        if (maxIncreasingQuerySequence[i].Length >= max)
                        {
                            // it is important to have >= since in case if the query increase and the track is not 
                            // we need to move the sequence forward
                            max = maxIncreasingQuerySequence[i].Length;
                            maxIndex = i;
                        }
                    }
                }
            }

            return maxIncreasingQuerySequence;
        }


        private static IEnumerable<MaxAt> GetCandidatesWithEqualMaxLength(MaxAt[] maxIncreasingQuerySequence, int index)
        {
            var current = maxIncreasingQuerySequence[index];
            var candidates = new List<MaxAt> {current};
            for (int lookAheadIndex = index - 1;
                lookAheadIndex >= 0 && current.Length == maxIncreasingQuerySequence[lookAheadIndex].Length;
                --lookAheadIndex)
            {
                // lis does not decrease meaning we need to pick best candidate by score
                // from the list of candidates with the same LIS length
                var lookAhead = maxIncreasingQuerySequence[lookAheadIndex];
                candidates.Add(lookAhead);
            }

            return candidates;
        }

        private static bool IsSameSequence(MatchedWith a, MatchedWith b, double maxGap)
        {
            return Math.Abs(a.QueryMatchAt - b.QueryMatchAt) <= maxGap &&
                   Math.Abs(a.TrackMatchAt - b.TrackMatchAt) <= maxGap;
        }
    }
}