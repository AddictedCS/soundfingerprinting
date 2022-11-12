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

    protected static bool TryPop<T>(Stack<T> s, out T? result)
    {
        result = default;
        if (s.Any())
        {
            result = s.Pop();
            return true;
        }

        return false;
    }

    protected static bool TryPeek<T>(Stack<T> s, out T? result)
    {
        result = default(T);
        if (s.Any())
        {
            result = s.Peek();
            return true;
        }

        return false;
    }

    protected static bool EqualMaxLength(MaxAt current, MaxAt lookAhead)
    {
        return current.Length == lookAhead.Length;
    }
}