namespace SoundFingerprinting.LCS;

using System;
using System.Collections.Generic;
using System.Linq;
using SoundFingerprinting.Query;

internal class QueryPathReconstructionStrategy : IQueryPathReconstructionStrategy
{
    private readonly Comparer<MatchedWith> comparer = Comparer<MatchedWith>.Create((a, b) => a.QuerySequenceNumber.CompareTo(b.QuerySequenceNumber));
    
    /// <inheritdoc cref="IQueryPathReconstructionStrategy.GetBestPaths"/>
    /// <remarks>
    ///   Returns all possible reconstructed paths, where both <see cref="MatchedWith.TrackMatchAt"/> and <see cref="MatchedWith.QueryMatchAt"/> are strictly increasing. <br />
    /// </remarks>
    public IEnumerable<IEnumerable<MatchedWith>> GetBestPaths(IEnumerable<MatchedWith> matches)
    {
        return GetIncreasingSequences(matches).ToList();
    }
    
    private IEnumerable<IEnumerable<MatchedWith>> GetIncreasingSequences(IEnumerable<MatchedWith> matched)
    {
        var matchedWiths = matched.ToList();
        var bestPaths = new List<IEnumerable<MatchedWith>>();
        while (matchedWiths.Any())
        {
            var (sequence, badSequence) = GetLongestIncreasingSequence(matchedWiths);
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

    private LongestIncreasingSequence GetLongestIncreasingSequence(IEnumerable<MatchedWith> matched)
    {
        // locking first dimension - track sequence number
        var matches = matched.OrderBy(x => x.TrackSequenceNumber).ToList();
        if (!matches.Any())
        {
            return new LongestIncreasingSequence(Enumerable.Empty<MatchedWith>(), Enumerable.Empty<MatchedWith>());
        }

        double maxGap = GetMaxGap(matches);
        var maxArray = MaxIncreasingQuerySequenceOptimal(matches, maxGap, out int max, out int maxIndex);
        
        var maxs = new Stack<MaxAt>(maxArray.Take(maxIndex + 1));
        var result = new Stack<MaxAt>();
        var excluded = new List<MaxAt>();
        while (maxs.TryPop(out var candidate) && max > 0)
        {
            if (candidate!.Length != max)
            {
                // out of order element need to be excluded if it is part of the same sequence
                if (result.TryPeek(out var lastPicked) && IsSameSequence(candidate.MatchedWith, lastPicked!.MatchedWith, maxGap))
                {
                    excluded.Add(candidate);
                }
                
                continue;
            }

            max--;
            do
            {
                bool firstElementInSequence = !result.TryPeek(out var lastPicked);
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
            }
            while (maxs.TryPeek(out var lookAhead) && EqualMaxLength(candidate!, lookAhead!) && maxs.TryPop(out candidate));
        }

        return new LongestIncreasingSequence(result.Select(_ => _.MatchedWith), excluded.Select(_ => _.MatchedWith));
    }

    private static double GetMaxGap(List<MatchedWith> matches)
    {
        float queryMatchAtMax = float.MinValue, queryMatchAtMin = float.MaxValue, trackMatchAtMax = float.MinValue, trackMatchAtMin = float.MaxValue;
        foreach (var entry in matches)
        {
            queryMatchAtMax = Math.Max(queryMatchAtMax, entry.QueryMatchAt);
            queryMatchAtMin = Math.Min(queryMatchAtMin, entry.QueryMatchAt);
            trackMatchAtMax = Math.Max(trackMatchAtMax, entry.TrackMatchAt);
            trackMatchAtMin = Math.Min(trackMatchAtMin, entry.TrackMatchAt);
        }

        return Math.Min(queryMatchAtMax - queryMatchAtMin, trackMatchAtMax - trackMatchAtMin);
    }

    private static bool IsQuerySequenceDecreasing(MaxAt lookAhead, MaxAt? lastPicked)
    {
        return !(lookAhead.MatchedWith.QuerySequenceNumber > lastPicked?.MatchedWith.QuerySequenceNumber);
    }

    private static bool EqualMaxLength(MaxAt current, MaxAt lookAhead)
    {
        return current.Length == lookAhead.Length;
    }

    private record LongestIncreasingSequence(IEnumerable<MatchedWith> Sequence, IEnumerable<MatchedWith> BadCandidates)
    {
        public IEnumerable<MatchedWith> Sequence { get; } = Sequence;

        public IEnumerable<MatchedWith> BadCandidates { get; } = BadCandidates;

        public void Deconstruct(out IEnumerable<MatchedWith> sequence, out IEnumerable<MatchedWith> badCandidates)
        {
            sequence = Sequence;
            badCandidates = BadCandidates;
        }
    }
}