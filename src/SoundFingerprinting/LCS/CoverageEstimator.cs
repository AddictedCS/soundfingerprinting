namespace SoundFingerprinting.LCS
{
    using SoundFingerprinting.Query;

    using System.Collections.Generic;
    using System.Linq;

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
        /// <param name="matchedWiths">Matched withs for specific track</param>
        /// <param name="queryLength">Length of the query</param>
        /// <param name="fingerprintLengthInSeconds">Fingerprint length in seconds</param>
        /// <returns>Longest track region</returns>
        public static Coverage EstimateTrackCoverage(IEnumerable<MatchedWith> matchedWiths, double queryLength, double fingerprintLengthInSeconds)
        {
            var matches = matchedWiths.OrderBy(with => with.ResultAt).ToList();

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

            var trackRegion = new TrackRegion(minI, maxI);
            var notCovered = GetNotCoveredLength(matches, trackRegion, fingerprintLengthInSeconds, out var bestMatch);

            // optimistic coverage length
            double sourceCoverageLength = SubFingerprintsToSeconds.AdjustLengthToSeconds(matches[trackRegion.EndAt].ResultAt, matches[trackRegion.StartAt].ResultAt, fingerprintLengthInSeconds);
            double queryMatchLength = sourceCoverageLength - notCovered; // exact length of matched fingerprints

            double queryMatchStartsAt = matches[trackRegion.StartAt].QueryAt;
            double trackMatchStartsAt = matches[trackRegion.StartAt].ResultAt;

            return new Coverage(queryMatchStartsAt, queryMatchLength, sourceCoverageLength, trackMatchStartsAt, GetTrackStartsAt(bestMatch), queryLength);
   
        }
        
        private static double GetNotCoveredLength(IReadOnlyList<MatchedWith> orderedByResultAt, TrackRegion trackRegion, double fingerprintLengthInSeconds, out MatchedWith bestMatch)
        {
            double notCovered = 0d;
            bestMatch = orderedByResultAt[trackRegion.StartAt];
            for (int i = trackRegion.StartAt + 1; i <= trackRegion.EndAt; ++i)
            {
                if (orderedByResultAt[i].ResultAt - orderedByResultAt[i - 1].ResultAt > fingerprintLengthInSeconds)
                {
                    notCovered += orderedByResultAt[i].ResultAt - (orderedByResultAt[i - 1].ResultAt + fingerprintLengthInSeconds);
                }

                if (bestMatch.HammingSimilarity < orderedByResultAt[i].HammingSimilarity)
                {
                    bestMatch = orderedByResultAt[i];
                }
            }

            return notCovered;
        }

        private static bool ConsecutiveMatchesAreLongerThanTheQuery(double queryLength, List<MatchedWith> sortedMatches, int index, double fingerprintLengthInSeconds)
        {
            return SubFingerprintsToSeconds.AdjustLengthToSeconds(sortedMatches[index].ResultAt, sortedMatches[index - 1].ResultAt, fingerprintLengthInSeconds) > queryLength;
        }
        
        private static double GetTrackStartsAt(MatchedWith bestMatch)
        {
            return bestMatch.QueryAt - bestMatch.ResultAt;
        }
    }
}
