namespace SoundFingerprinting.Query;

using System;
using System.Collections.Generic;

internal interface IOngoingAvResultEntryTracker
{
    void AddOrUpdate(AVResultEntry avResultEntry, Func<AVResultEntry, AVResultEntry> updateFunc);

    IEnumerable<string> GetTrackIds();
    
    IEnumerable<AVResultEntry> GetAvResultEntries();
    
    IEnumerable<AVResultEntry> GetAvResultEntriesExcept(IEnumerable<AVResultEntry> set);
    
    bool TryRemove(AVResultEntry avResultEntry);
    
    void Clear();
    
    IEnumerable<AVResultEntry> GetAndRemoveTimeShiftedEntries();
}