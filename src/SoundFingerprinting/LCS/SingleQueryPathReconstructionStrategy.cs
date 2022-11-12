namespace SoundFingerprinting.LCS
{
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.Query;

    internal class SingleQueryPathReconstructionStrategy : QueryPathReconstructionStrategy
    {
        /// <inheritdoc cref="IQueryPathReconstructionStrategy.GetBestPaths"/>
        /// <remarks>
        ///  Returns a single best reconstructed path where both <see cref="MatchedWith.TrackMatchAt"/> and <see cref="MatchedWith.QueryMatchAt"/> are strictly increasing. <br />
        ///  Maximum gap parameter is ignored, since any gap is treated like a gap.
        /// </remarks>
        public override IEnumerable<IEnumerable<MatchedWith>> GetBestPaths(IEnumerable<MatchedWith> matches, double maxGap)
        {
            var orderedByTrackMatchAt = matches.OrderBy(with => with.TrackSequenceNumber).ToList();

            if (!orderedByTrackMatchAt.Any())
            {
                return Enumerable.Empty<IEnumerable<MatchedWith>>();
            }
            
            var maxArray = MaxIncreasingQuerySequenceOptimal(orderedByTrackMatchAt, maxGap, out int max, out int maxIndex);
            var maxs = new Stack<MaxAt>(maxArray.Take(maxIndex + 1));
            var result = new Stack<MaxAt>();
            while (TryPop(maxs, out var candidate) && max > 0)
            {
                if (candidate!.Length != max)
                {
                    // not a good candidate
                    continue;
                }

                // found a potential entry to insert into the final list
                max--;
                while (true)
                {
                    // check last entry in the result set
                    bool contains = TryPeek(result, out var lastPicked);
                    if (!contains || lastPicked!.MatchedWith.QuerySequenceNumber >= candidate!.MatchedWith.QuerySequenceNumber)
                    {
                        // query sequence numbers are decreasing, good candidate
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
            
            return new[] { result.Select(_ => _.MatchedWith) };
        }

        protected override bool IsSameSequence(MatchedWith a, MatchedWith b, double maxGap)
        {
            return true;
        }
    }
}