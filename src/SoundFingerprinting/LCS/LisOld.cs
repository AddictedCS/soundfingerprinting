namespace SoundFingerprinting.LCS
{
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.Query;
    using static SoundFingerprinting.Query.SubFingerprintsToSeconds;

    public class LisOld
    {
        /// <summary>
        ///  Get one best path out of all candidates. Allows best path to contain revert matches (empirically proven to work better).
        ///  If you need strictly increasing sequence matches, set AllowMultipleMatchesInQuery to true.
        /// </summary>
        /// <param name="matchedEntries">All matched entries</param>
        /// <param name="queryLength">Query length</param>
        /// <param name="trackLength">Track length</param>
        /// <param name="fingerprintLength">Fingerprint length</param>
        /// <returns>Best computed path</returns>
        public static IEnumerable<MatchedWith> GetBestPath(
            IEnumerable<MatchedWith> matchedEntries, 
            double queryLength, 
            double trackLength,
            double fingerprintLength)
        {
            var orderedByTrackMatchAt = matchedEntries.OrderBy(with => with.TrackMatchAt).ToList();
            var trackRegion = GetTrackRegion(orderedByTrackMatchAt, System.Math.Min(queryLength, trackLength), fingerprintLength);
            var bestPath = GetBestReconstructedPath(trackRegion, orderedByTrackMatchAt);
            return bestPath;
        }

        private static TrackRegion GetTrackRegion(IReadOnlyList<MatchedWith> orderedByTrackMatchAt, double maxGap, double fingerprintLengthInSeconds)
        {
            int minI = 0, maxI = 0, curMinI = 0, maxLength = 0;
            for (int i = 1; i < orderedByTrackMatchAt.Count; ++i)
            {
                // since we don't allow same multiple matches in the query, we check to see if this is a start of new best sequence 
                if (ConsecutiveMatchesAreLongerThanMaxGap(maxGap, orderedByTrackMatchAt, i, fingerprintLengthInSeconds))
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
                           var bestByScore = group
                                                  .OrderByDescending(m => m.Score)
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

        private static bool ConsecutiveMatchesAreLongerThanMaxGap(double maxGap, IReadOnlyList<MatchedWith> sortedByTrackAtMatches, int index, double fingerprintLengthInSeconds)
        {
            // order is not guaranteed for query matches as we are sorted by trackAt
            var (queryEndsAt, queryStartsAt) = sortedByTrackAtMatches[index].QueryMatchAt > sortedByTrackAtMatches[index - 1].QueryMatchAt
                ? (sortedByTrackAtMatches[index].QueryMatchAt, sortedByTrackAtMatches[index - 1].QueryMatchAt)
                : (sortedByTrackAtMatches[index - 1].QueryMatchAt, sortedByTrackAtMatches[index].QueryMatchAt);

            return MatchLengthToSeconds(sortedByTrackAtMatches[index].TrackMatchAt, sortedByTrackAtMatches[index - 1].TrackMatchAt, fingerprintLengthInSeconds) > maxGap ||
                   MatchLengthToSeconds(queryEndsAt, queryStartsAt, fingerprintLengthInSeconds) > maxGap;
        }
    }
}