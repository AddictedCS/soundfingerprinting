namespace SoundFingerprinting.LCS
{
    using System.Collections.Concurrent;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.Query;

    /// <summary>
    ///  List of candidate matches. Each track is allowed to have only one best match identified by MatchedWith
    /// </summary>
    internal class Candidates
    {
        private readonly ConcurrentDictionary<IModelReference, MatchedWith> candidates = new ConcurrentDictionary<IModelReference, MatchedWith>();

        public Candidates(IModelReference trackReference, params MatchedWith[] candidates)
        {
            foreach (var candidate in candidates)
            {
                AddOrUpdateNewMatch(trackReference, candidate);
            }
        }

        public bool TryGetMatchesForTrack(IModelReference trackReference, out MatchedWith matchedWith)
        {
            return candidates.TryGetValue(trackReference, out matchedWith);
        }

        public void AddOrUpdateNewMatch(IModelReference trackReference, MatchedWith match)
        {
            candidates.AddOrUpdate(trackReference, reference => match, (reference, old) =>
            {
                if (old.HammingSimilarity > match.HammingSimilarity)
                {
                    return old;
                }

                return match;
            });
        }

        public int Count
        {
            get
            {
                return candidates.Count;
            }
        }
    }
}
