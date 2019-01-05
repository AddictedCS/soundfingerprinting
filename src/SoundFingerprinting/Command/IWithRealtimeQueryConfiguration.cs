namespace SoundFingerprinting.Command
{
    public interface IWithRealtimeQueryConfiguration
    {
        IUsingRealtimeQueryServices WithRealtimeQueryConfig(RealtimeQueryConfiguration realtimeQueryConfiguration);
    }
}