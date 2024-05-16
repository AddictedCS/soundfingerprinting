namespace SoundFingerprinting.LCS
{
    using SoundFingerprinting.Query;

    internal record MaxAt(int Length, MatchedWith MatchedWith)
    {
        public int Length { get; } = Length;

        public MatchedWith MatchedWith { get; } = MatchedWith;

        public float QueryTrackDistance => System.Math.Abs(MatchedWith.QueryMatchAt - MatchedWith.TrackMatchAt);
    }
}