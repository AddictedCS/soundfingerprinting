namespace Soundfingerprinting.Query
{
    using Soundfingerprinting.Dao.Entities;

    public class QueryResult
    {
        public bool IsSuccessful { get; set; }

        public Track BestMatch { get; set; }
    }
}