namespace SoundFingerprinting.Command
{
    using SoundFingerprinting.Query;

    public class NoPassRealtimeResultEntryFilter : IRealtimeResultEntryFilter
    {
        public bool Pass(ResultEntry entry, bool canContinueInTheNextQuery)
        {
            return false;
        }
    }
}