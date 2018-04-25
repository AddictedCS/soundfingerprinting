namespace SoundFingerprinting.LCS
{
    internal class Coverage
    {
        public Coverage(double sourceMatchStartsAt, double sourceMatchLength, double originMatchStartsAt, double trackStartsAt)
        {
            SourceMatchStartsAt = sourceMatchStartsAt;
            SourceMatchLength = sourceMatchLength;
            OriginMatchStartsAt = originMatchStartsAt;
            TrackStartsAt = trackStartsAt;
        }

        public double SourceMatchStartsAt { get; }

        public double SourceMatchLength { get; }

        public double OriginMatchStartsAt { get; }

        public double TrackStartsAt { get; }
    }
}
