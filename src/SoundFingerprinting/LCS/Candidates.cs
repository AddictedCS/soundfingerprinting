namespace SoundFingerprinting.LCS
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.Query;

    internal class Candidates
    {
        private readonly ConcurrentDictionary<IModelReference, List<MatchedWith>> candidates = new ConcurrentDictionary<IModelReference, List<MatchedWith>>();

        public Candidates(IModelReference trackReference, params MatchedWith[] candidates)
        {
            foreach (var candidate in candidates)
            {
                AddNewMatch(trackReference, candidate);
            }
        }

        public IEnumerable<MatchedWith> GetMatchesForTrack(IModelReference trackReference)
        {
            return candidates.TryGetValue(trackReference, out var matchedWith) ? matchedWith : Enumerable.Empty<MatchedWith>();
        }

        public void AddNewMatch(IModelReference trackReference, MatchedWith match)
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
