namespace SoundFingerprinting.Command
{
    public class PassThroughRealtimeResultEntryFilter<T> : IRealtimeResultEntryFilter<T>
    {
        public bool Pass(T entry, bool canContinueInTheNextQuery)
        {
            return true;
        }
    }
}