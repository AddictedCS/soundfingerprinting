namespace SoundFingerprinting.LCS
{
    internal class Coverage
    {
        public Coverage(double sourceMatchStartsAt, double sourceMatchLength, double originMatchStartsAt)
        {
            SourceMatchStartsAt = sourceMatchStartsAt;
            SourceMatchLength = sourceMatchLength;
            OriginMatchStartsAt = originMatchStartsAt;
        }

        public double SourceMatchStartsAt { get; }

        public double SourceMatchLength { get; }

        public double OriginMatchStartsAt { get; }
    }
}
