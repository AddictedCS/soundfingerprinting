namespace SoundFingerprinting.Builder
{
    using SoundFingerprinting.Command;

    /// <summary>
    ///  Query command builder interface.
    /// </summary>
    public interface IQueryCommandBuilder
    {
        /// <summary>
        ///  Start building a query command.
        /// </summary>
        /// <returns>Instance of <see cref="IQuerySource"/> that allows selecting a source of the fingerprints.</returns>
        IQuerySource BuildQueryCommand();

        /// <summary>
        ///  Starts building a realtime query command.
        /// </summary>
        /// <returns>Instance of <see cref="IRealtimeSource"/> that allows selecting a source of the realtime samples or files.</returns>
        IRealtimeSource BuildRealtimeQueryCommand();
    }
}