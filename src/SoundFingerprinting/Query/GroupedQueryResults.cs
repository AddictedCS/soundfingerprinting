namespace SoundFingerprinting.Query
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    using SoundFingerprinting.Data;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;

    internal class GroupedQueryResults
    {
        private readonly MatchedPair[] matches;
        private readonly ConcurrentDictionary<IModelReference, int> similaritySumPerTrack;

        public GroupedQueryResults(int queryLength)
        {
            matches = new MatchedPair[queryLength];
            similaritySumPerTrack = new ConcurrentDictionary<IModelReference, int>();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Add(HashedFingerprint hashedFingerprint, SubFingerprintData subFingerprintData, int hammingSimilarity)
        {
            similaritySumPerTrack.AddOrUpdate(subFingerprintData.TrackReference, hammingSimilarity, (key, old) => old + hammingSimilarity);
            if (matches[hashedFingerprint.SequenceNumber] == null)
            {
                matches[hashedFingerprint.SequenceNumber] = new MatchedPair(hashedFingerprint, subFingerprintData, hammingSimilarity);
                return;
            }

            var trackReference = subFingerprintData.TrackReference;
            var matched = matches[hashedFingerprint.SequenceNumber];

            matched.Matches.AddOrUpdate(trackReference, key => new MatchedWith(hashedFingerprint.StartsAt, subFingerprintData.SequenceAt, hammingSimilarity),
                (key, old) =>
                {
                    if (old.HammingSimilarity > hammingSimilarity)
                    {
                        return old;
                    }

                    return new MatchedWith(hashedFingerprint.StartsAt, subFingerprintData.SequenceAt, hammingSimilarity);
                });
        }

        public bool ContainsMatches
        {
            get
            {
                return similaritySumPerTrack.Any();
            }
        }

        public int SubFingerprintsCount
        {
            get
            {
                return matches.Select(match => match?.Matches.Count ?? 0).Sum();
            }
        }

        public int TracksCount
        {
            get
            {
                return similaritySumPerTrack.Count;
            }
        }

        public IEnumerable<IModelReference> GetTopTracksByHammingSimilarity(int count)
        {
            var sorted = from entry in similaritySumPerTrack orderby entry.Value descending select entry;
            int c = 0;
            foreach (var entry in sorted)
            {
                yield return entry.Key;
                c++;
                if (c >= count)
                    break;
            }
        }

        public int GetHammingSimilaritySumForTrack(IModelReference trackReference)
        {
            if (similaritySumPerTrack.TryGetValue(trackReference, out int sum))
            {
                return sum;
            }

            return 0;
        }

        public MatchedWith GetBestMatchForTrack(IModelReference trackReference)
        {
            return GetOrderedMatchesForTrack(trackReference).OrderByDescending(matchedWith => matchedWith.HammingSimilarity).FirstOrDefault();
        }

        public IEnumerable<MatchedWith> GetOrderedMatchesForTrack(IModelReference trackReference)
        {
            foreach (var match in matches)
            {
                if (match != null && match.Matches.TryGetValue(trackReference, out var matchedWith))
                {
                    yield return matchedWith;
                }
            }
        }
    }
}