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
            
            return new ResultEntry(entry.Track, 
                entry.QueryMatchStartsAt < with.QueryMatchStartsAt ? entry.QueryMatchStartsAt : with.QueryMatchStartsAt,
                entry.QueryMatchLength + with.QueryMatchLength,
                entry.QueryCoverageLength + with.QueryCoverageLength,
                entry.TrackMatchStartsAt < with.TrackMatchStartsAt ? entry.TrackMatchStartsAt : with.TrackMatchStartsAt,
                entry.TrackStartsAt < with.TrackStartsAt ? entry.TrackStartsAt : with.TrackStartsAt,
                entry.Confidence + with.Confidence > 1 ? 1 : entry.Confidence + with.Confidence,
                entry.HammingSimilaritySum + with.HammingSimilaritySum,
                entry.QueryLength + with.QueryLength);
        }
    }
}