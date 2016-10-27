namespace SoundFingerprinting.LCS
{
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;

    internal class AudioSequencesAnalyzer : IAudioSequencesAnalyzer
    {
        private const int AllowedMissalignment = 2;

        // TODO Loose comparison, if 2 sequences are equal last added will be selected as the winner
        private readonly Comparer<IEnumerable<SubFingerprintData>> candiateLengthComparer = Comparer<IEnumerable<SubFingerprintData>>.Create((a, b) => b.Count().CompareTo(a.Count()));

        public IEnumerable<IEnumerable<SubFingerprintData>> SortCandiatesByLongestIncresingAudioSequence(Dictionary<IModelReference, SubfingerprintSetSortedByTimePosition> candidates, double queryLength)
        {
            var resultSet = new SortedSet<IEnumerable<SubFingerprintData>>(this.candiateLengthComparer);

            foreach (var candidate in candidates)
            {
                var lcs = SortCandiatesByLongestIncresingAudioSequence(candidate.Value, queryLength);
                foreach (var lc in lcs)
                {
                    resultSet.Add(lc);
                }
            }

            return resultSet;
        }

        private IEnumerable<IEnumerable<SubFingerprintData>> SortCandiatesByLongestIncresingAudioSequence(SortedSet<SubFingerprintData> set, double queryLength)
        {
            if (set == null || set.Count == 0)
            {
                return new List<IEnumerable<SubFingerprintData>>();
            }

            if (set.Count == 1)
            {
                return new List<List<SubFingerprintData>> { set.ToList() };
            }

            int[] maxs = new int[set.Count];
            int[] ind = new int[set.Count];
            for (int i = 0; i < set.Count; i++)
            {
                maxs[i] = 1;
            }

            int maxLen = 1;
            var sortedSequence = set.ToList();
            for (int i = 1; i < sortedSequence.Count; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    if (sortedSequence[i].SequenceAt > sortedSequence[j].SequenceAt
                        && sortedSequence[i].SequenceAt - sortedSequence[j].SequenceAt <= queryLength)
                    {
                        if (maxs[j] + 1 > maxs[i])
                        {
                            maxs[i] = maxs[j] + 1;
                            ind[i] = j;
                        }

                        if (maxLen < maxs[i])
                        {
                            maxLen = maxs[i];
                        }
                    }
                }
            }

            var allCandidates = new List<List<SubFingerprintData>>();
            for (int diff = 0; diff <= AllowedMissalignment; diff++)
            {
                for (int i = 0; i < maxs.Length; i++)
                {
                    if (maxLen == maxs[i])
                    {
                        Stack<SubFingerprintData> candidate = new Stack<SubFingerprintData>();
                        int last = i;
                        for (int k = 0; k < maxLen; k++)
                        {
                            candidate.Push(sortedSequence[last]);
                            maxs[last] = 0;
                            last = ind[last];
                        }

                        allCandidates.Add(candidate.ToList());
                    }
                }

                maxLen = maxLen - 1;
                if (maxLen == 0)
                {
                    break;
                }
            }

            return allCandidates;
        }
    }
}
