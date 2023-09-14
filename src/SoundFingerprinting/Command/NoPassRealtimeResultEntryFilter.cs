namespace SoundFingerprinting.Command
{
    using SoundFingerprinting.Query;

    /// <summary>
    ///  No pass realtime result entry filter.
    /// </summary>
    internal class NoPassRealtimeResultEntryFilter : IRealtimeResultEntryFilter
    {
        /// <summary>
        ///  Never pass result entry filter.
        /// </summary>
        /// <param name="entry">Instance of <see cref="ResultEntry"/> class.</param>
        /// <param name="canContinueInTheNextQuery">A flag indicating whether the <see cref="ResultEntry"/> can continue in the next query.</param>
        /// <returns>False.</returns>
        public bool Pass(AVResultEntry entry, bool canContinueInTheNextQuery)
        {
            return false;
        }
    }
}