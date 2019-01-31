namespace SoundFingerprinting.Query
{
    public class MatchedWith
    {
        public MatchedWith(float queryMatchAt, float resultAt, int hammingSimilarity)
        {
            QueryMatchAt = queryMatchAt;
            ResultAt = resultAt;
            HammingSimilarity = hammingSimilarity;
        }

        public float QueryMatchAt { get; }

        public float ResultAt { get; }

        public int HammingSimilarity { get; }
    }
}