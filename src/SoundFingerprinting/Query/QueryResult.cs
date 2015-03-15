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

        public double SequenceLength { get; set; }

        public double SequenceStart { get; set; }
    }
}