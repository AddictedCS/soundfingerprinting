namespace SoundFingerprinting.Query;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

internal class StatefulOngoingAvResultEntryTracker : IOngoingAvResultEntryTracker
{
    private readonly ConcurrentDictionary<string, AVResultEntry> avResultEntries = new ();
    private readonly Queue<AVResultEntry> timeShiftedEntries = new ();
    
    public void AddOrUpdate(AVResultEntry avResultEntry, Func<AVResultEntry, AVResultEntry> updateFunc)
    {
        var key = GetKey(avResultEntry);
        avResultEntries.AddOrUpdate(key, avResultEntry, (_, previousAvEntry) =>
        {
            var newAvEntry = updateFunc(previousAvEntry);
            if (!previousAvEntry.IsEquivalent(newAvEntry))
            {
                // we have a time shift, hence we need to purge previous value, as it will not be tracked anymore
                timeShiftedEntries.Enqueue(previousAvEntry);
            }

            return newAvEntry;
        });
    }

    public IEnumerable<string> GetTrackIds()
    {
        return new HashSet<string>(avResultEntries.Values.Select(_ => _.TrackId));
    }
    
    public IEnumerable<AVResultEntry> GetAvResultEntries()
    {
        return avResultEntries.Values.ToList();
    }
    
    public IEnumerable<AVResultEntry> GetAndRemoveTimeShiftedEntries()
    {
        while (timeShiftedEntries.Count > 0)
        {
            yield return timeShiftedEntries.Dequeue();
        }
    }

    public IEnumerable<AVResultEntry> GetAvResultEntriesExcept(IEnumerable<AVResultEntry> set)
    {
        var setKeys = new HashSet<string>(set.Select(GetKey));
        return avResultEntries.Where(kv => !setKeys.Contains(kv.Key)).Select(kv => kv.Value).ToList();
    }
    
    public bool TryRemove(AVResultEntry avResultEntry)
    {
        string key = GetKey(avResultEntry);
        return avResultEntries.TryRemove(key, out _);
    }

    public void Clear()
    {
        avResultEntries.Clear();
    }

    private static string GetKey(AVResultEntry avResultEntry)
    {
        return avResultEntry.TrackId;
    }
}