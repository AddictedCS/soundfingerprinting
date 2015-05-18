namespace SoundFingerprinting.Query
{
    using System.Collections.Generic;
    using System.Linq;

    public class QueryResult
    {
        public bool IsSuccessful { get; set; }

        public List<ResultEntry> ResultEntries { get; set; }

        public int AnalyzedCandidatesCount { get; set; }

        public ResultEntry BestMatch
        {
            get
            {
                if (ResultEntries != null && ResultEntries.Any())
                {
                    return ResultEntries[0];
                }

                return null;
            }
        }

        public SequenceInfo TimeInfo
        {
            get
            {
                if (BestMatch != null && ContainsSequenceInfo(BestMatch))
                {
                    return new SequenceInfo { Start = BestMatch.SequenceStart, Length = BestMatch.SequenceLength };
                }

                return null;
            }
        }

        private bool ContainsSequenceInfo(ResultEntry bestMatch)
        {
            return !bestMatch.SequenceStart.Equals(0) && !bestMatch.SequenceLength.Equals(0);
        }
    }
}