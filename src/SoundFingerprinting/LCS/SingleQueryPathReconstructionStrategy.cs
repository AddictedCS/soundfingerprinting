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
            var matched = matches as MatchedWith[] ?? matches.ToArray();
            if (!matched.Any())
            {
                return Enumerable.Empty<IEnumerable<MatchedWith>>();
            }
            
            return new[] { GetLongestIncreasingSequence(matched, maxGap) };
        }

        protected override bool IsSameSequence(MatchedWith a, MatchedWith b, double maxGap)
        {
            return true;
        }
    }
}