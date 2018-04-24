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
        private readonly SortedDictionary<int, MatchedPair> matches;
        private readonly ConcurrentDictionary<IModelReference, int> similaritySumPerTrack;

        public GroupedQueryResults()
        {
            matches = new SortedDictionary<int, MatchedPair>();
            similaritySumPerTrack = new ConcurrentDictionary<IModelReference, int>();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Add(HashedFingerprint hashedFingerprint, SubFingerprintData subFingerprintData, int hammingSimilarity)
        {
            similaritySumPerTrack.AddOrUpdate(subFingerprintData.TrackReference, hammingSimilarity, (key, oldHamming) => oldHamming + hammingSimilarity);

            if (!matches.ContainsKey((int)hashedFingerprint.SequenceNumber))
            {
                matches.Add((int)hashedFingerprint.SequenceNumber, new MatchedPair(hashedFingerprint, subFingerprintData, hammingSimilarity));
            }
            else
            {
                var matched = matches[(int)hashedFingerprint.SequenceNumber];
                var trackReference = subFingerprintData.TrackReference;
                matched.Matches.AddOrUpdate(trackReference, reference => new MatchedWith(hashedFingerprint.StartsAt, subFingerprintData.SequenceAt, hammingSimilarity),
                    (reference, matchedWith) =>
                    {
                        if (matchedWith.HammingSimilarity > hammingSimilarity)
                        {
                            return matchedWith;
                        }

                        return new MatchedWith(hashedFingerprint.StartsAt, subFingerprintData.SequenceAt, hammingSimilarity);
                    });
            }
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
                return matches.Values.Select(match => match.Matches.Count).Sum();
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
            foreach(var valuePair in matches)
            {
                if (valuePair.Value.Matches.TryGetValue(trackReference, out var matchedWith))
                {
                    yield return matchedWith;
                }
            }
        }
    }
}