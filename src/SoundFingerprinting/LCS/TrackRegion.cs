namespace SoundFingerprinting.LCS
{
    internal class TrackRegion
    {
        public TrackRegion( int startAt, int endAt)
        {
            EndAt = endAt;
            StartAt = startAt;
        }

        public int StartAt { get; }

        public int EndAt { get; }
    }
}