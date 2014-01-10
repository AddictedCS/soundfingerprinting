namespace SoundFingerprinting.Query
{
    using SoundFingerprinting.Data;

    public class QueryResult
    {
        public bool IsSuccessful { get; set; }

        public int Similarity { get; set; }

        public TrackData BestMatch { get; set; }

        public int NumberOfCandidates { get; set; }
    }
}
