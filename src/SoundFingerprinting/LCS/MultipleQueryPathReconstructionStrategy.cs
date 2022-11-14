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
            var bestPaths = new List<IEnumerable<MatchedWith>>();
            while (matchedWiths.Any())
            {
                var result  = GetLongestIncreasingSequence(matchedWiths, maxGap);
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
    }
}
