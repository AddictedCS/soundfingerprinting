namespace SoundFingerprinting.Query
{
    using SoundFingerprinting.LCS;
    using System.Collections.Generic;
    using System.Linq;

    public static class MatchedWithExtensions
    {
        /// <summary>
        ///  Estimates track coverage assuming there is only one match of the target Track in the query.
        ///  i.e. [----xxx----] but never [----xxxx-----xxxx] where x is 'complete' match.
        ///  In case if [---xx---xxx--] is found as incomplete match (assumed by default), allowed distance between multiple 
        ///  regions is the query length. This is calculated taking into account it's possible to have short queries in long tracks.
        ///  This never happens in case if a query is longer than any track in the storage.
        ///  In case if multiple tracks could be found in the same track, LongestIncreasingTrackSequence has to be used 
        /// </summary>
        /// <param name="matchedEntries">Matched withs for specific track</param>
        /// <param name="queryLength">Length of the query</param>
        /// <param name="fingerprintLength">Fingerprint length in seconds</param>
        /// <param name="permittedGap">Permitted gap for discontinuity calculation</param>
        /// <returns>Longest track region</returns>
        public static Coverage EstimateCoverage(this IEnumerable<MatchedWith> matchedEntries, double queryLength, double fingerprintLength, double permittedGap)
        {
            var matches = matchedEntries.OrderBy(with => with.TrackMatchAt).ToList();
            var trackRegion = GetTrackRegion(matches, queryLength, fingerprintLength);
            return new Coverage(GetBestReconstructedPath(trackRegion, matches), queryLength, fingerprintLength, permittedGap);
        }

        private static TrackRegion GetTrackRegion(IReadOnlyList<MatchedWith> orderedByTrackMatchAt, double queryLength, double fingerprintLengthInSeconds)
        {
            int minI = 0, maxI = 0, curMinI = 0, maxLength = 0;
            for (int i = 1; i < orderedByTrackMatchAt.Count; ++i)
            {
                // since we don't allow same multiple matches in the query, we check to see if this is a start of new best sequence 
                if (ConsecutiveMatchesAreLongerThanTheQuery(queryLength, orderedByTrackMatchAt, i, fingerprintLengthInSeconds))
                {
                    // potentially a new start of best matched sequence
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

        private static IEnumerable<MatchedWith> GetBestReconstructedPath(TrackRegion trackRegion, IEnumerable<MatchedWith> matches)
        {
            return matches.Skip(trackRegion.StartAt)
                   .Take(trackRegion.Count)
                   .OrderByDescending(m => m.Score)
                   .GroupBy(m => m.TrackSequenceNumber)
                   .Aggregate(new { List = new List<MatchedWith>(), Used = new HashSet<uint>()}, (acc, group) =>
                       {
                           var bestByScore = group.OrderByDescending(m => m.Score).ToList();
                           foreach (var match in bestByScore)
                           {
                               if (!acc.Used.Contains(match.QuerySequenceNumber))
                               {
                                   acc.List.Add(match);
                                   acc.Used.Add(match.QuerySequenceNumber);
                                   return acc;
                               }
                           }

                           // TODO parametrize this strategy as it generates unnecessary matches for image search
                           // if all query matches have been used, lets pick up first as a match
                           // this is done in order to simplify the use case when you have a bigger 
                           // query stride, which generates more matches between query to track entries
                           // acc.List.Add(bestByScore.First());
                           return acc;
                       }, acc => acc.List)
                   .OrderBy(m => m.TrackSequenceNumber);
        }

        private static bool ConsecutiveMatchesAreLongerThanTheQuery(double queryLength, IReadOnlyList<MatchedWith> sortedMatches, int index, double fingerprintLengthInSeconds)
        {
            return SubFingerprintsToSeconds.AdjustLengthToSeconds(sortedMatches[index].TrackMatchAt, sortedMatches[index - 1].TrackMatchAt, fingerprintLengthInSeconds) > queryLength;
        }
    }
}
