namespace SoundFingerprinting.Command
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IRealtimeQueryCommand
    {
        /// <summary>
        ///  Query in realtime
        /// </summary>
        /// <param name="cancellationToken">Token to cancel continuous query</param>
        /// <returns>Queried number of seconds</returns>
        Task<double> Query(CancellationToken cancellationToken);
    }
}