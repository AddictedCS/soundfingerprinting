namespace SoundFingerprinting.Query
{
    using System.Collections.Generic;

    public class QueryResult
    {
        public bool IsSuccessful { get; set; }

        public List<ResultData> Results { get; set; }

        public int TotalNumberOfAnalyzedCandidates { get; set; }
    }
}
