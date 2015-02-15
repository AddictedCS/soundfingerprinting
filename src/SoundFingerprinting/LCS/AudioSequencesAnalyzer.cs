namespace SoundFingerprinting.LCS
{
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Data;

    public class AudioSequencesAnalyzer : IAudioSequencesAnalyzer
    {
        public IEnumerable<SubFingerprintData> GetLongestIncreasingSubSequence(
            List<SubFingerprintData> sequence)
        {
            int len = sequence.Count;
            const int Delta = 2;
            int[] maxs = new int[len];
            int max = 0, index = 0;
            for (int i = 1; i < len; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    if (sequence[i].SequenceNumber > sequence[j].SequenceNumber && sequence[i].SequenceNumber - sequence[j].SequenceNumber <= Delta)
                    {
                        maxs[i] = System.Math.Max(maxs[j] + 1, maxs[i]);
                        if (max < maxs[i])
                        {
                            max = maxs[i];
                            index = i;
                        }
                    }
                }
            }

            List<int> seq = new List<int> { index };
            max--;
            int r = index;
            while (--r >= 0)
            {
                if (maxs[r] == max && sequence[r].SequenceNumber < sequence[seq.Last()].SequenceNumber)
                {
                    seq.Add(r);
                    max--;
                }
            }

            seq.Reverse();

            return sequence.Where((s, i) => seq.Contains(i));
        }
    }
}
