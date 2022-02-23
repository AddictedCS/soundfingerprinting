namespace SoundFingerprinting.Query
{
    using System.Collections.Generic;
    using SoundFingerprinting.Data;

    internal interface IQueryHashesConcatenator
    {
        void UpdateHashesForTracks(IEnumerable<string> trackIds, AVHashes hashes, AVQueryCommandStats commandStats);
        
        IEnumerable<AVQueryResult> GetQueryResults(IEnumerable<AVResultEntry> completed);
        
        void Cleanup(IEnumerable<string> purgedTrackIds);
    }
}