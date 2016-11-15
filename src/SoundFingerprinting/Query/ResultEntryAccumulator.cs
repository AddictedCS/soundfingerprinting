namespace SoundFingerprinting.Query
{
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.LCS;

    internal class ResultEntryAccumulator
    {
        private readonly SortedSet<SubFingerprintData> matches = new SortedSet<SubFingerprintData>(new SubFingerprintSequenceComparer());

        public int HammingSimilarity { get; set; }

        public double StartAt
        {
            get
            {
                return matches.First().SequenceAt;
            }
        }

        public double EndAt
        {
            get
            {
                return matches.Last().SequenceAt;
            }
        }

        public void Add(SubFingerprintData match)
        {
            matches.Add(match);
        }

        public List<SubFingerprintData> Matches
        {
            get
            {
                return matches.ToList();
            }
        }
    }
}
