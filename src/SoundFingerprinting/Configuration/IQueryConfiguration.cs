namespace SoundFingerprinting.Configuration
{
    public interface IQueryConfiguration
    {
        int ThresholdVotes { get; }

        int MaximumNumberOfTracksToReturnAsResult { get; }
    }
}