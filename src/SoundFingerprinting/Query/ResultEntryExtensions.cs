namespace SoundFingerprinting.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.Data;

    public static class ResultEntryExtensions
    {
        public static IEnumerable<QueryMatch> ToQueryMatches(this IEnumerable<ResultEntry> resultEntries)
        {
            var queryMatches = resultEntries.Select(resultEntry =>
                {
                    var trackData = resultEntry.Track;
                    var trackInfo = new TrackInfo(trackData.Id, trackData.Title, trackData.Artist, trackData.MetaFields, trackData.MediaType);
                    var queryMatchId = Guid.NewGuid().ToString();
                    return new QueryMatch(queryMatchId, trackInfo, resultEntry.Coverage, resultEntry.MatchedAt);
                })
                .ToList();
            return queryMatches;
        }
        
        public static ResultEntry MergeWith(this ResultEntry entry, ResultEntry with)
        {
            if (!entry.Track.Equals(with.Track))
            {
                throw new ArgumentException($"{nameof(with)} merging entries should correspond to the same track");
            }

            double avgConfidence = Math.Min((entry.Confidence + with.Confidence) / 2, 1d);

            var queryMatchStartsAt = entry.TrackMatchStartsAt < with.TrackMatchStartsAt ? entry.QueryMatchStartsAt : with.QueryMatchStartsAt;
            var trackMatchStartsAt = entry.TrackMatchStartsAt < with.TrackMatchStartsAt ? entry.TrackMatchStartsAt : with.TrackMatchStartsAt;
            var trackStartsAt = entry.TrackMatchStartsAt < with.TrackMatchStartsAt ? entry.TrackStartsAt : with.TrackStartsAt;
            var matchedAt = entry.MatchedAt < with.MatchedAt ? entry.MatchedAt : with.MatchedAt;

            return new ResultEntry(entry.Track,
                avgConfidence,
                entry.Score + with.Score,
                matchedAt,
                CalculateNewQueryLength(entry, with),
                queryMatchStartsAt,
                CalculateNewQueryCoverage(entry, with),
                CalculateNewMatchLength(entry, with),
                trackMatchStartsAt,
                trackStartsAt);
        }
        
        private static double CalculateNewQueryCoverage(ResultEntry a, ResultEntry b)
        {
            var first = a.TrackMatchStartsAt <= b.TrackMatchStartsAt ? a : b;
            var second = a.TrackMatchStartsAt <= b.TrackMatchStartsAt ? b : a;

            if (first.TrackMatchStartsAt + first.CoverageWithPermittedGapsLength >= second.TrackMatchStartsAt + second.CoverageWithPermittedGapsLength)
            {
                // a ---------
                // b   -----
                return first.CoverageWithPermittedGapsLength;
            }

            if (first.TrackMatchStartsAt <= second.TrackMatchStartsAt && first.TrackMatchStartsAt + first.CoverageWithPermittedGapsLength >= second.TrackMatchStartsAt)
            {
                // a     -------
                // b          -------
                return  second.CoverageWithPermittedGapsLength - first.TrackMatchStartsAt + second.TrackMatchStartsAt;

            }
            
            // a  -------
            // b            ------
            // not glued on purpose
            return first.CoverageWithPermittedGapsLength + second.CoverageWithPermittedGapsLength;
        }
        
        private static double CalculateNewMatchLength(ResultEntry a, ResultEntry b)
        {
            var first = a.TrackMatchStartsAt <= b.TrackMatchStartsAt ? a : b;
            var second = a.TrackMatchStartsAt <= b.TrackMatchStartsAt ? b : a;

            if (first.TrackMatchStartsAt + first.DiscreteCoverageLength >= second.TrackMatchStartsAt + second.DiscreteCoverageLength)
            {
                return first.DiscreteCoverageLength;
            }

            if (first.TrackMatchStartsAt <= second.TrackMatchStartsAt && first.TrackMatchStartsAt + first.DiscreteCoverageLength >= second.TrackMatchStartsAt)
            {
                return  second.DiscreteCoverageLength - first.TrackMatchStartsAt + second.TrackMatchStartsAt;

            }
            
            return first.DiscreteCoverageLength + second.DiscreteCoverageLength;
        }

        private static double CalculateNewQueryLength(ResultEntry a, ResultEntry b)
        {
            if (Math.Abs(a.TrackMatchStartsAt - b.TrackMatchStartsAt) < 0.0001)
            {
                // same start
                return a.QueryLength;
            }

            // t --------------
            // a      ---------
            // b   --------
            if (a.TrackMatchStartsAt > b.TrackMatchStartsAt)
            {
                double diff = a.TrackMatchStartsAt - b.TrackMatchStartsAt;
                if (diff > b.TrackMatchStartsAt)
                {
                    return a.QueryLength + b.QueryLength;
                }

                return diff + a.QueryLength;
            }
            else
            {
                // t -------------
                // a  ------
                // b      -----
                double diff = b.TrackMatchStartsAt - a.TrackMatchStartsAt;
                if (diff > a.QueryLength)
                {
                    return a.QueryLength + b.QueryLength;
                }

                return diff + b.QueryLength;
            }
        }
    }
}