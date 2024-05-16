namespace SoundFingerprinting.LCS;

using System;
using System.Collections.Concurrent;
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
    public IEnumerable<IEnumerable<MatchedWith>> GetBestPaths(IEnumerable<MatchedWith> matches, double permittedGap)
    {
        return GetIncreasingSequences(matches, permittedGap).ToList();
    }
    
    private IEnumerable<IEnumerable<MatchedWith>> GetIncreasingSequences(IEnumerable<MatchedWith> matched, double permittedGap)
    {
        var matchedWiths = matched.ToList();
        var bestPaths = new List<IEnumerable<MatchedWith>>();
        while (matchedWiths.Any())
        {
            var (sequence, exclusions) = GetLongestIncreasingSequence(matchedWiths, permittedGap);
            var withs = sequence as MatchedWith[] ?? sequence.ToArray();
            if (!withs.Any())
            {
                break;
            }

            bestPaths.Add(withs);
            matchedWiths = matchedWiths.Except(withs.Concat(exclusions)).ToList();
        }

        // this may seem as redundant, but it is not, since we can pick the first candidates from not the same sequences
        return bestPaths.OrderByDescending(_ => _.Count());
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

    private LongestIncreasingSequence GetLongestIncreasingSequence(IEnumerable<MatchedWith> matched, double permittedGap)
    {
        // locking first dimension - track sequence number
        var matches = matched.OrderBy(x => x.TrackSequenceNumber).ThenBy(_ => _.TrackMatchAt).ToList();
        if (!matches.Any())
        {
            return new LongestIncreasingSequence([], []);
        }

        double maxGap = GetMaxGap(matches, permittedGap);
        
        // locking second dimension - query sequence number
        var maxArray = MaxIncreasingQuerySequenceOptimal(matches, maxGap, out int max, out int maxIndex);
        
        // initializing the datastructures with first element set to max
        var maxs = new Stack<MaxAt>(maxArray.Take(maxIndex));
        var excluded = new List<MaxAt>();
        var result = new ConcurrentDictionary<int, MaxAt> {[max--] = maxArray[maxIndex]};
        var lastPicked = maxArray[maxIndex];
        
        while (maxs.TryPop(out var candidate))
        {
            if (candidate!.Length != max)
            {
                // check if the candidate is part of the same decreasing sequence
                if (IsSameSequence(candidate, lastPicked, maxGap))
                {
                    // check if we previously picked a sequence with the same length, if yes we should try picking the best one
                    if (candidate.Length > max)
                    {
                        TryUpdateResultSelection(result, candidate, excluded);
                    }
                    else
                    {
                        // start of a shorter sequence, we should exclude it
                        excluded.Add(candidate);
                    }
                }

                continue;
            }

            max--;
            
            do
            {
                switch (IsQuerySequenceDecreasing(candidate, lastPicked))
                {
                    case true when IsSameSequence(candidate, lastPicked, maxGap):
                        lastPicked = TryUpdateResultSelection(result, candidate, excluded);
                        break;
                    case false when IsSameSequence(candidate, lastPicked, maxGap):
                        excluded.Add(candidate);
                        break;
                }
            }
            while (maxs.TryPeek(out var lookAhead) && EqualMaxLength(candidate!, lookAhead!) && maxs.TryPop(out candidate!));
        }

        return new LongestIncreasingSequence(result.OrderBy(_ => _.Key).Select(_ => _.Value.MatchedWith), excluded.Select(_ => _.MatchedWith));
    }

    private static MaxAt TryUpdateResultSelection(ConcurrentDictionary<int, MaxAt> result, MaxAt candidate, List<MaxAt> excluded)
    {
        // check if the candidate is closer to the diagonal than the previous element, pick best and exclude the other
        return result.AddOrUpdate(candidate.Length, candidate, (_, previous) =>
        {
            double prevQueryTrackDistance = previous.QueryTrackDistance;
            double currentQueryTrackDistance = candidate.QueryTrackDistance;

            // possible when the candidate is part of a different decreasing sequence with equal maxAt
            if (!IsQuerySequenceDecreasing(candidate, previous))
            {
                excluded.Add(candidate);
                return previous;
            }

            // if the current element is closer to the diagonal, we should pick it
            var pickedValue = prevQueryTrackDistance < currentQueryTrackDistance ? previous : candidate!;
            var excludedValue = prevQueryTrackDistance < currentQueryTrackDistance ? candidate : previous;
            excluded.Add(excludedValue);
            return pickedValue;
        });
    }
    
    private static bool IsSameSequence(MaxAt first, MaxAt second, double maxGap)
    {
        return IsSameSequence(first.MatchedWith, second.MatchedWith, maxGap);
    }
    
    private static bool IsSameSequence(MatchedWith first, MatchedWith second, double maxGap)
    {
        return Math.Abs(first.QueryMatchAt - second.QueryMatchAt) <= maxGap && Math.Abs(first.TrackMatchAt - second.TrackMatchAt) <= maxGap;
    }

    private static double GetMaxGap(List<MatchedWith> matches, double permittedGap)
    {
        float queryMatchAtMax = float.MinValue, queryMatchAtMin = float.MaxValue, trackMatchAtMax = float.MinValue, trackMatchAtMin = float.MaxValue;
        foreach (var entry in matches)
        {
            queryMatchAtMax = Math.Max(queryMatchAtMax, entry.QueryMatchAt);
            queryMatchAtMin = Math.Min(queryMatchAtMin, entry.QueryMatchAt);
            trackMatchAtMax = Math.Max(trackMatchAtMax, entry.TrackMatchAt);
            trackMatchAtMin = Math.Min(trackMatchAtMin, entry.TrackMatchAt);
        }

        return Math.Max(permittedGap, Math.Min(queryMatchAtMax - queryMatchAtMin, trackMatchAtMax - trackMatchAtMin));
    }

    private static bool IsQuerySequenceDecreasing(MaxAt lookAhead, MaxAt lastPicked)
    {
        return !(lookAhead.MatchedWith.QuerySequenceNumber > lastPicked.MatchedWith.QuerySequenceNumber);
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