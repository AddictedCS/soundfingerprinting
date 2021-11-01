namespace SoundFingerprinting.Command
{
    /// <summary>
    ///  Completed match realtime result entry filter.
    /// </summary>
    /// <typeparam name="T">Instance of T.</typeparam>
    public class CompletedRealtimeMatchResultEntryFilter<T> : IRealtimeResultEntryFilter<T>
    {
        /// <summary>
        ///  Passes only when a match is considered completed. A completed match can't continue in the next query.
        /// </summary>
        /// <param name="entry">Entry to analyze.</param>
        /// <param name="canContinueInTheNextQuery">A flag indicating whether the match can continue in the next query.</param>
        /// <returns>True when the match can't continue in the next query, otherwise false.</returns>
        public bool Pass(T entry, bool canContinueInTheNextQuery)
        {
            return !canContinueInTheNextQuery;
        }
    }
}