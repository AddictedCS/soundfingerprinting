namespace SoundFingerprinting.Query
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.LCS;

    internal class GroupedQueryResults
    {
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

        public void Add(uint queryHashSequenceNumber, IModelReference trackReference, MatchedWith matchedWith)
        {
            scoreSumPerTrack.AddOrUpdate(trackReference, matchedWith.Score, (_, old) => old + matchedWith.Score);
            if (!sequenceToCandidates.TryGetValue(queryHashSequenceNumber, out var candidates))
            {
                sequenceToCandidates.Add(queryHashSequenceNumber, new Candidates(trackReference, new[] { matchedWith }));
            }
            else
            {
                candidates.AddNewMatchForTrack(trackReference, matchedWith);
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