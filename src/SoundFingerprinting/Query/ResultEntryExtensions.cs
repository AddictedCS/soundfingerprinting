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

            double avgConfidence = (entry.Confidence + with.Confidence) / 2;
            
            return new ResultEntry(entry.Track, 
                entry.QueryMatchStartsAt < with.QueryMatchStartsAt ? entry.QueryMatchStartsAt : with.QueryMatchStartsAt,
                CalculateNewQueryMatchLength(entry, with),
                entry.QueryCoverageLength + with.QueryCoverageLength,
                entry.TrackMatchStartsAt < with.TrackMatchStartsAt ? entry.TrackMatchStartsAt : with.TrackMatchStartsAt,
                entry.TrackMatchStartsAt < with.TrackMatchStartsAt ? entry.TrackStartsAt : with.TrackStartsAt,
                avgConfidence > 1 ? 1 : avgConfidence,
                entry.HammingSimilaritySum + with.HammingSimilaritySum,
                entry.QueryLength + with.QueryLength);
        }
        
        private static double CalculateNewQueryMatchLength(ResultEntry a, ResultEntry b)
        {
            var first = a.TrackMatchStartsAt <= b.TrackMatchStartsAt ? a : b;
            var second = a.TrackMatchStartsAt <= b.TrackMatchStartsAt ? b : a;

            if (first.TrackMatchStartsAt + first.QueryMatchLength >= second.TrackMatchStartsAt + second.QueryMatchLength)
            {
                // A ----------
                // B   -----
                return first.QueryMatchLength;
            }

            if (first.TrackMatchStartsAt <= second.TrackMatchStartsAt && 
                first.TrackMatchStartsAt + first.QueryMatchLength >= second.TrackMatchStartsAt)
            {
                // t  ---5----8----10
                // A     -------
                // B        -------
                return  second.QueryMatchLength - first.TrackMatchStartsAt + second.TrackMatchStartsAt;

            }
            
            // A  -------
            // B           ------
            // not glued on purpose
            return first.QueryMatchLength + second.QueryMatchLength;
        }
    }
}