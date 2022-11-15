namespace SoundFingerprinting.LCS
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.Query;

    internal class Candidates
    {
        private readonly ConcurrentDictionary<IModelReference, List<MatchedWith>> candidates = new ();

        public Candidates(IModelReference trackReference, params MatchedWith[] candidates)
        {
            foreach (var candidate in candidates)
            {
                AddNewMatchForTrack(trackReference, candidate);
            }
        }

        public IEnumerable<MatchedWith> GetMatchesForTrack(IModelReference trackReference)
        {
            return candidates.TryGetValue(trackReference, out var matchedWith) ? matchedWith : Enumerable.Empty<MatchedWith>();
        }

        public void AddNewMatchForTrack(IModelReference trackReference, MatchedWith match)
        {
            candidates.AddOrUpdate(trackReference, _ => new List<MatchedWith> {match}, (_, old) =>
            {
                old.Add(match);
                return old;
            });
        }

        public int Count => candidates.Count;
    }
}
