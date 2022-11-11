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
            var maxArray = MaxIncreasingQuerySequenceOptimal(orderedByTrackMatchAt, maxGap, out int max, out int maxIndex);
            var sequence = new List<MatchedWith>();
            int index = maxIndex;
            while (max > 0 && index >= 0)
            {
                if (maxArray[index].Length == max)
                {
                    while (index >= 0 && maxArray[index].Length == max)
                    {
                        if (max == 1 && 
                            sequence.Any() &&
                            maxArray[index].MatchedWith.QuerySequenceNumber > sequence.Last().QuerySequenceNumber)
                        {
                            break;
                        }
                        
                        sequence.Add(maxArray[index--].MatchedWith);
                    }

                    --max;
                }
                else
                {
                    --index;
                }
            }

            sequence.Reverse();
            return new[] { sequence };
        }

        protected override bool IsSameSequence(MatchedWith a, MatchedWith b, double maxGap)
        {
            return true;
        }
    }
}