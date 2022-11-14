namespace SoundFingerprinting.LCS;

using System;
using System.Collections.Generic;
using System.Linq;
using SoundFingerprinting.Query;

internal abstract class QueryPathReconstructionStrategy : IQueryPathReconstructionStrategy
{
    private readonly Comparer<MatchedWith> comparer = Comparer<MatchedWith>.Create((a, b) => a.QuerySequenceNumber.CompareTo(b.QuerySequenceNumber));
    
    public abstract IEnumerable<IEnumerable<MatchedWith>> GetBestPaths(IEnumerable<MatchedWith> matches, double maxGap);
    
    protected abstract bool IsSameSequence(MatchedWith a, MatchedWith b, double maxGap);
    
    protected MaxAt[] MaxIncreasingQuerySequenceOptimal(IReadOnlyList<MatchedWith> matches, double maxGap, out int max, out int maxIndex)
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
    
    protected IEnumerable<MatchedWith> GetLongestIncreasingSequence(IEnumerable<MatchedWith> matched, double maxGap)
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
                bool contains = TryPeek(result, out var lastPicked);
                if (!contains || (!IsQuerySequenceIncreasing(candidate!, lastPicked) && IsSameSequence(candidate!.MatchedWith, lastPicked!.MatchedWith, maxGap)))
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
    
    private static bool IsQuerySequenceIncreasing(MaxAt lookAhead, MaxAt? lastPicked)
    {
        return lookAhead.MatchedWith.QuerySequenceNumber > lastPicked?.MatchedWith.QuerySequenceNumber;
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
        result = default(T);
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