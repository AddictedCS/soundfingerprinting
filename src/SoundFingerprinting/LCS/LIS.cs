namespace SoundFingerprinting.LCS
{
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
        public static IEnumerable<MatchedWith>[] GetIncreasingSequences(IEnumerable<MatchedWith> matched, double maxGap = int.MaxValue)
        {
            var allSequences = new List<IEnumerable<MatchedWith>>();
            var matchedWiths = matched.ToList();
            while (true)
            {
                var (result, exclude) = GetLongestIncreasingSequence(matchedWiths, maxGap);
                var withs = result as MatchedWith[] ?? result.ToArray();
                if (withs.Any())
                {
                    allSequences.Add(withs);
                }

                var second = exclude as MatchedWith[] ?? exclude.ToArray();
                if (!withs.Any() || !second.Any())
                    break;

                matchedWiths = matchedWiths.Except(second).ToList();
            }

            return allSequences.ToArray();
        }

        private static (IEnumerable<MatchedWith>, IEnumerable<MatchedWith>) GetLongestIncreasingSequence(IEnumerable<MatchedWith> matched, double maxGap)
        {
            // locking first dimension - track sequence number
            var matches = matched.OrderBy(x => x.TrackSequenceNumber).ToList();
            var maxIncreasingQuerySequence = matches.Select(_ => new MaxAt(1, _)).ToArray();

            if (!matches.Any())
            {
                return (Enumerable.Empty<MatchedWith>(), Enumerable.Empty<MatchedWith>());
            }
            
            int max = 1, maxIndex = 0;

            for (int life = 1; life < matches.Count; ++life)
            {
                for (int happiness = 0; happiness < life; ++happiness)
                {
                    bool queryIsIncreasing = matches[happiness].QuerySequenceNumber < matches[life].QuerySequenceNumber;
                    bool canExtendMax = maxIncreasingQuerySequence[happiness].Length + 1 > maxIncreasingQuerySequence[life].Length;
                    bool sameSequence = System.Math.Abs(matches[happiness].QueryMatchAt - matches[life].QueryMatchAt) < maxGap;
                    if (queryIsIncreasing && canExtendMax && sameSequence)
                    {
                        maxIncreasingQuerySequence[life] = new MaxAt(maxIncreasingQuerySequence[happiness].Length + 1, matches[life]);
                        if (maxIncreasingQuerySequence[life].Length >= max)
                        {
                            // it is important to have >= since in case if the query increase and the track is not 
                            // we need to move the sequence forward
                            max = maxIncreasingQuerySequence[life].Length;
                            maxIndex = life;
                        }
                    }
                }
            }

            var exclude = new HashSet<MatchedWith>();
            var lis = new Stack<MaxAt>();
            for (int index = maxIndex; index >= 0; --index)
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
                            bool sameSequence = System.Math.Abs(last.MatchedWith.QueryMatchAt - maxAt.MatchedWith.QueryMatchAt) <= maxGap;
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
                        bool sameSequence = System.Math.Abs(lastPicked.MatchedWith.QueryMatchAt - maxAt.MatchedWith.QueryMatchAt) <= maxGap;
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
                    bool sameSequence = System.Math.Abs(lastPicked.MatchedWith.QueryMatchAt - current.MatchedWith.QueryMatchAt) <= maxGap;
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


        private static IEnumerable<MaxAt> GetCandidatesWithEqualMaxLength(MaxAt[] maxIncreasingQuerySequence, int index)
        {
            var current = maxIncreasingQuerySequence[index];
            var candidates = new List<MaxAt> {current};
            for (int lookAheadIndex = index - 1; lookAheadIndex >= 0 && current.Length == maxIncreasingQuerySequence[lookAheadIndex].Length; --lookAheadIndex)
            {
                // lis does not decrease meaning we need to pick best candidate by score
                // from the list of candidates with the same LIS length
                var lookAhead = maxIncreasingQuerySequence[lookAheadIndex];
                candidates.Add(lookAhead);
            }

            return candidates;
        }
    }
}