namespace SoundFingerprinting.LCS
{
    internal class Coverage
    {
        public Coverage(
            double sourceMatchStartsAt,
            double sourceMatchLength,
            double sourceCoverageLength,
            double originMatchStartsAt,
            double trackStartsAt)
        {
            SourceMatchStartsAt = sourceMatchStartsAt;
            SourceMatchLength = sourceMatchLength;
            OriginMatchStartsAt = originMatchStartsAt;
            TrackStartsAt = trackStartsAt;
            SourceCoverageLength = sourceCoverageLength;
        }

        public double SourceMatchStartsAt { get; }

        public double SourceMatchLength { get; }

        public double SourceCoverageLength { get; }

        public double OriginMatchStartsAt { get; }

        public double TrackStartsAt { get; }
    }
}
