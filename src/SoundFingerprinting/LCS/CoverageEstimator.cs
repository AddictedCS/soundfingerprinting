namespace SoundFingerprinting.LCS
{
    using SoundFingerprinting.Query;

    using System.Collections.Generic;

    internal static class CoverageEstimator
    {
        /// <summary>
        ///  Estimates track coverage assuming there is only one match of the target Track in the query.
        ///  i.e. [----xxx----] but never [----xxxx-----xxxx] where x is 'complete' match.
        ///  In case if [---xx---xxx--] is found as incomplete match (assumed by default), allowed distance between multiple 
        ///  regions is the query length. This is calculated taking into account it's possible to have short queries in long tracks.
        ///  This never happens in case if a query is longer than any track in the storage.
        ///  In case if multiple tracks could be found in the same track, LongestIncreasingTrackSequence has to be used 
        /// </summary>
        /// <param name="matches">Ordered matches by ResultAt</param>
        /// <param name="queryLength">Length of the query</param>
        /// <param name="fingerprintLengthInSeconds">Fingerprint length in seconds</param>
        /// <returns>Longest track region</returns>
        public static TrackRegion EstimateTrackCoverage(List<MatchedWith> matches, double queryLength, double fingerprintLengthInSeconds)
        {
            int minI = 0, maxI = 0, curMinI = 0, maxLength = 0;
            for (int i = 1; i < matches.Count; ++i)
            {
                if (ConsecutiveMatchesAreLongerThanTheQuery(queryLength, matches, i, fingerprintLengthInSeconds))
                {
                    // potentialy a new start of best matched sequence
                    curMinI = i;
                }

                if (i - curMinI > maxLength)
                {
                    maxLength = i - curMinI;
                    maxI = i;
                    minI = curMinI;
                }
            }

            return new TrackRegion(minI, maxI);
        }

        private static bool ConsecutiveMatchesAreLongerThanTheQuery(double queryLength, List<MatchedWith> sortedMatches, int index, double fingerprintLengthInSeconds)
        {
            return SubFingerprintsToSeconds.AdjustLengthToSeconds(sortedMatches[index].ResultAt, sortedMatches[index - 1].ResultAt, fingerprintLengthInSeconds) > queryLength;
        }
    }
}
