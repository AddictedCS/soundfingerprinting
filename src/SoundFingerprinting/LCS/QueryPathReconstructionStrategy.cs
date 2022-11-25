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

            var (sequence, badSequence) = GetLongestIncreasingSequence(matchedWiths, maxGap);
            var withs = sequence as MatchedWith[] ?? sequence.ToArray();
            if (!withs.Any())
            {
                break;
            }

            bestPaths.Add(withs);
            matchedWiths = matchedWiths.Except(withs.Concat(badSequence)).ToList();
        }

        // this may seem as redundant but it is not, since we can pick the first candidates from not the same sequences
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

    private LongestIncreasingSequence GetLongestIncreasingSequence(IEnumerable<MatchedWith> matched, double maxGap)
    {
        // locking first dimension - track sequence number
        var matches = matched.OrderBy(x => x.TrackSequenceNumber).ToList();
        if (!matches.Any())
        {
            return new LongestIncreasingSequence(Enumerable.Empty<MatchedWith>(), Enumerable.Empty<MatchedWith>());
        }

        var maxArray = MaxIncreasingQuerySequenceOptimal(matches, maxGap, out int max, out int maxIndex);
        var maxs = new Stack<MaxAt>(maxArray.Take(maxIndex + 1));
        var result = new Stack<MaxAt>();
        var excluded = new List<MaxAt>();
        while (TryPop(maxs, out var candidate) && max > 0)
        {
            if (candidate!.Length != max)
            {
                // out of order element need to be excluded if it is part of the same sequence
                if (TryPeek(result, out var lastPicked) && IsSameSequence(candidate.MatchedWith, lastPicked!.MatchedWith, maxGap))
                {
                    excluded.Add(candidate);
                }
                
                continue;
            }

            max--;
            while (true)
            {
                bool firstElementInSequence = !TryPeek(result, out var lastPicked);
                bool querySequenceDecreasing = IsQuerySequenceDecreasing(candidate!, lastPicked);
                bool sameSequence = firstElementInSequence || IsSameSequence(candidate!.MatchedWith, lastPicked!.MatchedWith, maxGap);

                switch (querySequenceDecreasing)
                {
                    case true when sameSequence:
                        result.Push(candidate!);
                        break;
                    case false when sameSequence:
                        excluded.Add(candidate!);
                        break;
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

        return new LongestIncreasingSequence(result.Select(_ => _.MatchedWith), excluded.Select(_ => _.MatchedWith));
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

    private class LongestIncreasingSequence
    {
        public LongestIncreasingSequence(IEnumerable<MatchedWith> sequence, IEnumerable<MatchedWith> badCandidates)
        {
            Sequence = sequence;
            BadCandidates = badCandidates;
        }

        public IEnumerable<MatchedWith> Sequence { get; }

        public IEnumerable<MatchedWith> BadCandidates { get; }

        public void Deconstruct(out IEnumerable<MatchedWith> sequence, out IEnumerable<MatchedWith> badCandidates)
        {
            sequence = Sequence;
            badCandidates = BadCandidates;
        }
    }
}