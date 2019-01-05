namespace SoundFingerprinting.Query
{
    using System.Collections.Generic;

    public class RealtimeQueryResult
    {
        public RealtimeQueryResult(IEnumerable<ResultEntry> successEntries, IEnumerable<ResultEntry> didNotPassThresholdEntries)
        {
            SuccessEntries = successEntries;
            DidNotPassThresholdEntries = didNotPassThresholdEntries;
        }
        
        public IEnumerable<ResultEntry> SuccessEntries { get; }
        
        public IEnumerable<ResultEntry> DidNotPassThresholdEntries { get; }
    }
}