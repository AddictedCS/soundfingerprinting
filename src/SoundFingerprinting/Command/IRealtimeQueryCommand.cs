namespace SoundFingerprinting.Command
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///  Contract for realtime query command.
    /// </summary>
    public interface IRealtimeQueryCommand
    {
        /// <summary>
        ///  Query in realtime.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel query.</param>
        /// <returns>Total sum of queried seconds.</returns>
        Task<double> Query(CancellationToken cancellationToken);
    }
}