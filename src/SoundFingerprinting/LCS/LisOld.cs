namespace SoundFingerprinting.LCS
{
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.Query;

    public class LisOld
    {
        public static IEnumerable<MatchedWith> GetBestPath(IEnumerable<MatchedWith> matchedEntries, double queryLength, double fingerprintLength)
        {
            var matches = matchedEntries.OrderBy(with => with.TrackMatchAt).ToList();
            var trackRegion = GetTrackRegion(matches, queryLength, fingerprintLength);
            var bestPath = GetBestReconstructedPath(trackRegion, matches);
            return bestPath;
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
            // matches are already sorted by `TrackMatchAt`
            return matches.Skip(trackRegion.StartAt)
                   .Take(trackRegion.Count)
                   .GroupBy(m => m.TrackSequenceNumber)
                   .Aggregate(new { List = new List<MatchedWith>(), Used = new HashSet<uint>()}, (acc, group) =>
                       {
                           var bestByScore = group.OrderByDescending(m => m.Score)
                                                  .ThenBy(m => m.QueryMatchAt)
                                                  .ToList();
                           foreach (var match in bestByScore.Where(match => !acc.Used.Contains(match.QuerySequenceNumber)))
                           {
                               acc.List.Add(match);
                               acc.Used.Add(match.QuerySequenceNumber);
                               return acc;
                           }

                           return acc;
                       }, acc => acc.List)
                   .OrderBy(m => m.TrackSequenceNumber);
        }

        private static bool ConsecutiveMatchesAreLongerThanTheQuery(double queryLength, IReadOnlyList<MatchedWith> sortedMatches, int index, double fingerprintLengthInSeconds)
        {
            return SubFingerprintsToSeconds.MatchLengthToSeconds(sortedMatches[index].TrackMatchAt, sortedMatches[index - 1].TrackMatchAt, fingerprintLengthInSeconds) > queryLength;
        }
    }
}