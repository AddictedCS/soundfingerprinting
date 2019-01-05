namespace SoundFingerprinting.Query
{
    using System.Collections.Generic;
    using SoundFingerprinting.Command;

    public interface IRealtimeResultEntryAggregator
    {
        RealtimeQueryResult Consume(IEnumerable<ResultEntry> candidates, 
            IRealtimeResultEntryFilter realtimeResultEntryFilter, 
            double queryLength);
    }
}