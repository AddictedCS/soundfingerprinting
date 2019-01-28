namespace SoundFingerprinting.LCS
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.Query;

    internal class Candidates
    {
        private readonly ConcurrentDictionary<IModelReference, List<MatchedWith>> candidates = new ConcurrentDictionary<IModelReference, List<MatchedWith>>();

        public Candidates(IModelReference trackReference, params MatchedWith[] candidates)
        {
            foreach (var candidate in candidates)
            {
                AddOrUpdateNewMatch(trackReference, candidate);
            }
        }

        public bool TryGetMatchesForTrack(IModelReference trackReference, out List<MatchedWith> matchedWith)
        {
            return candidates.TryGetValue(trackReference, out matchedWith);
        }

        public void AddOrUpdateNewMatch(IModelReference trackReference, MatchedWith match)
        {
            candidates.AddOrUpdate(trackReference, reference => new List<MatchedWith> {match}, (reference, old) =>
            {
                old.Add(match);
                return old;
            });
        }

        public int Count => candidates.Count;
    }
}
