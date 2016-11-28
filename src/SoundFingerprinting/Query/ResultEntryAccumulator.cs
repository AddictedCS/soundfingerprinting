namespace SoundFingerprinting.Query
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    internal class ResultEntryAccumulator
    {
        private readonly SortedSet<MatchedPair> matches = new SortedSet<MatchedPair>();

        public ResultEntryAccumulator(HashedFingerprint hashedFingerprint, SubFingerprintData match, int hammingSimilarity)
        {
            BestMatch = new MatchedPair(hashedFingerprint, match, hammingSimilarity);
            Add(hashedFingerprint, match, hammingSimilarity);
        }

        public int HammingSimilaritySum { get; private set; }

        public SortedSet<MatchedPair> Matches
        {
            get
            {
                return matches;
            }
        }

        public MatchedPair BestMatch { get; private set; }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public ResultEntryAccumulator Add(HashedFingerprint hashedFingerprint, SubFingerprintData match, int hammingSimilarity)
        {
            HammingSimilaritySum += hammingSimilarity;
            var matchedPair = new MatchedPair(hashedFingerprint, match, hammingSimilarity);
            ResetBestMatchIfAppropriate(matchedPair);
            matches.Add(matchedPair);
            return this;
        }

        private void ResetBestMatchIfAppropriate(MatchedPair newPair)
        {
            if (BestMatch.HammingSimilarity < newPair.HammingSimilarity)
            {
                BestMatch = newPair;
            }
        }
    }
}
