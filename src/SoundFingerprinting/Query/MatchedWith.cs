namespace SoundFingerprinting.Query
{
    internal class MatchedWith
    {
        public MatchedWith(float queryAt, float resultAt, int hammingSimilarity)
        {
            QueryAt = queryAt;
            ResultAt = resultAt;
            HammingSimilarity = hammingSimilarity;
        }

        public float QueryAt { get; }

        public float ResultAt { get; }

        public int HammingSimilarity { get; }
    }
}