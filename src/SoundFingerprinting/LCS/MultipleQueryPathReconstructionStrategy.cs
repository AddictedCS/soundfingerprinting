namespace SoundFingerprinting.LCS
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Query;

    internal class MultipleQueryPathReconstructionStrategy : QueryPathReconstructionStrategy
    {
        /// <inheritdoc cref="IQueryPathReconstructionStrategy.GetBestPaths"/>
        /// <remarks>
        ///   Returns all possible reconstructed paths, where both <see cref="MatchedWith.TrackMatchAt"/> and <see cref="MatchedWith.QueryMatchAt"/> are strictly increasing. <br />
        ///   The paths are divided by the maximum allowed gap, meaning in case if a gap is detected bigger than maxGap, a new path is built from detection point onwards. <br />
        ///   This implementation can be used to built the reconstructed path when <see cref="QueryConfiguration.AllowMultipleMatchesOfTheSameTrackInQuery"/> is set to true.
        /// </remarks>
        public override IEnumerable<IEnumerable<MatchedWith>> GetBestPaths(IEnumerable<MatchedWith> matches, double maxGap)
        {
            return GetIncreasingSequences(matches, maxGap).OrderByDescending(list => list.Count()).ToList();
        }
        
        protected override bool IsSameSequence(MatchedWith a, MatchedWith b, double maxGap)
        {
            return Math.Abs(a.QueryMatchAt - b.QueryMatchAt) <= maxGap && Math.Abs(a.TrackMatchAt - b.TrackMatchAt) <= maxGap;
        }

        /// <summary>
        ///  Gets longest increasing sequences in the matches candidates
        /// </summary>
        /// <param name="matched">All matched candidates</param>
        /// <param name="maxGap">Max gap (i.e. Math.Min(trackLength, queryLength)</param>
        /// <returns>All available sequences</returns>
        private IEnumerable<IEnumerable<MatchedWith>> GetIncreasingSequences(IEnumerable<MatchedWith> matched, double maxGap = int.MaxValue)
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
                {
                    break;
                }

                matchedWiths = matchedWiths.Except(second).ToList();
            }
#if DEBUG
            AssertResults(results, matchedWiths);
#endif
            return results.OrderByDescending(_ => _.Count());
        }

        private (IEnumerable<MatchedWith>, IEnumerable<MatchedWith>) GetLongestIncreasingSequence(IEnumerable<MatchedWith> matched, double maxGap)
        {
            // locking first dimension - track sequence number
            var matches = matched.OrderBy(x => x.TrackSequenceNumber).ToList();

            if (!matches.Any())
            {
                return (Enumerable.Empty<MatchedWith>(), Enumerable.Empty<MatchedWith>());
            }

            var maxArray = MaxIncreasingQuerySequenceOptimal(matches, maxGap, out int max, out int maxIndex);
            var maxs = new Stack<MaxAt>(maxArray.Take(maxIndex + 1));
            var exclude = new List<MatchedWith>();
            var lis = new Stack<MaxAt>();
            while (TryPop(maxs, out var current) && max > 0)
            {
                if (current.Length == max) // found a potential entry to insert into the final list
                {
                    if (TryPeek(lis, out var lastPicked) && !IsSameSequence(current, lastPicked, maxGap))
                    {
                        // entry is not from the same list of increasing candidates
                        continue;
                    }

                    max--;
                    exclude.Add(current.MatchedWith);

                    // get all candidates in the current region that did not increase max length
                    // pick best by score
                    while (TryPeek(maxs, out var lookAhead) && EqualMaxLength(current, lookAhead))
                    {
                        // we disallow query sequence reverts, so let's find best candidate by score
                        // from the same increasing sequence
                        if (IsQuerySequenceIncreasing(lookAhead, lastPicked))
                        {
                            exclude.Add(maxs.Pop().MatchedWith);
                            continue;
                        }
                        
                        // select best candidate by score and from the same sequence
                        if (IsSameSequence(current, maxs.Pop(), maxGap))
                        {
                            exclude.Add(lookAhead.MatchedWith);
                            current = GetBestByScore(current, lookAhead);
                        }
                    }
                    
                    lis.Push(current);
                }
                else
                {
                    // out of order element need to be excluded during next iteration
                    if (TryPeek(lis, out var lastPicked) && IsSameSequence(lastPicked, current, maxGap))
                    {
                        exclude.Add(current.MatchedWith);
                    }
                }
            }

            return (CaptureResult(lis), exclude);
        }

        private static bool IsQuerySequenceIncreasing(MaxAt lookAhead, MaxAt lastPicked)
        {
            return lookAhead.MatchedWith.QuerySequenceNumber >= lastPicked?.MatchedWith.QuerySequenceNumber;
        }

        private static IEnumerable<MatchedWith> CaptureResult(Stack<MaxAt> lis)
        {
            var result = new List<MatchedWith>();
            while (TryPop(lis, out var current))
            {
                while (TryPeek(lis, out var lookAhead) && EqualTrackSequence(current, lookAhead))
                {
                    // lis peaked same track sequence entries, let's find best candidate of them all
                    current = GetBestByScore(current, lis.Pop());
                }

                result.Add(current.MatchedWith);
            }

            return result;
        }

        private static bool EqualTrackSequence(MaxAt current, MaxAt lookAhead)
        {
            return current.MatchedWith.TrackSequenceNumber == lookAhead.MatchedWith.TrackSequenceNumber;
        }

        private static MaxAt GetBestByScore(MaxAt current, MaxAt lookAhead)
        {
            return current.MatchedWith.Score < lookAhead.MatchedWith.Score ? lookAhead : current;
        }

        private static bool EqualMaxLength(MaxAt current, MaxAt lookAhead)
        {
            return current.Length == lookAhead.Length;
        }
        
        private bool IsSameSequence(MaxAt a, MaxAt b, double maxGap)
        {
            return IsSameSequence(a.MatchedWith, b.MatchedWith, maxGap);
        }
       
        private static bool TryPop<T>(Stack<T> s, out T? result)
        {
            result = default(T);
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
        
        private static void AssertResults(IEnumerable<IEnumerable<MatchedWith>> results, IEnumerable<MatchedWith> initial)
        {
            var initialResults = initial.OrderBy(_ => _.TrackSequenceNumber);
            foreach (IEnumerable<MatchedWith> result in results)
            {
                var trackIds = result.Select(_ => _.TrackSequenceNumber).ToArray();
                var queryIds = result.Select(_ => _.QuerySequenceNumber).ToArray();

                for (int i = 1; i < trackIds.Length; ++i)
                {
                    if (trackIds[i] <= trackIds[i - 1])
                    {
                        throw new Exception($"Track Ids are not sorted [{trackIds[i]},{trackIds[i - 1]}] from [{string.Join(",", trackIds)}], " +
                                            $"initial results {string.Join(",", initialResults.Select(_ => $"({_.QuerySequenceNumber},{_.TrackSequenceNumber},{_.Score})"))}");
                    }

                    if (queryIds[i] <= queryIds[i - 1])
                    {
                        throw new Exception($"Query Ids are not sorted [{queryIds[i]},{queryIds[i - 1]}] from [{string.Join(",", queryIds)}], " +
                                            $"initial results {string.Join(",", initialResults.Select(_ => $"({_.QuerySequenceNumber},{_.TrackSequenceNumber},{_.Score})"))}");
                    }
                }
            }
        }
    }
}
