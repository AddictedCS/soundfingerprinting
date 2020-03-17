namespace SoundFingerprinting.Command
{
    using SoundFingerprinting.Query;

    public interface IRealtimeResultEntryFilter
    {
        bool Pass(ResultEntry entry);
    }
}