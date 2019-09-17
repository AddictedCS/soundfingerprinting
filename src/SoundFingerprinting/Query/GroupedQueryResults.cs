namespace SoundFingerprinting.Query
{
    using System;
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

        private readonly SortedDictionary<uint, Candidates> matches;
        private readonly ConcurrentDictionary<IModelReference, double> similaritySumPerTrack;

        public GroupedQueryResults(double queryLength, DateTime relativeTo)
        {
            RelativeTo = relativeTo;
            QueryLength = queryLength;
            matches = new SortedDictionary<uint, Candidates>();
            similaritySumPerTrack = new ConcurrentDictionary<IModelReference, double>();
        }

        public double QueryLength { get; }

        public DateTime RelativeTo { get; }

        public void Add(HashedFingerprint queryFingerprint, SubFingerprintData resultSubFingerprint, double score)
        {
            lock (lockObject)
            {
                similaritySumPerTrack.AddOrUpdate(resultSubFingerprint.TrackReference, score, (key, old) => old + score);
                var matchedWith = new MatchedWith(queryFingerprint.SequenceNumber, queryFingerprint.StartsAt, resultSubFingerprint.SequenceNumber, resultSubFingerprint.SequenceAt, score);
                if (!matches.TryGetValue(queryFingerprint.SequenceNumber, out var candidates))
                {
                    matches.Add(queryFingerprint.SequenceNumber, new Candidates(resultSubFingerprint.TrackReference, matchedWith));
                }
                else
                {
                    candidates.AddNewMatch(resultSubFingerprint.TrackReference, matchedWith);
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

        public double GetHammingSimilaritySumForTrack(IModelReference trackReference)
        {
            if (similaritySumPerTrack.TryGetValue(trackReference, out double sum))
            {
                return sum;
            }

            return 0;
        }

        public MatchedWith GetBestMatchForTrack(IModelReference trackReference)
        {
            return GetMatchesForTrack(trackReference)
                                .OrderByDescending(matchedWith => matchedWith.HammingSimilarity)
                                .FirstOrDefault();
        }

        public IEnumerable<MatchedWith> GetMatchesForTrack(IModelReference trackReference)
        {
            foreach (var candidates in matches.Values)
            {
                foreach (var match in candidates.GetMatchesForTrack(trackReference))
                {
                    yield return match;
                }
            }
        }
    }
}