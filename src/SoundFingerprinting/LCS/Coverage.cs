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

        public double SourceMatchStartsAt { get; private set; }

        public double SourceMatchLength { get; private set; }

        public double OriginMatchStartsAt { get; private set; }
    }
}
