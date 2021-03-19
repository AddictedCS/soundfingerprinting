namespace SoundFingerprinting.Command
{
    public interface IRealtimeSource
    {
        IWithRealtimeQueryConfiguration From(IRealtimeCollection realtimeCollection);
    }
}