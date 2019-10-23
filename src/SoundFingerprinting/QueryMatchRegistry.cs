namespace SoundFingerprinting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Query;

    public abstract class QueryMatchRegistry : IQueryMatchRegistry
    {
        public abstract void RegisterMatches(IEnumerable<QueryMatch> queryMatches);

        public virtual void RegisterMatches(IEnumerable<ResultEntry> resultEntries)
        {
            var queryMatches = resultEntries.Select(resultEntry =>
                new QueryMatch(Guid.NewGuid().ToString(),
                    resultEntry.Track.Id,
                    resultEntry.CoverageLength,
                    resultEntry.QueryMatchStartsAt, resultEntry.TrackStartsAt,
                    resultEntry.TrackMatchStartsAt, resultEntry.Confidence, resultEntry.QueryLength,
                    resultEntry.MatchedAt, resultEntry.Track.Length));
            RegisterMatches(queryMatches);
        }
    }
}