namespace SoundFingerprinting.Query
{
    using SoundFingerprinting.Dao.Entities;

    public class QueryResult
    {
        public bool IsSuccessful { get; set; }

        public Track BestMatch { get; set; }
    }
}