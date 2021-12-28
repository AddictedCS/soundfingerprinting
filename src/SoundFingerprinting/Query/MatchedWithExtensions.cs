namespace SoundFingerprinting.Query
{
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.LCS;

    public static class MatchedWithExtensions
    {
        private static readonly ILongestIncreasingTrackSequence LongestIncreasingTrackSequence = new LongestIncreasingTrackSequence();
        
        /// <summary>
        ///  Estimates track coverage assuming there is only one match of the target Track in the query.
        /// </summary>
        /// <param name="matchedEntries">Matched withs for specific track.</param>
        /// <param name="queryLength">Length of the query.</param>
        /// <param name="trackLength">Length of the matched track.</param>
        /// <param name="fingerprintLength">Fingerprint length in seconds.</param>
        /// <param name="permittedGap">Permitted gap for discontinuity calculation.</param>
        /// <returns>An instance of the <see cref="Coverage"/> class.</returns>
        /// <remarks>
        ///  [----xxx----] but never [----xxxx-----xxxx] where x is 'complete' match.
        ///  In case if [---xx---xxx--] is found as incomplete match (assumed by default), allowed distance between multiple 
        ///  regions is the query length. This is calculated taking into account it's possible to have short queries in long tracks.
        ///  This never happens in case if a query is longer than any track in the storage.
        ///  In case if multiple tracks could be found in the same track, LongestIncreasingTrackSequence has to be used.
        /// </remarks>
        public static Coverage EstimateCoverage(this IEnumerable<MatchedWith> matchedEntries, double queryLength, double trackLength, double fingerprintLength, double permittedGap)
        {
            var bestPath = LisOld.GetBestPath(matchedEntries, queryLength, trackLength, fingerprintLength);
            return new Coverage(bestPath, queryLength, trackLength, fingerprintLength, permittedGap);
        }
        
        public static IEnumerable<Coverage> EstimateIncreasingCoverages(this IEnumerable<MatchedWith> matches, double queryLength, double trackLength, double fingerprintLength, double permittedGap)
        {
            var sequences = LongestIncreasingTrackSequence.FindAllIncreasingTrackSequences(matches, permittedGap);
            var coverages = sequences.Select(sequence => new Coverage(sequence, queryLength, trackLength, fingerprintLength, permittedGap));
            return OverlappingRegionFilter.FilterCrossMatchedCoverages(coverages);
        }
    }
}
