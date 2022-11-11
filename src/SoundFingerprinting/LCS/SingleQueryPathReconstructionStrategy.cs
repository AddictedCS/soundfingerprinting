namespace SoundFingerprinting.LCS
{
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.Query;

    internal class SingleQueryPathReconstructionStrategy : AbstractQueryPathReconstructionStrategy
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
            for (int j = maxIndex; j >= 0; --j)
            {
                var maxAt = maxArray[j];
                if (maxAt.Length == max)
                {
                    sequence.Add(maxAt.MatchedWith);
                    --max;
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