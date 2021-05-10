namespace SoundFingerprinting.Command
{
    using SoundFingerprinting.Query;

    public interface IRealtimeResultEntryFilter
    {
        /// <summary>
        ///  Checks if provided result entry passes the filter.
        /// </summary>
        /// <param name="entry">Instance of <see cref="ResultEntry"/> to check.</param>
        /// <param name="canContinueInTheNextQuery">Flag indicating whether the result entry can continue in the next query.</param>
        /// <returns>True if the result entry passes the filter, otherwise false.</returns>
        bool Pass(ResultEntry entry, bool canContinueInTheNextQuery);
    }
}