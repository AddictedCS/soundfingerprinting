namespace SoundFingerprinting.LCS;

using System;
using System.Collections.Generic;
using System.Linq;
using SoundFingerprinting.Configuration;
using SoundFingerprinting.Query;

internal class QueryPathReconstructionStrategy : IQueryPathReconstructionStrategy
{
    private readonly Comparer<MatchedWith> comparer = Comparer<MatchedWith>.Create((a, b) => a.QuerySequenceNumber.CompareTo(b.QuerySequenceNumber));
    
    /// <inheritdoc cref="IQueryPathReconstructionStrategy.GetBestPaths"/>
    /// <remarks>
    ///   Returns all possible reconstructed paths, where both <see cref="MatchedWith.TrackMatchAt"/> and <see cref="MatchedWith.QueryMatchAt"/> are strictly increasing. <br />
    ///   The paths are divided by the maximum allowed gap, meaning in case if a gap is detected bigger than maxGap, a new path is built from detection point onwards. <br />
    ///   This implementation can be used to built the reconstructed path when <see cref="QueryConfiguration.AllowMultipleMatchesOfTheSameTrackInQuery"/> is set to true.
    /// </remarks>
    public IEnumerable<IEnumerable<MatchedWith>> GetBestPaths(IEnumerable<MatchedWith> matches, double maxGap, int limit)
    {
        return GetIncreasingSequences(matches, maxGap, limit).ToList();
    }
    
    /// <summary>
    ///  Gets longest increasing sequences in the matches candidates
    /// </summary>
    /// <param name="matched">All matched candidates</param>
    /// <param name="maxGap">Max gap (i.e., for multiple query path reconstruction strategy <see cref="QueryConfiguration.PermittedGap" /> is used)</param>
    /// <returns>All available sequences</returns>
    private IEnumerable<IEnumerable<MatchedWith>> GetIncreasingSequences(IEnumerable<MatchedWith> matched, double maxGap, int limit)
    {
        var matchedWiths = matched.ToList();
        var bestPaths = new List<IEnumerable<MatchedWith>>();
        for (int i = 0; i < limit; ++i)
        {
            if (!matchedWiths.Any())
            {
                break;
            }

            var result = GetLongestIncreasingSequence(matchedWiths, maxGap);
            var withs = result as MatchedWith[] ?? result.ToArray();
            if (!withs.Any())
            {
                break;
            }

            bestPaths.Add(withs);
            matchedWiths = matchedWiths.Except(withs).ToList();
        }

        return bestPaths.OrderByDescending(_ => _.Count());
    }
    
    private static bool IsSameSequence(MatchedWith a, MatchedWith b, double maxGap)
    {
        return Math.Abs(a.QueryMatchAt - b.QueryMatchAt) <= maxGap && Math.Abs(a.TrackMatchAt - b.TrackMatchAt) <= maxGap;
    }
    
    private MaxAt[] MaxIncreasingQuerySequenceOptimal(IReadOnlyList<MatchedWith> matches, double maxGap, out int max, out int maxIndex)
    {
        var maxs = matches.Select(_ => new MaxAt(1, _)).ToArray();
        var dp = new MatchedWith[matches.Count];
        int len = 0;
        max = 1;
        maxIndex = 0;

        for (int j = 0; j < matches.Count; ++j)
        {
            var x = matches[j];
            int i = Array.BinarySearch(dp, 0, len, x, comparer);
            if (i < 0)
            {
                i = -(i + 1);
            }

            if (i > 0 && !IsSameSequence(dp[i - 1], x, maxGap))
            {
                continue;
            }
            
            if (i == 0 && dp[i] != null && !IsSameSequence(dp[0], x, maxGap))
            {
                continue;
            }
            
            dp[i] = x;

            if (i >= len - 1)
            {
                // the sequence has increased or found an equal element at the very end
                len = i == len ? len + 1 : len;
                maxs[j] = new MaxAt(len, x);
                maxIndex = j;
            }
            else
            {
                // the sequence does not increase
                maxs[j] = new MaxAt(i + 1, x);
            }
        }

        max = maxs[maxIndex].Length;
        return maxs;
    }

    private IEnumerable<MatchedWith> GetLongestIncreasingSequence(IEnumerable<MatchedWith> matched, double maxGap)
    {
        // locking first dimension - track sequence number
        var matches = matched.OrderBy(x => x.TrackSequenceNumber).ToList();
        if (!matches.Any())
        {
            return Enumerable.Empty<MatchedWith>();
        }

        var maxArray = MaxIncreasingQuerySequenceOptimal(matches, maxGap, out int max, out int maxIndex);
        var maxs = new Stack<MaxAt>(maxArray.Take(maxIndex + 1));
        var result = new Stack<MaxAt>();
        while (TryPop(maxs, out var candidate) && max > 0)
        {
            if (candidate!.Length != max)
            {
                continue;
            }

            max--;
                    
            while (true)
            {
                // check last entry in the result set
                if (!TryPeek(result, out var lastPicked))
                {
                    // first entry
                    result.Push(candidate!);
                }
                else if (IsQuerySequenceDecreasing(candidate!, lastPicked) && IsSameSequence(candidate!.MatchedWith, lastPicked!.MatchedWith, maxGap))
                {
                    // query sequence numbers are decreasing and is from the same sequence, good candidate
                    result.Push(candidate!);
                }
                        
                if (TryPeek(maxs, out var lookAhead) && EqualMaxLength(candidate!, lookAhead!) && TryPop(maxs, out candidate))
                {
                    // we are not ready yet, next candidate is of the same length, let's check it out and see if it is a good candidate
                    continue;
                }

                // we are done with current length
                break;
            }
        }

        return result.Select(_ => _.MatchedWith);
    }
    
    private static bool IsQuerySequenceDecreasing(MaxAt lookAhead, MaxAt? lastPicked)
    {
        return !(lookAhead.MatchedWith.QuerySequenceNumber > lastPicked?.MatchedWith.QuerySequenceNumber);
    }

    private static bool TryPop<T>(Stack<T> s, out T? result)
    {
        result = default;
        if (s.Any())
        {
            result = s.Pop();
            return true;
        }

        return false;
    }

    private static bool TryPeek<T>(Stack<T> s, out T? result)
    {
        result = default;
        if (s.Any())
        {
            result = s.Peek();
            return true;
        }

        return false;
    }

    private static bool EqualMaxLength(MaxAt current, MaxAt lookAhead)
    {
        return current.Length == lookAhead.Length;
    }
}