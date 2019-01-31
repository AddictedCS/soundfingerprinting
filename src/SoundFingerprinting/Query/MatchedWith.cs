namespace SoundFingerprinting.Query
{
    public class MatchedWith
    {
        public MatchedWith(float queryMatchAt, float trackMatchAt, int hammingSimilarity)
        {
            QueryMatchAt = queryMatchAt;
            TrackMatchAt = trackMatchAt;
            HammingSimilarity = hammingSimilarity;
        }

        public float QueryMatchAt { get; }

        public float TrackMatchAt { get; }

        public int HammingSimilarity { get; }
    }
}