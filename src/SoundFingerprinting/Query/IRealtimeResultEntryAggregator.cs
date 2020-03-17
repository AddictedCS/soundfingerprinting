namespace SoundFingerprinting.Query
{
    using System.Collections.Generic;

    public interface IRealtimeResultEntryAggregator
    {
        RealtimeQueryResult Consume(IEnumerable<ResultEntry> candidates, double queryLength);
    }
}