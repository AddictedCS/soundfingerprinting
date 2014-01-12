namespace SoundFingerprinting.Query
{
    using System.Collections.Generic;
    using SoundFingerprinting.Data;

    public class QueryResult
    {
        public bool IsSuccessful { get; set; }

        public List<ResultData> Results { get; set; }

        public int TotalNumberOfAnalyzedCandidates { get; set; }
}
