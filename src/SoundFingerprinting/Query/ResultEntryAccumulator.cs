namespace SoundFingerprinting.Query
{
    using System.Collections.Generic;

    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    internal class ResultEntryAccumulator
    {
        private readonly SortedSet<MatchedPair> matches = new SortedSet<MatchedPair>();

        public int HammingSimilaritySum { get; set; }

        public SortedSet<MatchedPair> Matches
        {
            get
            {
                return matches;
            }
        }

        public MatchedPair BestMatch { get; private set; }

        public void Add(HashedFingerprint hashedFingerprint,  SubFingerprintData match, int hammingSimilarity)
        {
            var matchedPair = new MatchedPair(hashedFingerprint, match, hammingSimilarity);
            ResetBestMatchIfAppropriate(matchedPair);
            matches.Add(matchedPair);
        }

        private void ResetBestMatchIfAppropriate(MatchedPair newPair)
        {
            if (BestMatch == null)
            {
                BestMatch = newPair;
                return;
            }

            if (BestMatch.HammingSimilarity < newPair.HammingSimilarity)
            {
                BestMatch = newPair;
            }
        }
    }
}
