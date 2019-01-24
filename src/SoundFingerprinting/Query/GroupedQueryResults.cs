namespace SoundFingerprinting.Query
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.LCS;

    public class GroupedQueryResults
    {
        private readonly object lockObject = new object();

        private readonly double queryLength;
        private readonly SortedDictionary<uint, Candidates> matches;
        private readonly ConcurrentDictionary<IModelReference, int> similaritySumPerTrack;

        public GroupedQueryResults(double queryLength)
        {
            this.queryLength = queryLength;
            matches = new SortedDictionary<uint, Candidates>();
            similaritySumPerTrack = new ConcurrentDictionary<IModelReference, int>();
        }

        public double GetQueryLength()
        {
            return queryLength;
        }

        public void Add(HashedFingerprint hashedFingerprint, SubFingerprintData subFingerprintData, int hammingSimilarity)
        {
            lock (lockObject)
            {
                similaritySumPerTrack.AddOrUpdate(subFingerprintData.TrackReference, hammingSimilarity, (key, oldHamming) => oldHamming + hammingSimilarity);
                var matchedWith = new MatchedWith(hashedFingerprint.StartsAt, subFingerprintData.SequenceAt, hammingSimilarity);
                if (!matches.TryGetValue(hashedFingerprint.SequenceNumber, out var matched))
                {
                    matches.Add(hashedFingerprint.SequenceNumber, new Candidates(subFingerprintData.TrackReference, matchedWith));
                }
                else
                {
                    matched.AddOrUpdateNewMatch(subFingerprintData.TrackReference, matchedWith);
                }
            }
        }

        public bool ContainsMatches => similaritySumPerTrack.Any();

        public int SubFingerprintsCount
        {
            get
            {
                return matches.Values.Select(candidates => candidates.Count).Sum();
            }
        }

        public int TracksCount => similaritySumPerTrack.Count;

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
            return GetMatchesForTrackOrderedByQueryAt(trackReference)
                                .OrderByDescending(matchedWith => matchedWith.HammingSimilarity)
                                .FirstOrDefault();
        }

        public IEnumerable<MatchedWith> GetMatchesForTrackOrderedByQueryAt(IModelReference trackReference)
        {
            foreach(var valuePair in matches)
            {
                if (valuePair.Value.TryGetMatchesForTrack(trackReference, out var matchedWith))
                {
                    foreach (var with in matchedWith)
                    {
                        yield return with;
                    }
                }
            }
        }
    }
}