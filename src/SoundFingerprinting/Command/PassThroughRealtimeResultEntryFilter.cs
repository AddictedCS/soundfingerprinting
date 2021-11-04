namespace SoundFingerprinting.Command
{
    using SoundFingerprinting.Query;

    /// <summary>
    ///  Pass through realtime result entry filter.
    /// </summary>
    public class PassThroughRealtimeResultEntryFilter : IRealtimeResultEntryFilter
    {
        /// <summary>
        ///  Always returns true.
        /// </summary>
        /// <param name="entry">Instance of <see cref="ResultEntry"/> class.</param>
        /// <param name="canContinueInTheNextQuery">A flag indicating whether the <see cref="ResultEntry"/> can continue in the next query.</param>
        /// <returns>True.</returns>
        public bool Pass(AVResultEntry entry, bool canContinueInTheNextQuery)
        {
            return true;
        }
    }
}