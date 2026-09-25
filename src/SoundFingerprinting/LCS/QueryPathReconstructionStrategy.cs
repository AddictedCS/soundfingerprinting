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
    ///   Reconstructs paths with monotone query and track positions, retaining the existing treatment of ties.
    ///   Ties are allowed on the sorted axis, chosen by the wider matched time span (track on equal spans).
    ///   Noise can change that choice for similar spans, changing the selected path and its length.
    /// </remarks>
    public IEnumerable<IEnumerable<MatchedWith>> GetBestPaths(IEnumerable<MatchedWith> matches, double permittedGap)
    {
        var points = matches.ToArray();
        if (points.Length == 0)
        {
            return [];
        }

        double querySpan = points.Max(p => p.QueryMatchAt) - points.Min(p => p.QueryMatchAt);
        double trackSpan = points.Max(p => p.TrackMatchAt) - points.Min(p => p.TrackMatchAt);
        // repeated occurrences must advance on the sorted axis, whichever recording contains them.
        if (querySpan <= trackSpan)
        {
            return GetIncreasingSequences(points, permittedGap).ToList();
        }

        return GetIncreasingSequences(points.Select(Transpose), permittedGap)
            .Select(path => path.Select(Transpose).ToArray()).ToList();
    }

    private static MatchedWith Transpose(MatchedWith match)
    {
        return new MatchedWith(match.TrackSequenceNumber, match.TrackMatchAt, match.QuerySequenceNumber, match.QueryMatchAt, match.Score, match.Type);
    }
    
    private IEnumerable<IEnumerable<MatchedWith>> GetIncreasingSequences(IEnumerable<MatchedWith> matched, double permittedGap, bool recoverCrossedPrefixes = true)
    {
        var remaining = new HashSet<MatchedWith>(matched);
        var bestPaths = new List<IEnumerable<MatchedWith>>();
        HashSet<MatchedWith>? crossedPrefixes = null;
        
        while (remaining.Count > 0)
        {
            var (sequence, exclusions) = GetLongestIncreasingSequence(remaining, permittedGap);
            var withs = sequence as MatchedWith[] ?? sequence.ToArray();
            if (withs.Length == 0)
            {
                break;
            }

            bestPaths.Add(withs);
            remaining.ExceptWith(withs);
            // retain earlier prefixes; crossed prefixes are reconsidered on the other axis.
            // singleton paths retain the existing exclusion policy because they have no time span.
            var first = withs[0];
            var last = withs[withs.Length - 1];
            if (recoverCrossedPrefixes && withs.Length > 1)
            {
                foreach (var candidate in exclusions)
                {
                    if (candidate.TrackMatchAt < first.TrackMatchAt && candidate.QueryMatchAt > last.QueryMatchAt && remaining.Contains(candidate))
                    {
                        (crossedPrefixes ??= []).Add(candidate);
                    }
                }
            }

            remaining.ExceptWith(withs.Length == 1 ? exclusions : exclusions.Where(x =>
                x.TrackMatchAt >= first.TrackMatchAt || x.QueryMatchAt > last.QueryMatchAt));
        }

        // one opposite-axis pass recovers disjoint suffixes without recursively reviving its own competitors.
        if (crossedPrefixes != null)
        {
            bestPaths.AddRange(GetIncreasingSequences(crossedPrefixes.Select(Transpose), permittedGap, false)
                .Select(path => path.Select(Transpose).ToArray()));
        }

        // this may seem as redundant, but it is not, since we can pick the first candidates from not the same sequences
        return bestPaths.OrderByDescending(p => p.Count());
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
        var result = new Dictionary<int, MaxAt> {[max--] = maxArray[maxIndex]};
        var lastPicked = maxArray[maxIndex];
        // compare equal-length alternatives with the selected occurrence, not the recordings' origins.
        double alignment = (double)lastPicked.MatchedWith.TrackMatchAt - lastPicked.MatchedWith.QueryMatchAt;
        
        while (maxs.TryPop(out var candidate))
        {
            if (!IsSameSequence(candidate!, lastPicked, maxGap))
            {
                continue;
            }
            
            if (candidate!.Length > max)
            {
                // check if we previously picked a sequence with the same length, if yes we should try picking the best one based on the distance to the selected alignment
                lastPicked = TryUpdateResultSelection(result, candidate, excluded, alignment);
                continue;
            }
            
            if (candidate.Length < max)
            {
                // start of a shorter sequence, we should exclude it
                excluded.Add(candidate);
                continue;
            }

            if (!IsQuerySequenceDecreasing(candidate, lastPicked))
            {
                // the candidate is part of a different decreasing sequence
                excluded.Add(candidate);
                continue;
            }
            
            max--;
            lastPicked = TryUpdateResultSelection(result, candidate, excluded, alignment);
        }

        return new LongestIncreasingSequence(result.OrderBy(_ => _.Key).Select(_ => _.Value.MatchedWith), excluded.Select(_ => _.MatchedWith));
    }

    private static MaxAt TryUpdateResultSelection(Dictionary<int, MaxAt> result, MaxAt candidate, List<MaxAt> excluded, double alignment)
    {
        if (!result.TryGetValue(candidate.Length, out var previous))
        {
            result[candidate.Length] = candidate;
            return candidate;
        }

        double previousAlignmentDistance = Math.Abs((double)previous.MatchedWith.TrackMatchAt - previous.MatchedWith.QueryMatchAt - alignment);
        double candidateAlignmentDistance = Math.Abs((double)candidate.MatchedWith.TrackMatchAt - candidate.MatchedWith.QueryMatchAt - alignment);

        // possible when the candidate is part of a different decreasing sequence with equal maxAt
        if (!IsQuerySequenceDecreasing(candidate, previous))
        {
            excluded.Add(candidate);
            return previous;
        }

        if (WouldBreakLowerNeighbour(result, candidate))
        {
            excluded.Add(candidate);
            return previous;
        }

        // prefer the alternative closer to this occurrence's alignment.
        var pickedValue = previousAlignmentDistance < candidateAlignmentDistance ? previous : candidate;
        var excludedValue = previousAlignmentDistance < candidateAlignmentDistance ? candidate : previous;
        excluded.Add(excludedValue);
        result[candidate.Length] = pickedValue;
        return pickedValue;
    }

    /// <summary>
    ///  Determines whether replacing <c>result[Length]</c> with <paramref name="candidate"/> would push the
    ///  chain's track axis below its immediate lower-length neighbour, breaking monotonicity going down the chain.
    /// </summary>
    /// <remarks>
    ///  Why <c>Length - 1</c> is the right key to inspect — and the only one:
    ///  <list type="bullet">
    ///   <item>
    ///    <description>
    ///     <b>Contiguity.</b> The keys in <c>result</c> are always the contiguous range
    ///     <c>[max + 1, initialMax]</c>. Init places one entry at <c>initialMax</c>; the bottom-block admission is
    ///     the only place that lowers <c>max</c>, and it does so in single steps while filling the freshly-uncovered
    ///     slot. So <c>result[Length - 1]</c> — if it exists — is the genuine immediate lower neighbour; a gap at
    ///     <c>Length - 1</c> with <c>Length - 2</c> filled is impossible.
    ///    </description>
    ///   </item>
    ///   <item>
    ///    <description>
    ///     <b>Lower neighbour missing is benign.</b> If <c>result[Length - 1]</c> is absent, then
    ///     <c>Length == max + 1</c> — the candidate sits at the bottom of the currently-filled range. The slot
    ///     below will be filled later by a pop at a lower input index, hence (by the track-ascending sort) at a
    ///     strictly lower track value than this candidate. The chain stays monotone after that future fill.
    ///    </description>
    ///   </item>
    ///   <item>
    ///    <description>
    ///     <b>Upper neighbour cannot break.</b> Same-length competitors are popped in reverse input order, so
    ///     <c>candidate.TrackSequenceNumber ≤ previous.TrackSequenceNumber</c>, and at swap time
    ///     <c>previous</c> was itself <c>≤ result[Length + 1]</c>. So the new track value at <c>Length</c> is still
    ///     below <c>result[Length + 1]</c> after the swap.
    ///    </description>
    ///   </item>
    ///  </list>
    ///  Track ties across the chain are accepted (<c>ShouldPickAllTrackCandidates</c> documents this) — only a
    ///  strict decrease is rejected here.
    /// </remarks>
    /// <param name="result">Selected positions by path length.</param>
    /// <param name="candidate">Alternative position.</param>
    /// <returns>True if replacement would reverse the track axis.</returns>
    private static bool WouldBreakLowerNeighbour(Dictionary<int, MaxAt> result, MaxAt candidate)
    {
        return result.TryGetValue(candidate.Length - 1, out var below) && below.MatchedWith.TrackSequenceNumber > candidate.MatchedWith.TrackSequenceNumber;
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