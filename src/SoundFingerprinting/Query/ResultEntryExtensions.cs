namespace SoundFingerprinting.Query
{
    using System;

    public static class ResultEntryExtensions
    {
        public static ResultEntry MergeWith(this ResultEntry entry, ResultEntry with)
        {
            if (!entry.Track.Equals(with.Track))
            {
                throw new ArgumentException($"{nameof(with)} merging entries should correspond to the same track");
            }

            double avgConfidence = Math.Min((entry.Confidence + with.Confidence) / 2, 1d);
            
            return new ResultEntry(entry.Track, 
                entry.TrackMatchStartsAt < with.TrackMatchStartsAt ? entry.QueryMatchStartsAt : with.QueryMatchStartsAt,
                CalculateNewQueryCoverage(entry, with),
                CalculateNewMatchLength(entry, with),
                entry.TrackMatchStartsAt < with.TrackMatchStartsAt ? entry.TrackMatchStartsAt : with.TrackMatchStartsAt,
                entry.TrackMatchStartsAt < with.TrackMatchStartsAt ? entry.TrackStartsAt : with.TrackStartsAt,
                avgConfidence,
                entry.Score + with.Score,
                CalculateNewQueryLength(entry, with),
                entry.MatchedAt < with.MatchedAt ? entry.MatchedAt : with.MatchedAt);
        }
        
        private static double CalculateNewQueryCoverage(ResultEntry a, ResultEntry b)
        {
            var first = a.TrackMatchStartsAt <= b.TrackMatchStartsAt ? a : b;
            var second = a.TrackMatchStartsAt <= b.TrackMatchStartsAt ? b : a;

            if (first.TrackMatchStartsAt + first.CoverageLength >= second.TrackMatchStartsAt + second.CoverageLength)
            {
                // a ---------
                // b   -----
                return first.CoverageLength;
            }

            if (first.TrackMatchStartsAt <= second.TrackMatchStartsAt && first.TrackMatchStartsAt + first.CoverageLength >= second.TrackMatchStartsAt)
            {
                // a     -------
                // b          -------
                return  second.CoverageLength - first.TrackMatchStartsAt + second.TrackMatchStartsAt;

            }
            
            // a  -------
            // b            ------
            // not glued on purpose
            return first.CoverageLength + second.CoverageLength;
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