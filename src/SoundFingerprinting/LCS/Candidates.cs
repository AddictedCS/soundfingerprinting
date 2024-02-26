namespace SoundFingerprinting.LCS
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using ProtoBuf;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.Query;

    /// <summary>
    ///  Candidates class.
    /// </summary>
    [ProtoContract]
    public class Candidates
    {
        [ProtoMember(1)]
        private readonly ConcurrentDictionary<IModelReference, List<MatchedWith>>? candidates;

        /// <summary>
        ///  Initializes a new instance of the <see cref="Candidates"/> class.
        /// </summary>
        public Candidates()
        {
            candidates = new ConcurrentDictionary<IModelReference, List<MatchedWith>>();
        }
        
        /// <summary>
        ///  Gets count of candidates.
        /// </summary>
        public int Count => candidates?.Count ?? 0;

        /// <summary>
        ///  Gets a value indicating whether candidates are empty.
        /// </summary>
        public bool IsEmpty => candidates?.IsEmpty ?? true;

        /// <summary>
        ///  Gets total count of matches.
        /// </summary>
        public int MatchesCount => candidates?.Sum(pair => pair.Value.Count) ?? 0;
        
        /// <summary>
        ///  Initializes a new instance of the <see cref="Candidates"/> class.
        /// </summary>
        /// <param name="trackReference">First track reference to add.</param>
        /// <param name="candidates">List of matched withs.</param>
        public Candidates(IModelReference trackReference, IEnumerable<MatchedWith> candidates) : this()
        {
            foreach (var candidate in candidates)
            {
                AddMatchesForTrack(trackReference, candidate);
            }
        }

        /// <summary>
        ///  Get matched tracks.
        /// </summary>
        /// <returns>Get all matched tracks.</returns>
        public IEnumerable<IModelReference> GetMatchedTracks()
        {
            return candidates?.Keys ?? Enumerable.Empty<IModelReference>();
        }

        /// <summary>
        ///  Get matches for a particular track.
        /// </summary>
        /// <param name="trackReference">Track reference.</param>
        /// <returns>List of matched withs.</returns>
        public IEnumerable<MatchedWith> GetMatchesForTrack(IModelReference trackReference)
        {
            return (candidates?.TryGetValue(trackReference, out var matchedWith) ?? false) ? matchedWith : Enumerable.Empty<MatchedWith>();
        }

        /// <summary>
        ///  Get matches.
        /// </summary>
        /// <returns>Enumerable of key value pairs.</returns>
        public IEnumerable<KeyValuePair<IModelReference, List<MatchedWith>>> GetMatches()
        {
            return candidates ?? Enumerable.Empty<KeyValuePair<IModelReference, List<MatchedWith>>>();
        }

        /// <summary>
        ///  Add new match for a particular track.
        /// </summary>
        /// <param name="trackReference">Track reference add matched with.</param>
        /// <param name="matches">An instance of <see cref="MatchedWith"/>.</param>
        public void AddMatchesForTrack(IModelReference trackReference, params MatchedWith[] matches)
        {
            foreach (var match in matches)
            {
                candidates?.AddOrUpdate(trackReference, _ => new List<MatchedWith> { match }, (_, old) =>
                {
                    old.Add(match);
                    return old;
                });
            }
        }
    }
}
