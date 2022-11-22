namespace SoundFingerprinting.Query
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.LCS;

    internal class GroupedQueryResults
    {
        private readonly object lockObject = new ();

        private readonly SortedDictionary<uint, Candidates> sequenceToCandidates;
        private readonly ConcurrentDictionary<IModelReference, double> scoreSumPerTrack;

        public GroupedQueryResults(double queryLength, DateTime relativeTo)
        {
            RelativeTo = relativeTo;
            QueryLength = queryLength;
            sequenceToCandidates = new SortedDictionary<uint, Candidates>();
            scoreSumPerTrack = new ConcurrentDictionary<IModelReference, double>();
        }

        public double QueryLength { get; }

        public DateTime RelativeTo { get; }

        public void Add(HashedFingerprint queryFingerprint, SubFingerprintData resultSubFingerprint, double score)
        {
            lock (lockObject)
            {
                scoreSumPerTrack.AddOrUpdate(resultSubFingerprint.TrackReference, score, (_, old) => old + score);
                var matchedWith = new MatchedWith(queryFingerprint.SequenceNumber, queryFingerprint.StartsAt, resultSubFingerprint.SequenceNumber, resultSubFingerprint.SequenceAt, score);
                if (!sequenceToCandidates.TryGetValue(queryFingerprint.SequenceNumber, out var candidates))
                {
                    sequenceToCandidates.Add(queryFingerprint.SequenceNumber, new Candidates(resultSubFingerprint.TrackReference, matchedWith));
                }
                else
                {
                    candidates.AddNewMatchForTrack(resultSubFingerprint.TrackReference, matchedWith);
                }
            }
        }

        public bool ContainsMatches => scoreSumPerTrack.Any();

        public int SubFingerprintsCount
        {
            get
            {
                return sequenceToCandidates.Values.Select(candidates => candidates.Count).Sum();
            }
        }

        public int TracksCount => scoreSumPerTrack.Count;

        public IEnumerable<IModelReference> GetTopTracksByScore(int count)
        {
            return scoreSumPerTrack
                .OrderByDescending(_ => _.Value)
                .Select(_ => _.Key)
                .Take(count);
        }

        public double GetScoreSumForTrack(IModelReference trackReference)
        {
            return scoreSumPerTrack.TryGetValue(trackReference, out double scoreSum) ? scoreSum : 0d;
        }

        public IEnumerable<MatchedWith> GetMatchesForTrack(IModelReference trackReference)
        {
            return sequenceToCandidates.Values.SelectMany(candidates => candidates.GetMatchesForTrack(trackReference));
        }
    }
}